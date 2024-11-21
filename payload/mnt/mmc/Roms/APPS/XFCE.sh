echo $0 $*
progdir=`dirname "$0"`
log=$progdir/XFCE.log
rm /var/log/Xorg*
export QT_X11_NO_MITSHM=1
date -us "$(wget -qS --spider google.com 2>&1 | grep "^  Date:" | sed 's/  Date: //' | head -n1)"
startx > $log 2>&1