#!/bin/bash

#init
echo $0 $*
progdir=`dirname "$0"`

# Path to the log file
LOG_FILE="/tmp/test.log"

# Start watching log
$progdir/components/read_and_display.sh $LOG_FILE & CHILD_PID=$!

# Infinite loop
while true; do
    # Writing current date and time to the log file
    echo "$(date) - Writing data to log" >> "$LOG_FILE"
    
    # Wait for half a second
    sleep 0.5
done

#kill log process
kill $CHILD_PID