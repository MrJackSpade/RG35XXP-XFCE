#!/bin/bash

# Check if text is provided
if [ -z "$1" ]; then
    echo "Please provide text as an argument."
    exit 1
fi

# Text to be displayed
TEXT="$1"

# Framebuffer device
FB_DEVICE=/dev/fb0

# Image file
IMAGE_FILE=/tmp/text.png

# Convert text to image with BGRA pixel ordering in RAW format
convert -size 640x480 -background black -fill white -font Courier -pointsize 18 -gravity northwest caption:"$TEXT" PNG:$IMAGE_FILE
pkill -f fbi
fbi -a -T 1 -noverbose -d /dev/fb0 $IMAGE_FILE