namespace CSharp.Utils.Extensions;

public static class StringExtensions
{
    public static string ToSafeFileName(this string s)
    {
        var badChars = Path.GetInvalidFileNameChars().Union(new[]
        {
            '\\', '/'
        });
        return badChars.Aggregate(s, (current, nameChar) => current.Replace(nameChar, '_'));
    }
}