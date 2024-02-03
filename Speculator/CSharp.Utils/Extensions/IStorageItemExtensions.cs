// Code authored by Dean Edis (DeanTheCoder).
// Anyone is free to copy, modify, use, compile, or distribute this software,
// either in source code form or as a compiled binary, for any non-commercial
// purpose.
//
// If you modify the code, please retain this copyright header,
// and consider contributing back to the repository or letting us know
// about your modifications. Your contributions are valued!
//
// THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND.

using Avalonia.Platform.Storage;

namespace CSharp.Utils.Extensions;

public static class IStorageItemExtensions
{
    public static FileInfo ToFileInfo(this IStorageFile storageFile) =>
        storageFile != null ? new FileInfo(storageFile.Path.LocalPath) : null;

    public static DirectoryInfo ToDirectoryInfo(this IStorageFolder storageFolder) =>
        storageFolder != null ? new DirectoryInfo(storageFolder.Path.LocalPath) : null;
}