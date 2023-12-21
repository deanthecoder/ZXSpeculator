#!/bin/bash

# Function to check if a command exists
command_exists() {
    type "$1" &> /dev/null
}

# Install necessary tools if they are not installed
if ! command_exists cmake; then
    echo "cmake not found. Installing cmake..."
    brew install cmake
else
    echo "cmake already exists."
fi

# Clone the libsoundio repository
git clone https://github.com/andrewrk/libsoundio.git
cd libsoundio

# Build for macOS
echo "Building for macOS..."
mkdir -p build-mac && cd build-mac
cmake ..
make
cd ..

echo "Build process complete."

