#!/bin/bash

#init
echo $0 $*
progdir=$(dirname "$(realpath "$0")")
log=$progdir/install-xfce.log
echo 'Starting' > $log 2>&1

cd $progdir

#Dont flush to log because we cant even see it yet. We're only logging 
#for user display.
date -s "$(wget -qSO- --max-redirect=0 google.com 2>&1 | grep Date: | cut -d' ' -f5-8)Z"

if [ $? -ne 0 ]; then
    echo "Failed to set date" >> $log
    exit 1
fi

#APT
dpkg-divert --local --rename --add /usr/bin/mandb

cp -aRfv $progdir/apt/. /

if [ $? -ne 0 ]; then
    echo "Nonzero apt copy?" >> $log
fi

apt-get update
apt-get install -y imagemagick fbi

if [ $? -ne 0 ]; then
    echo "Failed to install imagemagick" >> $log
    exit 1
fi

# Start watching log
pkill -f read_and_display
$progdir/components/read_and_display.sh $log & 

#Packages
apt-get install -y xorg xfce4 parole nano mousepad ntpdate onboard firefox qjoypad lsof language-pack-en xterm >> $log 2>&1
dpkg-divert --local --rename --remove /usr/bin/mandb

if [ $? -ne 0 ]; then
    echo "Failed to install packages" >> $log
    exit 1
fi

#freetype
cd source

echo "Unzipping freetype..." >> $log 2>&1
tar -xf freetype-2.13.2.tar.xz >> $log 2>&1

if [ $? -ne 0 ]; then
    echo "Nonzero exit unzipping freetype-2.13.2, this is probably normal" >> $log
fi

cd freetype-2.13.2 >> $log 2>&1

if [ $? -ne 0 ]; then
    echo "Failed to change directory to freetype-2.13.2" >> $log
    exit 1
fi

echo "Making freetype..." >> $log 2>&1

make >> $log 2>&1

if [ $? -ne 0 ]; then
    echo "Failed to make freetype-2.13.2" >> $log
    exit 1
fi

echo "Installing freetype..." >> $log 2>&1

make install >> $log 2>&1

if [ $? -ne 0 ]; then
    echo "Failed to install freetype-2.13.2" >> $log
    exit 1
fi

cd ..

#remove extra library
echo "Removing old freetype..." >> $log 2>&1

rm /mnt/vendor/lib/libfreetype.so.6.8.0

#XFCE
mkdir -p /home/root
mkdir -p /home/root/Desktop
mkdir -p /home/root/Downloads
mkdir -p /home/root/Templates
mkdir -p /home/root/Public
mkdir -p /home/root/Documents
mkdir -p /home/root/Music
mkdir -p /home/root/Pictures
mkdir -p /home/root/Videos


#locale
echo "en_US.UTF-8 UTF-8" > /etc/locale.gen
locale-gen en_US.UTF-8 >> $log 2>&1
dpkg-reconfigure --frontend=noninteractive locales >> $log 2>&1
update-locale LANG=en_US.UTF-8 LANGUAGE=en_US:en >> $log 2>&1

if [ $? -ne 0 ]; then
    echo "Failed to set local" >> $log
fi

# Swap file
SWAP_FILE="/mnt/mmc/swapfile"

# Check if the swap file does not exist
if [ ! -f "$SWAP_FILE" ]; then
    echo "Creating swap file... This part is SLOW! Grab a snack." >> $log 2>&1
    BLOCK_SIZE=1024
    BLOCK_COUNT=4000

    # Create the swap file
    dd if=/dev/zero of="$SWAP_FILE" bs="${BLOCK_SIZE}K" count=$BLOCK_COUNT >> $log 2>&1
    chmod 600 "$SWAP_FILE"
    mkswap "$SWAP_FILE" >> $log 2>&1
else
    echo "Swap file already exists." >> $log 2>&1
fi

swapon "$SWAP_FILE" >> $log 2>&1

if [ $? -ne 0 ]; then
    echo "Failed to mount swap" >> $log
fi

#configurations
cp -aRfv $progdir/payload/. / >> $log 2>&1

if [ $? -ne 0 ]; then
    echo "Nonzero payload copy?" >> $log
fi

#rebuild manuals
mandb >> $log 2>&1

echo "Completed." >> $log 2>&1

#kill log process
pkill -f read_and_display

reboot -f
