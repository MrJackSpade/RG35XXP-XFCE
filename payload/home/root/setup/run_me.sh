#!/bin/bash

date -s "$(wget -qSO- --max-redirect=0 google.com 2>&1 | grep Date: | cut -d' ' -f5-8)Z"

apt update
DEBIAN_FRONTEND=noninteractive apt-get -y -o Dpkg::Options::="--force-confnew" upgrade

apt install -y parole nano mousepad ntpdate onboard lsof language-pack-en xterm 

apt clean

#locale
echo "en_US.UTF-8 UTF-8" > /etc/locale.gen
locale-gen en_US.UTF-8 
dpkg-reconfigure --frontend=noninteractive locales 
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

reboot -f
