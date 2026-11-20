#!/usr/bin/env bash
# unity-build-lock.sh acquire|release
#
# One Unity player build at a time across every repository that builds with the Unity Build Server (one WireGuard CI
# key, one Build Server seat; see the Licensing Server runbook). GitHub's `concurrency:` groups only cover one
# repository, so the lock is a branch in a shared repository (unity-ci-locks) instead.
#
# The branch LOCK_BRANCH always points at the latest lock commit, and its message is the lock's state:
#   "held by <repository> run <id> job <job> attempt <n>"   held since the commit's date
#   "free (...)"                                            free (also when the branch doesn't exist yet)
#
# Every change is a new commit whose parent is the lock commit it replaces, pushed as a fast-forward: GitHub refuses
# the update if the branch has moved meanwhile. So taking the lock, taking over a stale one and releasing it are each
# one compare-and-swap, and two jobs can never both succeed from the same state. The first lock creates the branch,
# which GitHub also refuses if it already exists.
#
# A held lock is taken over when:
#   * it was taken more than LOCK_LEASE_MINUTES ago (its holder's runner was lost, so release never ran). The lease
#     starts when the lock is taken, not when waiting starts. Keep each build step's timeout-minutes well below it;
#   * or it was taken by an earlier attempt of this same job: a re-run only starts once that attempt has finished.
#
# Copied as-is into each repository that uses it (demo-hub, maestro-vr-teleop); keep the copies the same.
#
# Environment:
#   LOCK_REPO                owner/name of the repository that holds the lock
#   LOCK_APP_ID              the unity-ci-locks GitHub App's ID, and
#   LOCK_APP_PRIVATE_KEY     its private key (PEM): the script gets its own installation tokens and renews them
#                            before they expire (they last an hour), so a long wait keeps working. Or instead:
#   GH_TOKEN                 a token with Contents: write on LOCK_REPO (local tests; never renewed)
#   LOCK_BRANCH              default ci-locks/unity-build-server
#   LOCK_LEASE_MINUTES       default 120
#   LOCK_WAIT_MINUTES        give up waiting after this long (default 180)
#   LOCK_POLL_SECONDS        default 20
#   LOCK_STATE_FILE          where acquire records its lock commit for release (default $RUNNER_TEMP/unity-build-lock)
set -euo pipefail

: "${LOCK_REPO:?LOCK_REPO is not set}"
branch="${LOCK_BRANCH:-ci-locks/unity-build-server}"
lease_minutes="${LOCK_LEASE_MINUTES:-120}"
wait_minutes="${LOCK_WAIT_MINUTES:-180}"
poll_seconds="${LOCK_POLL_SECONDS:-20}"
state_file="${LOCK_STATE_FILE:-${RUNNER_TEMP:-/tmp}/unity-build-lock}"
api="repos/$LOCK_REPO/git"
github_api="${GITHUB_API_URL:-https://api.github.com}"

job_id="${GITHUB_REPOSITORY:-local} run ${GITHUB_RUN_ID:-0} job ${GITHUB_JOB:-local}"
holder="$job_id attempt ${GITHUB_RUN_ATTEMPT:-1}"

# ------------------------------------------------------------------ tokens

token_expires=0

b64url() { openssl base64 -A | tr '+/' '-_' | tr -d '='; }

# A JWT for the App (RS256, valid 9 minutes, backdated 1 minute for clock drift).
app_jwt() {
  local now header payload unsigned key signature status=0
  now="$(date +%s)"
  header="$(printf '%s' '{"alg":"RS256","typ":"JWT"}' | b64url)"
  payload="$(printf '{"iat":%d,"exp":%d,"iss":"%s"}' "$((now - 60))" "$((now + 540))" "$LOCK_APP_ID" | b64url)"
  unsigned="$header.$payload"
  # The key goes to a file only this user can read, for as long as openssl needs it.
  key="$(umask 077 && mktemp)"
  printf '%s\n' "$LOCK_APP_PRIVATE_KEY" > "$key"
  signature="$(printf '%s' "$unsigned" | openssl dgst -sha256 -binary -sign "$key" | b64url)" || status=$?
  rm -f "$key"
  if [ "$status" -ne 0 ]; then
    echo "::error::Can't sign the App token request: LOCK_APP_PRIVATE_KEY isn't a valid PEM private key." >&2
    return 1
  fi
  printf '%s.%s' "$unsigned" "$signature"
}

# Makes sure GH_TOKEN is valid for at least 10 more minutes.
ensure_token() {
  if [ -z "${LOCK_APP_ID:-}" ]; then
    : "${GH_TOKEN:?Set LOCK_APP_ID and LOCK_APP_PRIVATE_KEY, or GH_TOKEN}"
    return 0
  fi
  : "${LOCK_APP_PRIVATE_KEY:?LOCK_APP_PRIVATE_KEY is not set}"
  if [ "$(date +%s)" -lt "$((token_expires - 600))" ]; then
    return 0
  fi

  local jwt response installation
  jwt="$(app_jwt)"
  if ! response="$(curl -sS --fail-with-body -H "Authorization: Bearer $jwt" -H "Accept: application/vnd.github+json" \
      "$github_api/repos/$LOCK_REPO/installation")"; then
    echo "::error::The App (LOCK_APP_ID) isn't installed on $LOCK_REPO, or its private key doesn't match: $response"
    return 1
  fi
  installation="$(jq -r .id <<< "$response")"
  if ! response="$(curl -sS --fail-with-body -X POST -H "Authorization: Bearer $jwt" \
      -H "Accept: application/vnd.github+json" "$github_api/app/installations/$installation/access_tokens" \
      -d "$(jq -nc --arg repo "${LOCK_REPO#*/}" '{repositories: [$repo], permissions: {contents: "write"}}')")"; then
    echo "::error::Can't get a token for $LOCK_REPO from the App (it needs Contents: write there): $response"
    return 1
  fi
  GH_TOKEN="$(jq -r .token <<< "$response")"
  export GH_TOKEN
  if [ -n "${GITHUB_ACTIONS:-}" ]; then
    echo "::add-mask::$GH_TOKEN"
  fi
  token_expires="$(date -d "$(jq -r .expires_at <<< "$response")" +%s)"
  echo "Got a lock token for $LOCK_REPO, valid until $(jq -r .expires_at <<< "$response")."
}

