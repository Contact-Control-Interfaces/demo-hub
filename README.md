# Contact CI Demo Hub

The Demo Hub is a VR showcase for [Contact CI](https://contact.ci)'s Maestro
haptic gloves. Put on a headset and the gloves, and try a set of short demos
that show off the gloves' hand tracking, vibration and force feedback.

The demos:

- Interaction Panel
- Paint
- Shapes
- Vibration Orbs
- Force Feedback
- Apple Tree

## What you need

- **A Windows 10 or 11 PC (64-bit)** that can run PC VR.
- **A PC VR headset** connected to that PC, with its OpenXR runtime active. The
  Demo Hub is a Windows app, not a standalone headset app.
- **One or two Maestro gloves**, connected to the PC.
- **The Contact CI Windows Service**, which connects to the gloves and sends
  them haptics. See below.
- **The .NET 8 Desktop Runtime**, for the launcher. If it's missing, the
  launcher offers to download it when you start it.

## Install the Contact CI software

The Demo Hub talks to the gloves through the Contact CI Windows Service. Install
it once, before the first run:

1. Download `ContactCiInstaller.exe` from the latest release of
   [windows-installer](https://github.com/Contact-Control-Interfaces/windows-installer/releases/latest)
   ([direct link](https://github.com/Contact-Control-Interfaces/windows-installer/releases/latest/download/ContactCiInstaller.exe)).
2. Run it and follow its steps. Windows asks for administrator permission,
   since it installs a service.

The installer also installs the .NET Framework 4.8 if your PC needs it, and the
**Contact CI Manager**. The Manager doesn't need to be running for the gloves
to work: the service runs on its own. It's a useful front end for the service,
though. It shows the connected gloves and the service's logs, and lets you view
and control haptics on the gloves directly.

## Install and run

1. Download the latest release from the
   [Releases page](https://github.com/Contact-Control-Interfaces/demo-hub/releases).
   The zip is `DemoHub-<version>-win64.zip` (`DemoHub.zip` for 3.0.0 and
   earlier).
2. Optionally, check the download against the release's `SHA256SUMS`:

   ```powershell
   Get-FileHash .\DemoHub-<version>-win64.zip -Algorithm SHA256
   ```

3. Unzip it to a folder of its own. Keep the files together: the launcher
   starts the Demo Hub from the same folder.
4. Start your headset's PC VR mode and connect the gloves. The Contact CI
   Manager shows when the service sees them.
5. Run `DemoLauncher.exe`, then:
   - **Scenes:** pick which demos appear in the Demo Hub's menu.
   - **Start Scene:** pick the demo the Demo Hub opens first. If Apple Tree is
     one of the scenes, the Demo Hub always starts there.
   - **One/Two hands:** for Apple Tree, pick how many gloves must be connected
     before the tree appears.
6. Click **Save & Launch**. The launcher saves your choices to `config.json`
   beside it, starts `DemoHub.exe` and closes. **Save Configuration** saves
   without starting.

## If something goes wrong

If the gloves don't respond, open the Contact CI Manager. It shows whether the
service sees each glove, and its logs show why one isn't connecting. You can
also try haptics on a glove from there, without the Demo Hub.

The Demo Hub writes a log at:

```
%USERPROFILE%\AppData\LocalLow\ContactCI\Demo Hub\Player.log
```

Near the top, a `BuildInfo` line gives the exact version and build, for
example `Demo Hub 3.1.0 (commit …, build …, Unity 2022.3.15f1, Mono2x)`.
When you contact [support@contact.ci](mailto:support@contact.ci), include that
line, and the log if you can.

## License

Your use of the Demo Hub is governed by the Contact CI SDK EULA
([`EULA.md`](EULA.md), also at the top of every release zip). The third-party
components it includes, and their licenses, are listed in
[`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md). The source code in this
repository is proprietary ([`LICENSE.md`](LICENSE.md)).

Contact CI developers: see [`docs/internal.md`](docs/internal.md) for
building, versions, releases and CI.
