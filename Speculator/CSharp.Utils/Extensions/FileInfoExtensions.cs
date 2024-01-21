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

namespace CSharp.Utils.Extensions;

public static class FileInfoExtensions
{
    public static string LeafName(this FileInfo file)
    {
        var s = file?.Name ?? string.Empty;
        return string.IsNullOrEmpty(s) ? s : Path.GetFileNameWithoutExtension(s);
    }
    
    public static byte[] ReadAllBytes(this FileInfo file) =>
        File.ReadAllBytes(file.FullName);

    public static string ReadAllText(this FileInfo file) =>
        File.ReadAllText(file.FullName);

    public static string[] ReadAllLines(this FileInfo file) =>
        File.ReadAllLines(file.FullName);

    public static FileInfo WriteAllText(this FileInfo file, string s)
    {
        File.WriteAllText(file.FullName, s);
        return file;
    }

    public static bool ReallyExists(this FileSystemInfo info)
    {
        info.Refresh();
        return info.Exists;
    }

    public static bool TryDelete(this FileInfo file)
    {
        try
        {
            file.Delete();
        }
        catch
        {
            // This is ok.
        }

        return !File.Exists(file.FullName);
    }
}