# ------------------------------------------------------------------ the lock branch

tree=""
lock_sha=""
lock_message=""
lock_date=""

# Sets lock_sha (empty: no branch yet), lock_message and lock_date from the branch's latest commit.
read_lock() {
  local out info
  lock_sha=""
  lock_message=""
  lock_date=""
  if ! out="$(gh api "$api/ref/heads/$branch" --jq .object.sha 2>&1)"; then
    if grep -q 'HTTP 404' <<< "$out"; then
      return 0
    fi
    echo "::error::Can't read the lock $LOCK_REPO:$branch: $out"
    return 1
  fi
  lock_sha="$out"
  info="$(gh api "$api/commits/$lock_sha" --jq '[.message, .committer.date] | @tsv')"
  lock_message="${info%%$'\t'*}"
  lock_date="${info##*$'\t'}"
}

# A new lock commit with `message` on top of `parent` (empty: a root commit). Prints its sha.
new_commit() {
  local message=$1 parent=$2
  if [ -z "$tree" ]; then
    # Any existing tree will do: the lock repository's default branch.
    tree="$(gh api "repos/$LOCK_REPO/commits/HEAD" --jq .commit.tree.sha)"
  fi
  if [ -n "$parent" ]; then
    gh api "$api/commits" -f "message=$message" -f "tree=$tree" -f "parents[]=$parent" --jq .sha
  else
    gh api "$api/commits" -f "message=$message" -f "tree=$tree" --jq .sha
  fi
}

# Moves the branch from `from` (empty: it doesn't exist) to `to`, only if it is still at `from`.
# Returns 1 if another job moved it first.
move_lock() {
  local from=$1 to=$2 out
  if [ -z "$from" ]; then
    out="$(gh api "$api/refs" -f "ref=refs/heads/$branch" -f "sha=$to" --silent 2>&1)" && return 0
  else
    out="$(gh api -X PATCH "$api/refs/heads/$branch" -f "sha=$to" -F force=false --silent 2>&1)" && return 0
  fi
  if grep -q 'HTTP 422' <<< "$out"; then
    return 1 # the branch moved (or was created) since it was read
  fi
  echo "::error::Can't update the lock $LOCK_REPO:$branch: $out"
  exit 1
}

# ------------------------------------------------------------------ commands

acquire() {
  local start waited age stale commit
  start="$(date +%s)"
  rm -f "$state_file"
  while true; do
    ensure_token
    read_lock

    stale=""
    if [[ "$lock_message" == "held by "* ]]; then
      if [ "$lock_message" = "held by $holder" ]; then
        printf '%s\n' "$lock_sha" > "$state_file"
        echo "This job already holds the Unity build lock."
        return 0
      fi
      age=$(( ($(date +%s) - $(date -d "$lock_date" +%s)) / 60 ))
      if [ "$age" -ge "$lease_minutes" ]; then
        stale="it was taken $age minutes ago (lease $lease_minutes)"
      elif [[ "$lock_message" == "held by $job_id attempt "* ]]; then
        stale="an earlier attempt of this job took it"
      fi
    fi

    if [[ "$lock_message" != "held by "* ]] || [ -n "$stale" ]; then
      # A fresh commit for each try, so the lease starts now. Recorded before the swap, so release can tell the lock
      # is ours even if this step is cancelled right after it wins.
      commit="$(new_commit "held by $holder" "$lock_sha")"
      printf '%s\n' "$commit" > "$state_file"
      if move_lock "$lock_sha" "$commit"; then
        [ -n "$stale" ] && echo "::warning::Took over the Unity build lock from \"${lock_message#held by }\": $stale."
        echo "Took the Unity build lock ($LOCK_REPO:$branch) for $holder."
        return 0
      fi
      rm -f "$state_file"
      echo "Another job changed the lock first; checking again."
      continue
    fi

    echo "Waiting for the Unity build lock, held by \"${lock_message#held by }\" for $age minutes."
    waited=$(( ($(date +%s) - start) / 60 ))
    if [ "$waited" -ge "$wait_minutes" ]; then
      echo "::error::Gave up after waiting $waited minutes for the Unity build lock ($LOCK_REPO:$branch)."
      return 1
    fi
    sleep "$poll_seconds"
  done
}

release() {
  if [ ! -s "$state_file" ]; then
    echo "This job never tried to take the Unity build lock."
    return 0
  fi
  local ours commit
  ours="$(cat "$state_file")"
  ensure_token
  read_lock
  if [ "$lock_sha" != "$ours" ]; then
    echo "This job doesn't hold the Unity build lock (it was never taken, or was taken over)."
    rm -f "$state_file"
    return 0
  fi
  commit="$(new_commit "free (released by $holder)" "$ours")"
  if move_lock "$ours" "$commit"; then
    echo "Released the Unity build lock ($LOCK_REPO:$branch)."
  else
    echo "::warning::The Unity build lock was taken over before this job released it."
  fi
  rm -f "$state_file"
}

if [ "${BASH_SOURCE[0]}" = "$0" ]; then
  case "${1:-}" in
    acquire) acquire ;;
    release) release ;;
    *) echo "usage: $0 acquire|release" >&2; exit 2 ;;
  esac
fi
