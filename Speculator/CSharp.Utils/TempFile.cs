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

using CSharp.Utils.Extensions;

namespace CSharp.Utils;

public class TempFile : IDisposable
{
    private readonly FileInfo m_tempObj;
    
    public TempFile(string ext = ".tmp")
    {
        m_tempObj = new DirectoryInfo(Path.GetTempPath()).GetFile($"{Guid.NewGuid():N}{ext}");
    }

    public static implicit operator FileInfo(TempFile tempFile) =>
        tempFile.m_tempObj;
    public static implicit operator string(TempFile tempFile) =>
        tempFile.FullName;

    public string Name => m_tempObj.Name;
    public string FullName => m_tempObj.FullName;

    public void Dispose() => m_tempObj.TryDelete();
}
