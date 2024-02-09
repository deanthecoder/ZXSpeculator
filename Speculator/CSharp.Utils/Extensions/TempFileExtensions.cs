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

public static class TempFileExtensions
{
    public static byte[] ReadAllBytes(this TempFile file) =>
        ((FileInfo)file).ReadAllBytes();

    public static string ReadAllText(this TempFile file) =>
        ((FileInfo)file).ReadAllText();

    public static string[] ReadAllLines(this TempFile file) =>
        ((FileInfo)file).ReadAllLines();

    public static TempFile WriteAllText(this TempFile file, string s)
    {
        ((FileInfo)file).WriteAllText(s);
        return file;
    }

    public static TempFile WriteAllBytes(this TempFile file, byte[] bytes)
    {
        ((FileInfo)file).WriteAllBytes(bytes);
        return file;
    }

    public static bool ReallyExists(this TempFile file) =>
        ((FileInfo)file).ReallyExists();

    public static bool TryDelete(this TempFile file) =>
        ((FileInfo)file).TryDelete();
}