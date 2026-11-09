# macOS remote desktop (VNC)

This repository can start a temporary GitHub-hosted macOS desktop and expose its Screen Sharing service only to your private Tailscale network.

## One-time setup

1. Create or sign in to a [Tailscale](https://tailscale.com/) account and install Tailscale on the device you will use to connect.
2. In the Tailscale admin console, create an **ephemeral auth key**. Restrict it to a tag if your tailnet policy requires tagged devices.
3. In this GitHub repository, open **Settings → Secrets and variables → Actions → New repository secret**.
4. Add these two repository secrets:
   - `TAILSCALE_AUTH_KEY`: the Tailscale auth key. Keep it private.
   - `VNC_PASSWORD`: exactly 8 characters. Apple’s legacy VNC password authentication uses only eight characters; use a unique random value and do not reuse another password.
5. Make sure the Tailscale access policy permits your personal device to reach the runner on TCP port 5900.

Do not put either secret in workflow YAML, source files, issue comments, screenshots, or logs. Because this repository is public, never print the VNC password or Tailscale auth key in a workflow summary or artifact.

## Start a session

1. Open the repository's **Actions** tab.
2. Select **macOS Remote Desktop (VNC)**.
3. Choose a session length (1–5 hours) and select **Run workflow**.
4. Open the running job's **Summary** and copy the private VNC address.
5. On your Android phone, install a VNC client such as RealVNC Viewer or another reputable VNC viewer. Sign in to the same Tailscale network first.
6. Connect to the displayed address on port 5900 and enter the value stored in the `VNC_PASSWORD` secret. VNC password authentication does not use a separate username.

The workflow uses the supported `macos-15-intel` GitHub-hosted image. The macOS 14 runner labels were retired on 2 November 2026.

## Important limitations

- This is a temporary GitHub Actions virtual machine, not a persistent Mac. The desktop is destroyed when the workflow ends.
- Save work by committing and pushing it to GitHub or uploading non-sensitive outputs as workflow artifacts before the session expires.
- The workflow is manually triggered and limited to five hours, leaving time before GitHub's six-hour job timeout.
- Tailscale keeps port 5900 off the public internet. Do not replace it with a public unauthenticated TCP tunnel.
- Apple Screen Sharing/VNC behaviour varies by macOS image. On some newer images, VNC clients may receive view-only access or encounter permission limitations. If mouse/keyboard input is unavailable on `macos-15-intel`, this workflow needs a different supported remote-control method; do not expose VNC publicly to work around it.
- GitHub-hosted runner availability, usage limits, and billing depend on repository visibility and GitHub policy.

