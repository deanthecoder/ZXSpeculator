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