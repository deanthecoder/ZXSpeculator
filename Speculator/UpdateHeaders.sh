#!/bin/bash

# Copyright notice to be added
copyright_notice="// Code authored by Dean Edis (DeanTheCoder).\n\
// Anyone is free to copy, modify, use, compile, or distribute this software,\n\
// either in source code form or as a compiled binary, for any non-commercial\n\
// purpose.\n\
//\n\
// If you modify the code, please retain this copyright header,\n\
// and consider contributing back to the repository or letting us know\n\
// about your modifications. Your contributions are valued!\n\
//\n\
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.\n"

# Find all .cs files in the current directory and its subdirectories
find . -type f -name "*.cs" -print0 | while IFS= read -r -d '' file; do
    # Create a temporary file for the processed content
    temp_file=$(mktemp)

    # Delete all lines until a line starts with an alphabetic character, then append the rest of the file
    awk '/^[a-zA-Z]/,EOF' "$file" > "$temp_file"

    # Prepend the copyright notice
    echo -e "$copyright_notice" | cat - "$temp_file" > "$file"

    # Clean up temporary file
    rm "$temp_file"
done
