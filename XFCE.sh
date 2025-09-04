progdir=`dirname "$0"`
log=$progdir/XFCE.log

# Log script arguments
echo "$(date): Script called with: $0 $*" >> $log

# Check if X is running and kill it if necessary
if pgrep -x "Xorg" > /dev/null; then
    echo "$(date): X server already running, terminating existing session..." >> $log
    
    # Kill XFCE session components
    pkill -f xfce4-session
    pkill -f xfce4-panel
    pkill -f xfwm4
    pkill -f xfdesktop
    
    # Give processes time to terminate gracefully
    sleep 2
    
    # Force kill X server if still running
    pkill -9 Xorg 2>/dev/null
    
    # Clean up lock files
    rm -f /tmp/.X*-lock /tmp/.X11-unix/X* 2>/dev/null
    
    echo "$(date): Existing X session terminated" >> $log
    sleep 1
fi

# Clean up old X logs
rm -f /var/log/Xorg*

# Set environment variable
export QT_X11_NO_MITSHM=1

# Sync time with Google
echo "$(date): Syncing system time..." >> $log
date -us "$(wget -qS --spider google.com 2>&1 | grep "^  Date:" | sed 's/  Date: //' | head -n1)"

# Start new X session
echo "$(date): Starting new X session..." >> $log
startx >> $log 2>&1
