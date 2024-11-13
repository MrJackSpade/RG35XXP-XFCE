#!/bin/bash

date -s "$(wget -qSO- --max-redirect=0 google.com 2>&1 | grep Date: | cut -d' ' -f5-8)Z"

# Disable GNOME screensaver (if installed)
gsettings set org.gnome.desktop.screensaver lock-enabled false
gsettings set org.gnome.desktop.screensaver idle-activation-enabled false
gsettings set org.gnome.desktop.session idle-delay 0

# Disable XFCE power management
xfconf-query -c xfce4-power-manager -p /xfce4-power-manager/dpms-enabled -s false
xfconf-query -c xfce4-power-manager -p /xfce4-power-manager/blank-on-ac -s 0
xfconf-query -c xfce4-power-manager -p /xfce4-power-manager/dpms-on-ac-off -s 0
xfconf-query -c xfce4-power-manager -p /xfce4-power-manager/dpms-on-ac-sleep -s 0
xfconf-query -c xfce4-power-manager -p /xfce4-power-manager/inactivity-on-ac -s 14
xfconf-query -c xfce4-power-manager -p /xfce4-power-manager/inactivity-sleep-mode-on-ac -s 1
xfconf-query -c xfce4-power-manager -p /xfce4-power-manager/lock-screen-suspend-hibernate -s false

# Disable XFCE screensaver
xfconf-query -c xfce4-screensaver -p /saver/enabled -s false
xfconf-query -c xfce4-screensaver -p /lock/enabled -s false

# Disable light-locker
light-locker-command -k
systemctl --user mask light-locker.service

# Disable DPMS in X11
xset -dpms
xset s off
xset s noblank

# Disable screen blanking in console
echo -ne "\033[9;0]" >> /etc/issue
echo -ne "\033[14;0]" >> /etc/issue

# Disable sleep and hibernate
systemctl mask sleep.target suspend.target hibernate.target hybrid-sleep.target

# Disable lid switch actions
sed -i 's/#HandleLidSwitch=suspend/HandleLidSwitch=ignore/' /etc/systemd/logind.conf
sed -i 's/#HandleLidSwitchExternalPower=suspend/HandleLidSwitchExternalPower=ignore/' /etc/systemd/logind.conf

# Restart systemd-logind to apply changes
systemctl restart systemd-logind

# Disable Automatic Updates
sed -i 's/APT::Periodic::Update-Package-Lists "1";/APT::Periodic::Update-Package-Lists "0";/' /etc/apt/apt.conf.d/20auto-upgrades
sed -i 's/APT::Periodic::Unattended-Upgrade "1";/APT::Periodic::Unattended-Upgrade "0";/' /etc/apt/apt.conf.d/20auto-upgrades

echo "Power management, screensaver, and locking features have been disabled."
echo "Please reboot the system for all changes to take effect."

apt update
#Don't upgrade as it breaks Retroarch, unless we can exclude retroarch from the upgrade
#DEBIAN_FRONTEND=noninteractive apt-get -y -o Dpkg::Options::="--force-confnew" upgrade

# Install packages without triggering locale generation
LC_ALL=C DEBIAN_FRONTEND=noninteractive apt install -y parole nano mousepad ntpdate onboard lsof language-pack-en xterm network-manager network-manager-gnome

apt clean

# Configure and generate locale once
echo "en_US.UTF-8 UTF-8" > /etc/locale.gen
locale-gen en_US.UTF-8 
update-locale LANG=en_US.UTF-8 LANGUAGE=en_US:en


if [ $? -ne 0 ]; then
    echo "Failed to mount swap" >> $log
fi

#freetype
cd /home/root/setup/source

echo "Unzipping freetype..." 
tar -xf freetype-2.13.3.tar.gz 
rm freetype-2.13.3.tar.gz

if [ $? -ne 0 ]; then
    echo "Nonzero exit unzipping freetype-2.13.3, this is probably normal"
fi

cd freetype-2.13.3 

if [ $? -ne 0 ]; then
    echo "Failed to change directory to freetype-2.13.3"
    exit 1
fi

echo "Making freetype..." 

make 

if [ $? -ne 0 ]; then
    echo "Failed to make freetype-2.13.3"
    exit 1
fi

echo "Installing freetype..." 

make install 

if [ $? -ne 0 ]; then
    echo "Failed to install freetype-2.13.3"
    exit 1
fi

#remove extra library
echo "Removing old freetype..." 

rm /mnt/vendor/lib/libfreetype.so.6.8.0

cd ..

# Swap file
SWAP_FILE="/swapfile"

# Check if the swap file does not exist
if [ ! -f "$SWAP_FILE" ]; then
    BLOCK_SIZE=1024
    BLOCK_COUNT=500

    # Create the swap file
    dd if=/dev/zero of="$SWAP_FILE" bs="${BLOCK_SIZE}K" count=$BLOCK_COUNT 
    chmod 600 "$SWAP_FILE"
    mkswap "$SWAP_FILE" 
else
    echo "Swap file already exists." 
fi

swapon "$SWAP_FILE" 

update-alternatives --install /usr/bin/x-www-browser x-www-browser /usr/bin/firefox/firefox 100
update-alternatives --set x-www-browser /usr/bin/firefox/firefox

sed -i 's/load-module module-native-protocol-unix/load-module module-native-protocol-unix auth-anonymous=1/' /etc/pulse/system.pa 

#correct retroarch configuration
sed -i 's/^audio_driver =.*/audio_driver = "sdl2"/' ~/.config/retroarch/retroarch.cfg

aplay /home/root/Music/o98.wav

rm /home/root/Desktop/RunMe.desktop

# Create new user 'user' with home directory
useradd -m user

# Set password to 'user'
echo "user:user" | chpasswd

# Add user to sudo group to allow package installation
usermod -aG sudo user

# Create .bashrc if it doesn't exist
touch /home/user/.bashrc
chown user:user /home/user/.bashrc

# Set correct permissions for home directory
chown -R user:user /home/user

sudo systemctl enable NetworkManager.service

reboot -f
