# demo-hub: internal notes

For Contact CI developers working on demo-hub. The [README](../README.md) is for
people using the demo.

## Cloning

demo-hub is a Unity 2022.3 (HDRP) project. It uses two private submodules,
`Packages/com.contactci.unity` and `Packages/ultraleap` (ultraleap-fork), so
clone with:

```
git clone --recurse-submodules https://github.com/Contact-Control-Interfaces/demo-hub.git
```

You need read access to both submodules' repositories.

## Versions

Versioning follows the dev-handbook's
[versioning standard](https://github.com/Contact-Control-Interfaces/dev-handbook/blob/main/releases/versioning.md).
The table at the top of [`.github/workflows/ci.yml`](../.github/workflows/ci.yml)
has the details.

- **The declared version** is Player Settings > Version (`bundleVersion` in
  `ProjectSettings/ProjectSettings.asset`): the plain `X.Y.Z` that `staging` is
  working towards. CI adds the suffix (`-ci.N`, `-beta.N`, `-rc.N`) and never
  writes the file.
- **Every build records itself.** The player logs its full version, commit,
  build id and Unity version at startup (`BuildInfo` in the log). A local build
  reports `X.Y.Z-dev+<sha>`, with `.dirty` if tracked files had uncommitted
  changes.
- **Branches.** Feature branches merge into `staging` through pull requests.
  `staging` is promoted to `main` only by a fast-forward to a release candidate.

You can check the version rules locally:

```
./tools/resolve-version.ps1 -Ref refs/tags/v3.1.0-rc.1
```

## Releases

Only a person creates tags, and only a person publishes a release. For each rc
or release tag, CI creates a draft GitHub prerelease or release, published by a
person: the draft goes on the
[Releases page](https://github.com/Contact-Control-Interfaces/demo-hub/releases)
with the zip, `SHA256SUMS`, an SBOM and generated notes, and the run summary
links it. Drafts are visible only to people with write access to this
repository. A person checks the draft and publishes it. CI never changes a
release once it is published.

To cut a release candidate, tag a commit on `staging`:

```
git fetch origin
git tag -a v3.1.0-rc.1 -m "3.1.0-rc.1" origin/staging
git push origin v3.1.0-rc.1
```

CI checks that the tag's `X.Y.Z` matches the declared version and that the
commit is on `staging`, builds the player and creates a draft prerelease.
Download the zip from the draft and test it, then publish the draft from the
Releases page with "Set as a pre-release" left ticked. A broken rc is fixed with
`rc.2`, never by moving the tag.

To release, fast-forward `main` to the tested rc commit and tag that same
commit:

```
git fetch origin
git checkout main
git merge --ff-only v3.1.0-rc.1
git push origin main
git tag -a v3.1.0 -m "3.1.0" v3.1.0-rc.1
git push origin v3.1.0
```

CI creates a draft release with notes since the previous release. Check it,
then publish it from the Releases page with "Set as the latest release" ticked.
A draft can't be marked Latest, so CI can't do this for you.

Then bump Player Settings > Version on `staging` to the next version (e.g.
`3.1.1`), so later builds head towards it. Tags and released versions are
permanent.

## The launcher

The player zip also holds `DemoLauncher.exe` from
[demo-launcher](https://github.com/Contact-Control-Interfaces/demo-launcher),
beside `DemoHub.exe`. The launcher writes `config.json` next to itself and
starts `DemoHub.exe` from the same folder. Which release ships is pinned in
[`tools/demo-launcher.env`](../tools/demo-launcher.env): the tag, and the
SHA-256 of `DemoLauncher.exe` itself (not of the zip it comes in). To ship a new
launcher, change both lines. A build fails if the downloaded exe doesn't match
the pin.

## The EULA and notices

The source in this repository is proprietary ([`LICENSE.md`](../LICENSE.md),
the handbook's `licensing/templates/proprietary.md`). What the player zip ships
is licensed under the Contact CI SDK EULA, so the zip carries
[`EULA.md`](../EULA.md) at its top, as the dev-handbook's licensing standard asks
of release archives. It's the handbook's `licensing/templates/sdk-eula.md`,
copied unchanged; when the template changes, copy it again.

Beside it, [`THIRD-PARTY-NOTICES.md`](../THIRD-PARTY-NOTICES.md) carries the
notices and license texts the player's third-party parts require (the Ultraleap
plugin, protobuf and mingw-w64 in `libcci.dll`, the Meta SDK, fonts, sounds and
the rest). When you add or upgrade a package, plugin, font, sound or model,
update it. Freesound sounds and other assets need their license recorded too,
as `Assets/Sounds/SoundCredits.txt` does.

## CI settings

- **Submodules:** the org GitHub App (`SUBMODULE_APP_ID` variable and
  `SUBMODULE_APP_PRIVATE_KEY` secret, set at org level) reads the private
  submodules. It must also be installed on demo-launcher, whose releases the
  build reads for the launcher. A `SUBMODULE_TOKEN` PAT is the fallback for
  both.
- **Unity license:** the player build takes a floating Unity Build Server
  license from the company's Unity Licensing Server on AWS, through a WireGuard
  tunnel. It needs the `UNITY_LICENSING_SERVER` variable and the
  `UNITY_LICENSING_WIREGUARD` secret, set at org level. The server's setup and
  running are in the
  [Unity Licensing Server for CI](https://linear.app/contact-ci/document/unity-licensing-server-for-ci-unity-build-server-b8aceb1bf894)
  runbook in Linear (Unity SDK project). Without both settings, CI still checks
  versions and submodules but skips the build, and release tags fail. One
  player build runs at a time.
