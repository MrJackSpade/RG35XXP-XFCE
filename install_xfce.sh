#!/bin/bash

#init
echo $0 $*
progdir=$(dirname "$(realpath "$0")")
log=$progdir/install-xfce.log
echo 'Starting'

cd $progdir

date -s "$(wget -qSO- --max-redirect=0 google.com 2>&1 | grep Date: | cut -d' ' -f5-8)Z"

if [ $? -ne 0 ]; then
    echo "Failed to set date"
    exit 1
fi

#Packages
apt update
apt install -y xorg xfce4 qjoypad

if [ $? -ne 0 ]; then
    echo "Failed to install packages"
    exit 1
fi


#XFCE
mkdir -p /root

#configurations
rm /usr/bin/firefox

find /mnt/mmc/Roms/APPS/payload -mindepth 1 -printf '%P\0' | \
while IFS= read -r -d '' file; do
    if [ -d "/mnt/mmc/Roms/APPS/payload/$file" ]; then
        echo "Creating directory: /$file"
        mkdir -p "/$file"
    else
        echo "Moving file: /mnt/mmc/Roms/APPS/payload/$file to /$file"
        mv -f "/mnt/mmc/Roms/APPS/payload/$file" "/$file"
    fi
done

cd /usr/bin/firefox

tar -xf libxul.so.tar.gz
rm libxul.so.tar.gz

cd /

echo "Operation completed."
chmod +x /home/root/setup/run_me.sh

if [ $? -ne 0 ]; then
    echo "Nonzero payload copy?"
fi

rm /mnt/mmc/Roms/APPS/install_xfce.sh

xfconf-query --channel thunar --property /misc-exec-shell-scripts-by-default --create --type bool --set true

mkdir -p /home/root 
cp -R /root/* /home/root/ 
chown -R root:root /home/root
sed -i 's|root:x:0:0:root:/root:|root:x:0:0:root:/home/root:|' /etc/passwd

rm /etc/xdg/autostart/xfce4-screensaver.desktop
rm /etc/xdg/autostart/xscreensaver.desktop
apt remove -y xfce4-screensaver

apt-get clean

reboot -f
