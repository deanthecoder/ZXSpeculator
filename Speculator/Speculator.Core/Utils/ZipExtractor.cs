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

using CSharp.Utils;
using Ionic.Zip;

namespace Speculator.Core.Utils;

public static class ZipExtractor
{
    public static FileInfo ExtractZxFile(FileInfo zipFile)
    {
        // Find the first entry that matches the valid extensions.
        using var zip = ZipFile.Read(zipFile.FullName);
        var entry = zip.Entries.FirstOrDefault(e => ZxFileIo.FileFilters.Any(ext => ext.Trim('*').Equals(Path.GetExtension(e.FileName), StringComparison.OrdinalIgnoreCase)));

        if (entry == null)
        {
            Logger.Instance.Warn("No supported files found in the zip archive.");
            return null;
        }

        // Make a temp folder.
        var tempFile = Path.GetTempFileName();
        File.Delete(tempFile);
        var tempDir = new DirectoryInfo(tempFile);
        tempDir.Create();

        // Extract the file to the temp folder.
        entry.Extract(tempDir.FullName, ExtractExistingFileAction.OverwriteSilently);
        
        // Move up a level.
        var romFile = tempDir.EnumerateFiles().First();
        var newRomFile = Path.Combine(Path.GetTempPath(), romFile.Name);
        File.Move(romFile.FullName, newRomFile, true);
        
        // Remove the temp folder.
        tempDir.Delete(true);
        
        return new FileInfo(newRomFile);
    }
}
