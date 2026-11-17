# Unity Licensing Server for CI

CI builds the player with a floating **Unity Build Server** license, so no one's
personal Unity seat is used. The license comes from a **Unity Licensing Server**
(ULS) on a small EC2 instance. The ULS is never exposed to the internet: CI's
GitHub-hosted runner reaches it through a WireGuard tunnel, for the length of
the build job only.

```
GitHub runner ── WireGuard (UDP 51820) ──> EC2: wg0 10.66.0.1
  └ game-ci Unity container                    └ Unity Licensing Server :8080
      takes a floating license, builds, returns it
```

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
- **Cost.** About $20 a month on demand in us-east-1: a `t3.small` (about $15),
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
| AMI | Ubuntu Server 24.04 LTS (x86_64) |
| Instance type | `t3.small` |
| Key pair | None (you connect with Session Manager) |
| Network | Any subnet with a route to an internet gateway, auto-assign public IP off |
| Security group | New, `unity-licensing-server`: **inbound UDP 51820 from 0.0.0.0/0 only**, no SSH. Outbound: leave the default |
| Storage | 20 GiB gp3, encrypted |
| Advanced: IAM instance profile | A role with the `AmazonSSMManagedInstanceCore` policy |
| Advanced: Metadata version | V2 only (token required) |
| Advanced: Termination protection | Enable |

Then:

1. **Elastic IPs** > **Allocate Elastic IP address**, then **Associate** it with
   the instance. This is the WireGuard endpoint; without it the public IP
   changes on every stop and start.
2. Optionally, have **Data Lifecycle Manager** take a daily snapshot of the
   instance's volume.

Port 51820 is open to everyone, but WireGuard doesn't answer any packet that
isn't signed by a known key, so it doesn't show up to a scan. Port 8080 is never
open in the security group.

Connect to the instance from your machine:

```bash
aws ssm start-session --target <instance id>
```

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
# Over the tunnel, allow only the licensing server's port.
PostUp = iptables -I INPUT 1 -i %i -j DROP; iptables -I INPUT 1 -i %i -p tcp --dport 8080 -j ACCEPT
PostDown = iptables -D INPUT -i %i -p tcp --dport 8080 -j ACCEPT; iptables -D INPUT -i %i -j DROP

[Peer]
# GitHub Actions, demo-hub CI
PublicKey = <contents of ci.pub>
AllowedIPs = 10.66.0.2/32
```

`10.66.0.0/24` is the tunnel's own range. It must not overlap the VPC's range
(the default VPC is `172.31.0.0/16`). Start the tunnel now and at every boot:

```bash
systemctl enable --now wg-quick@wg0
wg show wg0
```

Then print the client config that CI will use, filling in `ci.key`,
`server.pub` and the Elastic IP:

```ini
[Interface]
PrivateKey = <contents of ci.key>
Address = 10.66.0.2/32

[Peer]
PublicKey = <contents of server.pub>
Endpoint = <Elastic IP>:51820
AllowedIPs = 10.66.0.1/32
PersistentKeepalive = 25
```

- `AllowedIPs = 10.66.0.1/32` sends only the server's address through the
  tunnel, so the runner's other traffic is untouched.
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
   - **Network interface:** `wg0` (`10.66.0.1`), port `8080`. If setup doesn't
     list `wg0`, pick the instance's own interface (`ens5`). Then use its
     private IP wherever this page says `10.66.0.1` in a URL, and add
     `<private IP>/32` to the client config's `AllowedIPs`. The server accepts
     tunnel traffic for its private IP too.
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
   in a **new** Session Manager session (setup changes group membership), run:

   ```bash
   cd /opt/UnityLicensingServer
   sudo -H ./Unity.Licensing.Server import /tmp/<license archive>.zip
   sudo -H ./Unity.Licensing.Server create-service
   sudo -H systemctl status unity.licensing.server.service
   ./Unity.Licensing.Server status
   ```

   Don't unzip the archive. `status` should report the server as `Healthy`.
3. **Back up the server's configuration files** now, as Unity's
   [backup page](https://docs.unity.com/en-us/licensing-server/server-setup-bkup)
   describes. Then delete the zips and XML from `/tmp` and the S3 bucket.

## 5. GitHub settings

In this repository, under **Settings** > **Secrets and variables** > **Actions**:

| Kind | Name | Value |
|---|---|---|
| Variable | `UNITY_LICENSING_SERVER` | `http://10.66.0.1:8080` |
| Secret | `UNITY_LICENSING_WIREGUARD` | The whole client config from step 2 |

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
  Server at http://10.66.0.1:8080`.
- The Build step's log should show `Acquired floating license` near the start
  and `Returning floating license` at the end.
- The ULS dashboard should show the seat in use during the build, and free
  afterwards.

To open the dashboard from your machine, forward the port through Session
Manager:

```bash
aws ssm start-session --target <instance id> \
  --document-name AWS-StartPortForwardingSession \
  --parameters portNumber=8080,localPortNumber=8080
```

Then open `http://localhost:8080`. The admin API only answers addresses in
`adminIpAllowlist` (`127.0.0.1` is in it by default).

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

## Limits

- **One build at a time.** A WireGuard key works from one runner at a time, so
  the build job is in the `unity-build-server` concurrency group:
  - A second build waits for the first.
  - GitHub keeps only one build waiting. A third cancels the waiting one, which
    then needs a re-run. That matters most for an rc or release tag, so check
    its run finished.
- **More seats need more keys.** Running builds in parallel needs another
  WireGuard peer (key and tunnel address) for each parallel build, and the
  workflow changed to give each build its own key.
- **What a leaked key gives.** The CI key reaches only the licensing server's
  port, over the tunnel. Anyone holding it can still take Build Server seats
  until the key is rotated.
