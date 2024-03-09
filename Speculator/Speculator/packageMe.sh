#!/bin/sh

# Define common variables
APP_NAME="ZX Speculator"
APP_VERSION="1.4.0"
APP_VERSION_SHORT="1.4"
EXECUTABLE_NAME="Speculator"
IDENTIFIER="com.deanedis.zxspeculator"
BUNDLE_NAME="${APP_NAME}.app"

# Function to create an app bundle and .dmg for a given architecture
create_dmg() {
    ARCH=$1
    DMG_NAME="${APP_NAME}_${APP_VERSION}_${ARCH}.dmg"
    VOLUME_NAME="${APP_NAME} ${APP_VERSION_SHORT} (${ARCH})"

    # Create (or clear) the temporary directory under the home directory
    DMG_TEMP_FOLDER="${HOME}/build_temp_${ARCH}"
    if [ -d "$DMG_TEMP_FOLDER" ]; then
        echo "Clearing existing temporary folder: $DMG_TEMP_FOLDER"
        rm -rf "$DMG_TEMP_FOLDER"/*
    else
        echo "Creating temporary folder: $DMG_TEMP_FOLDER"
        mkdir -p "$DMG_TEMP_FOLDER"
    fi

    echo "Creating $ARCH build..."

    # Publish the application for the given architecture
    rm -rf bin/Release/net7.0/${ARCH}/publish/
    dotnet publish -c Release -r ${ARCH} --self-contained true -property:Configuration=Release -p:UseAppHost=true

    # Create the app bundle structure in temporary folder
    mkdir -p "$DMG_TEMP_FOLDER/$BUNDLE_NAME/Contents/MacOS"
    mkdir -p "$DMG_TEMP_FOLDER/$BUNDLE_NAME/Contents/Resources"

    # Move the published files into the app bundle
    cp -r bin/Release/net7.0/${ARCH}/publish/* "$DMG_TEMP_FOLDER/$BUNDLE_NAME/Contents/MacOS/"

    # Delete any .pdb files
    find "$DMG_TEMP_FOLDER/$BUNDLE_NAME/Contents/MacOS/" -name '*.pdb' -delete

    # Create the Info.plist file
    cat > "$DMG_TEMP_FOLDER/$BUNDLE_NAME/Contents/Info.plist" << EOF
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

    cp Assets/Icon.icns "$DMG_TEMP_FOLDER/$BUNDLE_NAME/Contents/Resources/Icon.icns"

    echo "App bundle $BUNDLE_NAME created for $ARCH..."

    # Prepare for DMG creation

    # Remove any existing .dmg file with the same name
    rm -f "$DMG_NAME"

    # Link to Applications in the temporary folder
    ln -s /Applications "$DMG_TEMP_FOLDER/Applications"

    # Debugging: Display the size of the temp folder
    echo "Size of the temporary folder (before DMG creation):"
    du -sh "$DMG_TEMP_FOLDER"

    # Create a temporary disk image and format it
    hdiutil create -ov -volname "$VOLUME_NAME" -fs HFS+ -srcfolder "$DMG_TEMP_FOLDER" -format UDRW "$DMG_TEMP_FOLDER/temp_$DMG_NAME"

    # Convert the temporary disk image to a compressed .dmg file
    hdiutil convert "$DMG_TEMP_FOLDER/temp_$DMG_NAME" -format UDZO -o "$DMG_NAME"

    # Remove the temporary disk image and folder
    rm -f "$DMG_TEMP_FOLDER/temp_$DMG_NAME"
    rm -rf "$DMG_TEMP_FOLDER"

    echo "DMG file $DMG_NAME created for $ARCH."
}

# Create DMG files.
create_dmg "osx-x64"
create_dmg "osx-arm64"
