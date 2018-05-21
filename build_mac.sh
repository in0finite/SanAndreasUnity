#!/bin/bash

APPNAME="GTA-SA-Unity.app"

# Delete existing app bundle
echo "Removing old bundle..."
rm -rf ./$APPNAME

# Save config.json from being destroyed
echo "Saving config.json..."
cp config.json config.json.bak

# Build App
echo "Building binary..."
/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchmode -projectPath $PWD -buildOSXUniversalPlayer ./$APPNAME

# Restore file
echo "Restoring config.json..."
mv config.json.bak config.json

# Copy config and data into App bundle
echo "Preparing new bundle..."
cp config.json ./$APPNAME/config.json
cp config.user.json ./$APPNAME/config.user.json
cp Data/auxanimgrp.dat ./$APPNAME/Contents/Data/auxanimgrp.dat

