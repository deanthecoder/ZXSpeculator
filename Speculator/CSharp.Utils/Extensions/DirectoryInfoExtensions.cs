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

public static class DirectoryInfoExtensions
{
    public static FileInfo GetFile(this DirectoryInfo info, string name)
    {
        return new FileInfo(Path.Combine(info.FullName, name));
    }

    public static DirectoryInfo GetDir(this DirectoryInfo info, string name)
    {
        return new DirectoryInfo(Path.Combine(info.FullName, name));
    }
}
