#!/usr/bin/env bash
set -euo pipefail

# EarthOS v1.0 (Harmony) Live ISO Builder
# Output ISO: $HOME/earthos-build/earthos-v3.iso

BUILD_DIR="${HOME}/earthos-build"
ROOTFS_DIR="${BUILD_DIR}/rootfs"
ISO_DIR="${BUILD_DIR}/iso"
SQUASHFS_DIR="${ISO_DIR}/live"
CHROOT_SCRIPT="/tmp/earthos-chroot-setup.sh"
ISO_NAME="earthos-v3.iso"
DEBIAN_RELEASE="bookworm"
ARCH="amd64"
MIRROR="http://deb.debian.org/debian"

require_cmd() {
  command -v "$1" >/dev/null 2>&1 || {
    echo "Missing required command: $1" >&2
    exit 1
  }
}

run_sudo() {
  sudo DEBIAN_FRONTEND=noninteractive "$@"
}

echo "==> Installing host dependencies"
run_sudo apt-get update
run_sudo apt-get install -y \
  debootstrap squashfs-tools xorriso grub-pc-bin grub-efi-amd64-bin mtools \
  syslinux isolinux curl wget dialog whiptail nano

for c in debootstrap mksquashfs xorriso grub-mkrescue; do
  require_cmd "$c"
done

mkdir -p "${BUILD_DIR}" "${ROOTFS_DIR}" "${ISO_DIR}" "${SQUASHFS_DIR}"

echo "==> Bootstrapping Debian ${DEBIAN_RELEASE} root filesystem"
run_sudo debootstrap --arch="${ARCH}" "${DEBIAN_RELEASE}" "${ROOTFS_DIR}" "${MIRROR}"

echo "==> Basic rootfs configuration"
echo "earthos" | run_sudo tee "${ROOTFS_DIR}/etc/hostname" >/dev/null
run_sudo tee "${ROOTFS_DIR}/etc/hosts" >/dev/null <<'HOSTS'
127.0.0.1 localhost
127.0.1.1 earthos
HOSTS

run_sudo tee "${ROOTFS_DIR}/etc/resolv.conf" >/dev/null <<'RESOLV'
nameserver 8.8.8.8
RESOLV

run_sudo tee "${ROOTFS_DIR}/etc/os-release" >/dev/null <<'OSREL'
PRETTY_NAME="EarthOS v1.0 (Harmony)"
NAME="EarthOS"
VERSION_ID="1.0"
VERSION="1.0 (Harmony)"
VERSION_CODENAME="harmony"
ID=earthos
ID_LIKE=debian
HOME_URL="https://example.com/earthos"
SUPPORT_URL="https://example.com/earthos/support"
BUG_REPORT_URL="https://example.com/earthos/bugs"
OSREL

run_sudo mkdir -p "${ROOTFS_DIR}/etc/earthos"
run_sudo tee "${ROOTFS_DIR}/etc/earthos/manufacturer" >/dev/null <<'MFG'
Manufacturer=Harmony
Product=EarthOS
Model=EarthOS Live System
Version=1.0
MFG

cat > /tmp/earthos-chroot-setup.sh <<'CHROOT'
#!/usr/bin/env bash
set -euo pipefail
export DEBIAN_FRONTEND=noninteractive

apt-get update
apt-get install -y \
  linux-image-amd64 live-boot systemd-sysv sudo bash nano \
  network-manager xorg openbox lightdm firefox-esr

# Earth user setup
id -u earth >/dev/null 2>&1 || useradd -m -s /bin/bash earth
echo 'earth:earth' | chpasswd
usermod -aG sudo earth
echo 'earth ALL=(ALL) NOPASSWD:ALL' > /etc/sudoers.d/99-earth
chmod 0440 /etc/sudoers.d/99-earth

# LightDM autologin
mkdir -p /etc/lightdm/lightdm.conf.d
cat > /etc/lightdm/lightdm.conf.d/50-earth-autologin.conf <<'EOF_AUTLOGIN'
[Seat:*]
autologin-user=earth
autologin-user-timeout=0
user-session=openbox
EOF_AUTLOGIN

# X init for openbox + firefox
cat > /home/earth/.xinitrc <<'EOF_XINIT'
#!/usr/bin/env bash
xsetroot -solid black
openbox-session &
exec firefox-esr --kiosk https://example.com
EOF_XINIT
chown earth:earth /home/earth/.xinitrc
chmod +x /home/earth/.xinitrc

# XPUI recovery script
cat > /usr/local/bin/xpui-recovery <<'EOF_XPUI'
#!/usr/bin/env bash
set -euo pipefail

