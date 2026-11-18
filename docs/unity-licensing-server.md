# Unity Licensing Server for CI

CI builds the player with a floating **Unity Build Server** license, so no one's
personal Unity seat is used. The license comes from a **Unity Licensing Server**
(ULS) on a small EC2 instance. The ULS is never exposed to the internet: CI's
GitHub-hosted runner reaches it through a WireGuard tunnel, for the length of
the build job only.

```
GitHub runner ── WireGuard (UDP 51820) ──> EC2: wg0 10.66.0.1
  └ game-ci Unity container                    └ Unity Licensing Server
      takes a floating license,                    on the private IP, :8080
      builds, returns it
```

The Licensing Server listens on the instance's private IP (in the default VPC,
`172.31.x.y`), and the tunnel carries the runner's traffic to that address.
This page writes it as `<private IP>`. It's on the instance's **Details** tab,
under **Private IPv4 addresses**, and it stays the same across stops and
starts.

This page sets that up once. The CI side is already in
[`.github/workflows/ci.yml`](../.github/workflows/ci.yml) ("Connect to the
licensing server" and "Build"). The build is skipped until the
`UNITY_LICENSING_SERVER` variable and `UNITY_LICENSING_WIREGUARD` secret exist.

Unity's own documentation for the server is the
[Unity Licensing Server manual](https://docs.unity.com/en-us/licensing-server),
starting with its [quick start](https://docs.unity.com/en-us/licensing-server/quick-start-web).

## Before you start

- **Build Server seats.** The organization needs a Unity Build Server
  subscription with at least one seat. Build Server is sold separately from
  Industry seats, through Unity sales. One seat serves one build at a time,
  and CI runs one build at a time (see [Limits](#limits)).
- **Access.** You need an AWS account where you can create EC2 instances, IAM
  roles and security groups, and the AWS CLI with Session Manager plugin on
  your machine. You also need the Unity ID portal as an owner or manager of the
  organization, and admin on this GitHub repository.
- **Cost.** About $20 a month on demand in us-east-1 or us-east-2: a `t3.small` (about $15),
  20 GB of gp3 storage (about $2), and the Elastic IP's public IPv4 address
  (about $4).

## 1. Launch the instance

The ULS registration is bound to the machine it was set up on. Unity's status
output lists the bindings: the first network interface's MAC address, the
platform, the processor count and the machine name. **Choose the instance
once, and keep it.**

- Stopping and starting it is fine. That keeps its network interface, MAC
  address, private IP and hostname.
- Don't change its instance type, since the vCPU count is one of the bindings.
- Don't replace or terminate it, rename it, or attach its network interface
  elsewhere. Moving the server means Unity's migration process
  ([server administration](https://docs.unity.com/en-us/licensing-server/server-admin)).

In the EC2 console, **Launch instance**:

| Setting | Value |
|---|---|
| Name | `unity-licensing-server` |
| AMI | Click the **Ubuntu** tile, then **Ubuntu Server 24.04 LTS (HVM), SSD Volume Type**, 64-bit (x86). The wizard selects Amazon Linux by default, and this page's commands need Ubuntu |
| Instance type | `t3.small` |
| Key pair | None (you connect with Session Manager) |
| Network | The default VPC is fine, and so is subnet "No preference". **Auto-assign public IP: Enable** |
| Security group | New (or select it if it already exists), `unity-licensing-server`: **inbound UDP 51820 from 0.0.0.0/0 only**, no SSH. Outbound: leave the default |
| Storage | 20 GiB gp3, encrypted |
| Advanced: IAM instance profile | A role with the `AmazonSSMManagedInstanceCore` policy |
| Advanced: Metadata version | V2 only (token required) |
| Advanced: Termination protection | Enable |

Then:

1. **Elastic IPs** > **Allocate Elastic IP address**, then **Associate** it with
   the instance. Leave the private IP address empty, and leave reassociation
   unticked. This is the WireGuard endpoint; without it the public IP changes
   on every stop and start.
   - The Elastic IP replaces the automatically assigned public IP, so the
     instance ends up with only the Elastic IP.
   - Keep auto-assign on anyway. Without it the instance has no internet
     until the Elastic IP is associated, and the SSM agent and first-boot
     updates stall and may need a reboot to recover.
2. Optionally, have **Data Lifecycle Manager** take a daily snapshot of the
   instance's volume.

Port 51820 is open to everyone, but WireGuard doesn't answer any packet that
isn't signed by a known key, so it doesn't show up to a scan. Port 8080 is never
open in the security group.

To connect to the instance, the simplest way is the browser shell: in the EC2 console,
select the instance, then **Connect** > **Session Manager** > **Connect**.

- Paste with **Ctrl+Shift+V**.
- The session closes after 20 minutes idle. Reconnect and run `sudo -i`
  again; nothing is lost.

Or connect from your machine's terminal, giving the instance's region (its
Availability Zone without the last letter, e.g. `us-east-2` for `us-east-2a`):

```bash
aws ssm start-session --region <region> --target <instance id>
```

On Windows, run it from PowerShell or Windows Terminal, not Git Bash. In
Git Bash the session opens but every command hangs, even `ls`. If you use
Git Bash, put `winpty` in front of the command.

If it says the instance is not connected:

- Check that the region is right.
- Check that the instance has the IAM role (Details tab, **IAM Role**).
- Wait a few minutes after launch, or reboot the instance. Rebooting doesn't
  affect the machine binding.

`aws ssm describe-instance-information --region <region>` lists the instances
Session Manager can reach.

## 2. WireGuard on the server

Keep all keys on the server and in GitHub's secret. Don't paste them into
chat, tickets or this repository.

```bash
sudo apt-get update && sudo apt-get install -y wireguard
sudo -i
cd /etc/wireguard && umask 077
wg genkey | tee server.key | wg pubkey > server.pub
wg genkey | tee ci.key | wg pubkey > ci.pub
```

Write `/etc/wireguard/wg0.conf`, filling in the two keys:

```ini
[Interface]
Address = 10.66.0.1/24
ListenPort = 51820
PrivateKey = <contents of server.key>
# Size packets for the internet's 1500-byte MTU (see below).
MTU = 1420
# Over the tunnel, allow only the licensing server's port.
PostUp = iptables -I INPUT 1 -i %i -j DROP; iptables -I INPUT 1 -i %i -p tcp --dport 8080 -j ACCEPT
PostDown = iptables -D INPUT -i %i -p tcp --dport 8080 -j ACCEPT; iptables -D INPUT -i %i -j DROP

[Peer]
# GitHub Actions, demo-hub CI
PublicKey = <contents of ci.pub>
AllowedIPs = 10.66.0.2/32
```

- `10.66.0.0/24` is the tunnel's own range. It must not overlap the VPC's
  range (the default VPC is `172.31.0.0/16`).
- **Don't leave out `MTU = 1420`.** Without it, wg-quick sizes the tunnel from
  the instance's own interface. EC2 uses jumbo frames (9001 bytes) inside AWS,
  so the tunnel gets an MTU of 8921. The server then sends packets too big for
  the internet, which don't arrive at GitHub's runners. Small requests still
  work, so CI's connect check passes, but the license sync's larger reply never
  arrives. game-ci then fails after about 100 seconds with exit code 13
  ("Syncing entitlements failed").

Start the tunnel now and at every boot, and check that `wg0` has `mtu 1420`:

```bash
systemctl enable --now wg-quick@wg0
wg show wg0
ip link show wg0
```

Then print the client config that CI will use, filling in `ci.key`,
`server.pub`, the Elastic IP and the private IP:

```ini
[Interface]
PrivateKey = <contents of ci.key>
Address = 10.66.0.2/32

[Peer]
PublicKey = <contents of server.pub>
Endpoint = <Elastic IP>:51820
AllowedIPs = 10.66.0.1/32, <private IP>/32
PersistentKeepalive = 25
```

- `AllowedIPs` sends only the server's two addresses through the tunnel, so
  the runner's other traffic is untouched. The private IP is where the
  Licensing Server listens. `10.66.0.1` is the server's tunnel address. CI
  doesn't use it, and it doesn't answer ping, because `wg0.conf`'s firewall
  rules allow only TCP port 8080 over the tunnel.
- To test the tunnel by hand from a peer, run
  `curl http://<private IP>:8080/v1/status`, as CI's "Connect to the
  licensing server" step does. Any HTTP status means the tunnel works.
- Leave out any `DNS =` line: CI uses the server's IP, and the runner has no
  `resolvconf` for wg-quick.

Copy it into GitHub as described in [step 5](#5-github-settings). Then delete
`ci.key` from the server; only GitHub needs it.

```bash
shred -u /etc/wireguard/ci.key
exit
```

## 3. Install the Licensing Server

1. **Download it.** At [id.unity.com](https://id.unity.com/), go to
   **Organizations** > your organization > **Subscriptions & Services**. Select
   the Build Server subscription, then **Configure License Server** >
   **Download new server**, and choose Linux.
2. **Copy it to the instance.** Session Manager doesn't transfer files, so go
   through a private S3 bucket and a short-lived link:

   ```bash
   aws s3 cp Unity.Licensing.Server.linux-x64-*.zip s3://<private bucket>/
   aws s3 presign s3://<private bucket>/<file name> --expires-in 900
   ```

   On the instance:

   ```bash
   sudo apt-get install -y unzip
   curl -fLo /tmp/uls.zip '<presigned URL>'
   sudo mkdir -p /opt/UnityLicensingServer
   sudo unzip /tmp/uls.zip -d /opt/UnityLicensingServer
   cd /opt/UnityLicensingServer
   sudo -H ./Unity.Licensing.Server setup
   ```

3. **Answer the setup questions:**
   - **Server name:** for example `contactci-build-server`.
   - **HTTPS:** no. The tunnel already encrypts the traffic, and the port is
     reachable only through it.
   - **Network interface:** the instance's own interface (`ens5`, with the
     private IP), port `8080`. Setup doesn't list `wg0`.
     - Tunnel traffic still reaches it: the server accepts packets for its
       private IP on any interface, and `wg0.conf`'s firewall rules match the
       interface traffic arrives on, so port 8080 is still the only one open
       over the tunnel.
     - The security group keeps 8080 closed to the rest of the VPC and the
       internet.
   - **Administrator access:** set the admin password, and keep it in your
     password manager.

   Setup writes `server-registration-request.xml` and `services-config.json`
   to the current directory. CI doesn't need `services-config.json`, because
   game-ci writes its own from the variable in step 5.

If setup stops on a missing library, install it with apt and run setup again.
Unity's [requirements](https://docs.unity.com/en-us/licensing-server/requirements)
name the Ubuntu 20.04 packages (`libicu65`, `libssl1.1`). On 24.04, the ICU
package is `libicu74`.

## 4. Register the server and import its license

1. **Register it.** In the Unity ID portal, go to the Build Server
   subscription's **Configure License Server** page. Register a server by
   uploading `server-registration-request.xml`, assign it the Build Server
   seats, and download the license archive (a `.zip`). Copy the XML off the
   instance the same way, in reverse:

   ```bash
   aws s3 cp /opt/UnityLicensingServer/server-registration-request.xml s3://<private bucket>/
   ```

   That step runs on the instance, so the instance's role needs `s3:PutObject`
   on the bucket. Alternatively, print the file with `cat` and save it locally.
2. **Import the archive.** Copy it to the instance through S3 as before. Then,
   in a **new** Session Manager session (setup changes group membership), run
   the commands below.
   - Only members of the `unity-licensing-server` group can import, and that
     applies to root too. Setup adds the user who ran it (`ssm-user`) but not
     root, and `sudo` runs the import as root. Without the first command, the
     import fails with "You do not have the necessary permissions to import
     licensing files".
   - `sudo` reads root's groups fresh each time, but a `sudo -i` shell opened
     before the `usermod` doesn't see the change. Exit it and open a new one.

   ```bash
   sudo usermod -aG unity-licensing-server root
   cd /opt/UnityLicensingServer
   sudo -H ./Unity.Licensing.Server import /tmp/<license archive>.zip
   sudo -H ./Unity.Licensing.Server create-service
   sudo -H systemctl status unity.licensing.server.service
   sudo -H ./Unity.Licensing.Server status
   ```

   Don't unzip the archive. `status` should report the server as `Healthy`.
   Run it with `sudo -H` like the rest: as `ssm-user`, it can't write the
   server's database, so it reports `Database: Unhealthy` even when the
   server is fine.
3. **Back up the server's configuration files** now, as Unity's
   [backup page](https://docs.unity.com/en-us/licensing-server/server-setup-bkup)
   describes. Then delete the zips and XML from `/tmp` and the S3 bucket.

## 5. GitHub settings

In this repository, under **Settings** > **Secrets and variables** > **Actions**:

| Kind | Name | Value |
|---|---|---|
| Variable | `UNITY_LICENSING_SERVER` | `http://<private IP>:8080` |
| Secret | `UNITY_LICENSING_WIREGUARD` | The whole client config from step 2 |

Create each as the kind shown. CI reads `UNITY_LICENSING_SERVER` only as a
variable and `UNITY_LICENSING_WIREGUARD` only as a secret. A setting of the
wrong kind is ignored, the build is skipped, and the "Detect the Unity
license" step warns about it.

If more Unity repositories will build this way, these can be set at the
organization level instead. They'd share the one CI key, though, so only one
build across all of them could run at a time (see [Limits](#limits)).

Then delete the old serial-license secrets: `UNITY_SERIAL`, `UNITY_EMAIL`,
`UNITY_PASSWORD` and any `UNITY_LICENSE`. CI no longer reads them. game-ci
returned your seat at the end of each build, but check your Unity ID's
activations for one left behind by a cancelled run.

## 6. Check it works

Re-run the latest CI run on a branch with this workflow. In the **Build the
player** job:

- "Connect to the licensing server" should print `Reached the Unity Licensing
  Server at http://<private IP>:8080`.
- The Build step's log should show `Acquired floating license` near the start
  and `Returning floating license` at the end.
- The ULS dashboard should show the seat in use during the build, and free
  afterwards.

To open the dashboard from your machine, forward the port through Session
Manager (in Git Bash, with `winpty` in front):

```bash
aws ssm start-session --region <region> --target <instance id> \
  --document-name AWS-StartPortForwardingSessionToRemoteHost \
  --parameters host=<private IP>,portNumber=8080,localPortNumber=8080
```

Then open `http://localhost:8080`. The admin API only answers addresses in
`adminIpAllowlist`, which holds `127.0.0.1` by default. Forwarded requests
come from the instance's private IP instead. If the dashboard refuses them,
add the private IP to `adminIpAllowlist` in the server's configuration.

## Running it

- **Renewal.** When the Build Server subscription renews, download the new
  license archive from the portal and import it as in step 4. There is a
  one-month grace period; after that, builds fail.
- **Patching.** Ubuntu installs security updates on its own. A reboot is safe:
  WireGuard and the ULS start at boot. A build running during a reboot fails
  and needs a re-run.
- **Rotating the CI key.** Generate a new pair on the server and replace the
  `[Peer]` `PublicKey` in `wg0.conf`. Then run
  `systemctl restart wg-quick@wg0`, update `UNITY_LICENSING_WIREGUARD`, and
  shred the new private key. Do this when someone with access leaves, or if
  the secret may have leaked.
- **If CI can't connect,** check in this order:
  - The instance is running and the Elastic IP is still associated.
  - `wg show wg0` on the server shows a recent handshake.
  - `systemctl status unity.licensing.server.service` is active.
- **If CI connects but the Build step fails with exit code 13,** check that
  `ip link show wg0` on the server shows `mtu 1420` (see step 2). The server's
  journal (`journalctl -u unity.licensing.server.service`) shows whether the
  build's requests arrived.
- **If the Build step fails with exit code 19** and "Show the licensing
  client's errors" shows `429 | Too Many Requests` and `No remaining license`,
  every seat is held by a lease.
  - CI's "Return the floating license" step returns the lease even when a
    build fails or is cancelled. A lease can still be left behind if the
    runner itself is lost, or if the step warns that it couldn't return one.
  - Such a seat comes back when the lease expires. The client renews a lease
    while Unity runs, and each lease times out about 15 minutes after its
    last renewal: game-ci's "Acquired floating license" line prints the
    timeout.
  - Check the dashboard (step 6) for the open leases, then re-run the failed
    job once a seat is free.

## Limits

- **One build at a time.** A WireGuard key works from one runner at a time, so
  the build job is in the `unity-build-server` concurrency group:
  - A second build waits for the first.
  - GitHub keeps only one build waiting. A third cancels the waiting one, which
    then needs a re-run. That matters most for an rc or release tag, so check
    its run finished.
- **A seat can stay in use for up to 15 minutes after a build.** Unity's own
  Licensing Client can outlive the editor in the build container, and it
  sometimes takes a new lease right after game-ci returns the build's lease.
  CI's "Return the floating license" step returns those too. When it
  can't, the lease expires about 15 minutes after it was taken, and a
  build that starts before then fails with "No remaining license" and needs
  a re-run.
- **More seats need more keys.** Running builds in parallel needs another
  WireGuard peer (key and tunnel address) for each parallel build, and the
  workflow changed to give each build its own key.
- **What a leaked key gives.** The CI key reaches only the licensing server's
  port, over the tunnel. Anyone holding it can still take Build Server seats
  until the key is rotated.
