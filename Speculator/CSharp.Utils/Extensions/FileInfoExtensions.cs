namespace CSharp.Utils.Extensions;

public static class FileInfoExtensions
{
    public static byte[] ReadAllBytes(this FileInfo file)
    {
        return File.ReadAllBytes(file.FullName);
    }
    
    public static string ReadAllText(this FileInfo file)
    {
        return File.ReadAllText(file.FullName);
    }

    public static FileInfo WriteAllText(this FileInfo file, string s)
    {
        File.WriteAllText(file.FullName, s);
        return file;
    }

    public static bool ReallyExists(this FileInfo file)
    {
        file.Refresh();
        return file.Exists;
    }
}