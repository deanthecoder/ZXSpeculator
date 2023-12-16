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

# Check if the user wants to build for Windows
if [ "$1" == "--build-windows" ]; then
    # Install MinGW-w64 if it's not installed
    if ! command_exists x86_64-w64-mingw32-gcc; then
        echo "MinGW-w64 not found. Installing MinGW-w64..."
        brew install mingw-w64
    else
        echo "MinGW-w64 already exists."
    fi

    # Build for Windows (64-bit)
    echo "Building for Windows (64-bit)..."
    mkdir -p build-windows && cd build-windows
    cmake .. -DCMAKE_C_COMPILER=x86_64-w64-mingw32-gcc -DCMAKE_CXX_COMPILER=x86_64-w64-mingw32-g++
    make
    cd ..
fi

echo "Build process complete."

