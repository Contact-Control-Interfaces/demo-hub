# demo-hub

The Contact CI demo hub: a Unity 2022.3 (HDRP) VR project with the Maestro glove
demos. It uses two private submodules, `Packages/com.contactci.unity` and
`Packages/ultraleap` (ultraleap-fork), so clone with:

```
git clone --recurse-submodules https://github.com/Contact-Control-Interfaces/demo-hub.git
```

## Versions and releases

Versioning follows the dev-handbook's
[versioning standard](https://github.com/Contact-Control-Interfaces/dev-handbook/blob/main/releases/versioning.md).
The table at the top of [`.github/workflows/ci.yml`](.github/workflows/ci.yml) has the
details.

- **The declared version** is Player Settings > Version (`bundleVersion` in
  `ProjectSettings/ProjectSettings.asset`): the plain `X.Y.Z` that `staging` is working
  towards. CI adds the suffix (`-ci.N`, `-beta.N`, `-rc.N`) and never writes the file.
- **Every build records itself.** The player logs its full version, commit, build id
  and Unity version at startup (`BuildInfo` in the log). A local build reports
  `X.Y.Z-dev+<sha>`, with `.dirty` if tracked files had uncommitted changes.
- **Branches.** Feature branches merge into `staging` through pull requests. `staging`
  is promoted to `main` only by a fast-forward to a release candidate.

Only a person creates tags. To cut a release candidate, tag a commit on `staging`:

```
git fetch origin
git tag -a v3.1.0-rc.1 -m "3.1.0-rc.1" origin/staging
git push origin v3.1.0-rc.1
```

CI checks that the tag's `X.Y.Z` matches the declared version and that the commit is
on `staging`, builds the player and publishes a GitHub prerelease. A broken rc is
fixed with `rc.2`, never by moving the tag.

To release, fast-forward `main` to the tested rc commit and tag that same commit:

```
git fetch origin
git checkout main
git merge --ff-only v3.1.0-rc.1
git push origin main
git tag -a v3.1.0 -m "3.1.0" v3.1.0-rc.1
git push origin v3.1.0
```

Then bump Player Settings > Version on `staging` to the next version (e.g. `3.1.1`),
so later builds head towards it. Tags and released versions are permanent.

You can check the version rules locally:

```
./tools/resolve-version.ps1 -Ref refs/tags/v3.1.0-rc.1
```

### CI settings

- **Submodules:** the org GitHub App (`SUBMODULE_APP_ID` variable and
  `SUBMODULE_APP_PRIVATE_KEY` secret, set at org level) reads the private
  submodules.
- **Unity license:** the player build needs a Unity license in the repo's secrets.
  Set `UNITY_LICENSE` (contents of a Personal `.ulf` file) or `UNITY_SERIAL` (Pro),
  plus `UNITY_EMAIL` and `UNITY_PASSWORD`. Without them CI still checks versions and
  submodules but skips the build, and release tags fail.
