#!/bin/bash

# Check if two arguments are provided
if [ "$#" -ne 2 ]; then
    echo "Usage: $0 old_version new_version"
    exit 1
fi

old_version=$1
new_version=$2

# Update version in .iss, .manifest, and .sh files
find .. -type f \( -name "*.iss" -o -name "*.manifest" -o -name "*.sh" \) -exec sed -i '' "s/$old_version/$new_version/g" {} +

# Update version in .csproj files with specific prefixes
find .. -type f -name "*.csproj" -exec sed -i '' -E "s/(<AssemblyVersion>|<assemblyIdentity version=\")$old_version/\1$new_version/g" {} +

echo "Version update complete."

