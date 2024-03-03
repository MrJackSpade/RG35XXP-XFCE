#!/bin/bash

# Check if a file name is provided
if [ $# -eq 0 ]; then
    echo "No log file specified."
    exit 1
fi

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# The file to read from, provided as a command-line argument
FILE_TO_READ="$1"

# Loop indefinitely
while true
do
    # Read the last line from the file
    LAST_LINE=$(tail -n 1 "$FILE_TO_READ")

    # Get current date and time in ISO8601 format with seconds
	CURRENT_DATETIME=$(date +"%Y-%m-%d %H:%M:%S")

    # Prepend the datetime to the last line
    LINE_WITH_DATETIME="[$CURRENT_DATETIME] $LAST_LINE"

    # Call display.sh with the modified line
    "$DIR/display.sh" "$LINE_WITH_DATETIME"

    # Sleep for a bit to avoid constant looping
    sleep 1
done


#EXAMPLE 

#!/bin/bash

# Start the child script in the background
#/path/to/read_and_display.sh &
#CHILD_PID=$!

# Your parent script's main logic goes here
# ...

# When done, kill the child process
#kill $CHILD_PID