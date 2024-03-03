#!/bin/sh
echo $0 $*
progdir=`dirname "$0"`
log=$progdir/change-language.log

touch $log

apt-get install -y imagemagick fbi

if [ $? -ne 0 ]; then
    echo "Failed to install imagemagick" >> $log
    exit 1
fi

# Start watching log
pkill -f read_and_display
$progdir/components/read_and_display.sh $log &

apt-get install -y language-pack-en  >> $log 2>&1
echo "en_US.UTF-8 UTF-8" > /etc/locale.gen
locale-gen en_US.UTF-8 >> $log 2>&1
dpkg-reconfigure --frontend=noninteractive locales >> $log 2>&1
update-locale LANG=en_US.UTF-8 LANGUAGE=en_US:en >> $log 2>&1

echo "Completed." >> $log 2>&1

pkill -f read_and_display

reboot -f