echo $0 $*
progdir=`dirname "$0"`
log=$progdir/XFCE.log
rm /var/log/Xorg*
export QT_X11_NO_MITSHM=1
ntpdate time.nist.gov
startx > $log 2>&1