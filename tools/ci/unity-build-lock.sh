#!/usr/bin/env bash
# unity-build-lock.sh acquire|release
#
# One Unity player build at a time across every repository that builds with the Unity Build Server (one WireGuard CI
# key, one Build Server seat; see the Licensing Server runbook). GitHub's `concurrency:` groups only cover one
# repository, so the lock is a branch in a shared repository instead:
#
#   refs/heads/<LOCK_BRANCH> exists  = the lock is held. It points at a commit whose message names the holder
#                                      (repository, run, attempt, job) and whose date is when it was taken.
#
# Creating a ref that already exists fails (HTTP 422), so only one job can take the lock. A waiter polls every
# LOCK_POLL_SECONDS. A lock is taken over when:
#   * it is older than LOCK_LEASE_MINUTES (its holder's runner was lost, so release never ran), or
#   * it was taken by an earlier attempt of this same job: a re-run only starts once that attempt has finished.
# Give the build job a timeout-minutes below the lease, so a live build never outlasts its lock.
#
# Copied as-is into each repository that uses it (demo-hub, maestro-vr-teleop); keep the copies the same.
#
# Environment:
#   GH_TOKEN             token with Contents: write on LOCK_REPO
#   LOCK_REPO            owner/name of the repository that holds the lock
#   LOCK_BRANCH          default ci-locks/unity-build-server
#   LOCK_LEASE_MINUTES   default 120
#   LOCK_WAIT_MINUTES    give up waiting after this long (default 180)
#   LOCK_POLL_SECONDS    default 20
#   LOCK_STATE_FILE      where acquire records the lock commit for release (default $RUNNER_TEMP/unity-build-lock)
set -euo pipefail

: "${GH_TOKEN:?GH_TOKEN is not set}"
: "${LOCK_REPO:?LOCK_REPO is not set}"
branch="${LOCK_BRANCH:-ci-locks/unity-build-server}"
lease_minutes="${LOCK_LEASE_MINUTES:-120}"
wait_minutes="${LOCK_WAIT_MINUTES:-180}"
poll_seconds="${LOCK_POLL_SECONDS:-20}"
state_file="${LOCK_STATE_FILE:-${RUNNER_TEMP:-/tmp}/unity-build-lock}"
api="repos/$LOCK_REPO/git"

job_id="$GITHUB_REPOSITORY run $GITHUB_RUN_ID job $GITHUB_JOB"
holder="$job_id attempt ${GITHUB_RUN_ATTEMPT:-1}"

# The lock commit's sha, or nothing when the lock is free. Fails on anything but "not found".
current_lock() {
  local out
  if out="$(gh api "$api/ref/heads/$branch" --jq .object.sha 2>&1)"; then
    printf '%s' "$out"
  elif grep -q 'HTTP 404' <<< "$out"; then
    return 0
  else
    echo "::error::Can't read the lock $LOCK_REPO:$branch: $out" >&2
    return 1
  fi
}

acquire() {
  # A root commit naming the holder, on the lock repository's default branch tree (any existing tree will do).
  local tree commit
  tree="$(gh api "repos/$LOCK_REPO/commits/HEAD" --jq .commit.tree.sha)"
  commit="$(gh api "$api/commits" -f "message=$holder" -f "tree=$tree" --jq .sha)"
  # Recorded before trying, so release can tell the lock is ours even if this step is cancelled right after it wins.
  printf '%s\n' "$commit" > "$state_file"

  local start out sha info message taken age waited
  start="$(date +%s)"
  while true; do
    if out="$(gh api "$api/refs" -f "ref=refs/heads/$branch" -f "sha=$commit" --silent 2>&1)"; then
      echo "Took the Unity build lock ($LOCK_REPO:$branch) for $holder."
      return 0
    fi
    if ! grep -q 'HTTP 422' <<< "$out"; then
      echo "::error::Can't take the lock $LOCK_REPO:$branch: $out"
      return 1
    fi

    sha="$(current_lock)"
    if [ -n "$sha" ]; then
      info="$(gh api "$api/commits/$sha" --jq '[.message, .committer.date] | @tsv')"
      message="${info%%$'\t'*}"
      taken="${info##*$'\t'}"
      age=$(( ($(date +%s) - $(date -d "$taken" +%s)) / 60 ))

      local stale=""
      if [ "$age" -ge "$lease_minutes" ]; then
        stale="it is $age minutes old (lease $lease_minutes)"
      elif [[ "$message" == "$job_id attempt "* && "$message" != "$holder" ]]; then
        stale="an earlier attempt of this job took it"
      fi

      # Re-read just before deleting, so a lock another waiter has meanwhile taken over is left alone.
      if [ -n "$stale" ] && [ "$(current_lock)" = "$sha" ]; then
        echo "::warning::Taking over the Unity build lock held by \"$message\": $stale."
        gh api -X DELETE "$api/refs/heads/$branch" --silent || true
        continue
      fi
      echo "Waiting for the Unity build lock, held by \"$message\" for $age minutes."
    fi

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
  local ours sha
  ours="$(cat "$state_file")"
  sha="$(current_lock)"
  if [ "$sha" = "$ours" ]; then
    gh api -X DELETE "$api/refs/heads/$branch" --silent
    echo "Released the Unity build lock ($LOCK_REPO:$branch)."
  elif [ -z "$sha" ]; then
    echo "The Unity build lock is free; this job didn't hold it."
  else
    echo "The Unity build lock is held by another job; this job didn't hold it."
  fi
  rm -f "$state_file"
}

case "${1:-}" in
  acquire) acquire ;;
  release) release ;;
  *) echo "usage: $0 acquire|release" >&2; exit 2 ;;
esac
