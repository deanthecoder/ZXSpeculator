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

using CSharp.Core.Extensions;

namespace CSharp.Core;

public class TempDirectory : IDisposable
{
    private readonly DirectoryInfo m_tempObj;
    
    public TempDirectory()
    {
        m_tempObj = new DirectoryInfo(Path.GetTempPath()).GetDir(Guid.NewGuid().ToString("N"));
        m_tempObj.Create();
    }

    public static implicit operator DirectoryInfo(TempDirectory tempDirectory) =>
        tempDirectory.m_tempObj;
    public static implicit operator string(TempDirectory tempDirectory) =>
        tempDirectory.FullName;

    public string Name => m_tempObj.Name;
    public string FullName => m_tempObj.FullName;

    public IEnumerable<FileInfo> EnumerateFiles(string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
        m_tempObj.EnumerateFiles(searchPattern, searchOption);
    
    public void Dispose() => m_tempObj.TryDelete();
}
