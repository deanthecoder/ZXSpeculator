#!/bin/sh

# Define variables
APP_NAME="ZX Speculator"
APP_VERSION="1.0.0"
APP_VERSION_SHORT="1.0"
EXECUTABLE_NAME="Speculator"
BUNDLE_NAME="${APP_NAME}.app"
IDENTIFIER="com.deanedis.zxspeculator"

# Step 1: Publish the application
rm -rf bin/Release/net7.0/osx-arm64/publish/ 
dotnet publish -c Release -r osx-arm64 --self-contained true -property:Configuration=Release -p:UseAppHost=true

# Step 2: Create the app bundle structure
mkdir -p "$BUNDLE_NAME/Contents/MacOS"
mkdir -p "$BUNDLE_NAME/Contents/Resources"

# Step 3: Move the published files into the app bundle
cp -r bin/Release/net7.0/osx-arm64/publish/* "$BUNDLE_NAME/Contents/MacOS/"

# Step 4: Delete any .pdb files
find "$BUNDLE_NAME/Contents/MacOS/" -name '*.pdb' -delete

# Step 5: Create the Info.plist file
cat > "$BUNDLE_NAME/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleIconFile</key>
    <string>Icon.icns</string>
    <key>CFBundleIdentifier</key>
    <string>$IDENTIFIER</string>
    <key>CFBundleName</key>
    <string>$APP_NAME</string>
    <key>CFBundleExecutable</key>
    <string>$EXECUTABLE_NAME</string>
    <key>CFBundleVersion</key>
    <string>$APP_VERSION</string>
    <key>CFBundleShortVersionString</key>
    <string>$APP_VERSION_SHORT</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.12</string>
    <key>CFBundleSignature</key>
    <string>????</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

cp Assets/Icon.icns "$BUNDLE_NAME/Contents/Resources/Icon.icns"

echo "App bundle $BUNDLE_NAME created."