show_menu() {
  whiptail --title "XPUI Recovery" --menu "Choose a recovery action:" 20 70 10 \
    1 "Reboot" \
    2 "Factory Reset" \
    3 "Wipe Cache" \
    4 "OTA Update" \
    5 "System Info" \
    6 "Power Off" \
    3>&1 1>&2 2>&3
}

factory_reset() {
  whiptail --title "Factory Reset" --yesno "Erase /home/earth data and reset settings?" 10 60 || return 0
  rm -rf /home/earth/*
  mkdir -p /home/earth
  chown -R earth:earth /home/earth
  whiptail --msgbox "Factory reset complete." 8 40
}

wipe_cache() {
  rm -rf /var/cache/* /tmp/*
  whiptail --msgbox "Cache wiped." 8 30
}

ota_update() {
  /usr/local/bin/earthos-ota || true
  whiptail --msgbox "OTA update routine completed." 8 45
}

system_info() {
  local info
  info="$(uname -a)\n\nOS: $(grep '^PRETTY_NAME=' /etc/os-release | cut -d= -f2-)\nCPU: $(grep -m1 'model name' /proc/cpuinfo | cut -d: -f2-)\nMemory: $(free -h | awk '/Mem:/ {print $2}')"
  whiptail --title "System Info" --msgbox "$info" 18 78
}

while true; do
  choice="$(show_menu || true)"
  case "${choice:-}" in
    1) reboot ;;
    2) factory_reset ;;
    3) wipe_cache ;;
    4) ota_update ;;
    5) system_info ;;
    6) poweroff ;;
    *) exit 0 ;;
  esac
done
EOF_XPUI
chmod +x /usr/local/bin/xpui-recovery

# Recovery auto-detect via /proc/cmdline
cat > /usr/local/bin/earthos-recovery-autodetect <<'EOF_REC'
#!/usr/bin/env bash
set -euo pipefail
if grep -qE '(^| )recovery(=1| )( |$)' /proc/cmdline; then
  exec /usr/local/bin/xpui-recovery
fi
EOF_REC
chmod +x /usr/local/bin/earthos-recovery-autodetect

cat > /etc/systemd/system/earthos-recovery.service <<'EOF_RECSVC'
[Unit]
Description=EarthOS Recovery Auto-Detect
After=local-fs.target

[Service]
Type=simple
ExecStart=/usr/local/bin/earthos-recovery-autodetect
StandardInput=tty
TTYPath=/dev/tty1
TTYReset=yes
TTYVHangup=yes
TTYVTDisallocate=yes

[Install]
WantedBy=multi-user.target
EOF_RECSVC
systemctl enable earthos-recovery.service

# OTA script (checks remote version + extracts update tar.gz)
cat > /usr/local/bin/earthos-ota <<'EOF_OTA'
#!/usr/bin/env bash
set -euo pipefail
VERSION_URL="https://example.com/earthos/latest-version.txt"
UPDATE_URL="https://example.com/earthos/update.tar.gz"
STATE_DIR="/var/lib/earthos"
LOCAL_VERSION_FILE="${STATE_DIR}/version"
TMP_UPDATE="/tmp/earthos-update.tar.gz"

mkdir -p "${STATE_DIR}"
: "${LOCAL_VERSION:=1.0}"
if [ -f "${LOCAL_VERSION_FILE}" ]; then
  LOCAL_VERSION="$(cat "${LOCAL_VERSION_FILE}")"
fi

REMOTE_VERSION="$(curl -fsSL "${VERSION_URL}" || echo "${LOCAL_VERSION}")"
if [ "${REMOTE_VERSION}" = "${LOCAL_VERSION}" ]; then
  echo "EarthOS is up to date (${LOCAL_VERSION})."
  exit 0
fi

echo "Updating EarthOS from ${LOCAL_VERSION} -> ${REMOTE_VERSION}"
wget -qO "${TMP_UPDATE}" "${UPDATE_URL}"
tar -xzf "${TMP_UPDATE}" -C /
echo "${REMOTE_VERSION}" > "${LOCAL_VERSION_FILE}"
rm -f "${TMP_UPDATE}"
echo "Update complete."
EOF_OTA
chmod +x /usr/local/bin/earthos-ota

# Installer script for /dev/sda
cat > /usr/local/bin/earthos-installer <<'EOF_INSTALL'
#!/usr/bin/env bash
set -euo pipefail
TARGET_DISK="/dev/sda"
TARGET_ROOT="/mnt/earthos-target"

if [ "$(id -u)" -ne 0 ]; then
  echo "Run as root."
  exit 1
fi

read -rp "This will erase ${TARGET_DISK}. Continue? [yes/NO]: " ans
[ "${ans}" = "yes" ] || { echo "Aborted."; exit 1; }

parted -s "${TARGET_DISK}" mklabel gpt
parted -s "${TARGET_DISK}" mkpart ESP fat32 1MiB 513MiB
parted -s "${TARGET_DISK}" set 1 esp on
parted -s "${TARGET_DISK}" mkpart primary ext4 513MiB 100%

mkfs.vfat -F32 "${TARGET_DISK}1"
mkfs.ext4 -F "${TARGET_DISK}2"

mkdir -p "${TARGET_ROOT}"
mount "${TARGET_DISK}2" "${TARGET_ROOT}"
mkdir -p "${TARGET_ROOT}/boot/efi"
mount "${TARGET_DISK}1" "${TARGET_ROOT}/boot/efi"

rsync -aAX --exclude='/proc/*' --exclude='/sys/*' --exclude='/dev/*' --exclude='/run/*' / "${TARGET_ROOT}"

grub-install --target=x86_64-efi --efi-directory="${TARGET_ROOT}/boot/efi" --boot-directory="${TARGET_ROOT}/boot" --removable --root-directory="${TARGET_ROOT}" "${TARGET_DISK}"
chroot "${TARGET_ROOT}" update-grub

echo "EarthOS installed successfully on ${TARGET_DISK}."
EOF_INSTALL
chmod +x /usr/local/bin/earthos-installer

# Ensure services
systemctl enable lightdm
systemctl enable NetworkManager

# Clean apt cache
apt-get clean
rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*
CHROOT

chmod +x /tmp/earthos-chroot-setup.sh
run_sudo cp /tmp/earthos-chroot-setup.sh "${ROOTFS_DIR}${CHROOT_SCRIPT}"

# Bind mount pseudo filesystems for chroot
for d in dev proc sys run; do
  run_sudo mount --bind "/$d" "${ROOTFS_DIR}/$d"
done

cleanup_mounts() {
  for d in run sys proc dev; do
    if mountpoint -q "${ROOTFS_DIR}/$d"; then
      run_sudo umount -lf "${ROOTFS_DIR}/$d" || true
    fi
  done
}
trap cleanup_mounts EXIT

echo "==> Running chroot provisioning"
run_sudo chroot "${ROOTFS_DIR}" /bin/bash "${CHROOT_SCRIPT}"
run_sudo rm -f "${ROOTFS_DIR}${CHROOT_SCRIPT}"
rm -f /tmp/earthos-chroot-setup.sh

echo "==> Preparing ISO structure"
run_sudo mkdir -p "${ISO_DIR}/boot/grub" "${SQUASHFS_DIR}"
run_sudo mksquashfs "${ROOTFS_DIR}" "${SQUASHFS_DIR}/filesystem.squashfs" -e boot

KERNEL_PATH="$(run_sudo find "${ROOTFS_DIR}/boot" -maxdepth 1 -name 'vmlinuz-*' | head -n1)"
INITRD_PATH="$(run_sudo find "${ROOTFS_DIR}/boot" -maxdepth 1 -name 'initrd.img-*' | head -n1)"

if [ -z "${KERNEL_PATH}" ] || [ -z "${INITRD_PATH}" ]; then
  echo "Kernel/initrd not found in rootfs /boot" >&2
  exit 1
fi

run_sudo cp "${KERNEL_PATH}" "${ISO_DIR}/live/vmlinuz"
run_sudo cp "${INITRD_PATH}" "${ISO_DIR}/live/initrd"

run_sudo tee "${ISO_DIR}/boot/grub/grub.cfg" >/dev/null <<'GRUBCFG'
set default=0
set timeout=8

menuentry "EarthOS v1.0 (Harmony)" {
    linux /live/vmlinuz boot=live components quiet splash username=earth
    initrd /live/initrd
}

menuentry "EarthOS XPUI Recovery" {
    linux /live/vmlinuz boot=live components recovery=1 nomodeset
    initrd /live/initrd
}
GRUBCFG

echo "==> Building ISO image"
run_sudo grub-mkrescue -o "${BUILD_DIR}/${ISO_NAME}" "${ISO_DIR}" >/dev/null 2>&1
run_sudo chown "${USER}:${USER}" "${BUILD_DIR}/${ISO_NAME}"

echo "✅ ISO created at: ${BUILD_DIR}/${ISO_NAME}"
