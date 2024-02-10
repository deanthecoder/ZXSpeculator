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

using CSharp.Core;
using Ionic.Zip;

namespace Speculator.Core.Utils;

public static class ZipExtractor
{
    /// <summary>
    /// Return the ROM file extracted from the archive.
    /// </summary>
    /// <remarks>The file will automatically be deleted when the app closes.</remarks>
    public static FileInfo ExtractZxFile(FileInfo zipFile)
    {
        // Find the first entry that matches the valid extensions.
        using var zip = ZipFile.Read(zipFile.FullName);
        var entry = zip.Entries.FirstOrDefault(e => ZxFileIo.OpenFilters.Any(ext => ext.Trim('*').Equals(Path.GetExtension(e.FileName), StringComparison.OrdinalIgnoreCase)));

        if (entry == null)
        {
            Logger.Instance.Warn("No supported files found in the zip archive.");
            return null;
        }

        // Extract the file to a temp folder.
        var tempDir = new TempDirectory();
        LazyDisposer.Instance.Add(tempDir);
        entry.Extract(tempDir, ExtractExistingFileAction.OverwriteSilently);
        
        return tempDir.EnumerateFiles().First();
    }
}
