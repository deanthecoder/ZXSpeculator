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

using System.Reflection;
using System.Runtime.InteropServices;

namespace CSharp.Utils.Extensions;

public static class AssemblyExtensions
{
    public static string GetProductName(this Assembly assembly) =>
        assembly.GetName().Name;

    public static DirectoryInfo GetDirectory(this Assembly assembly) =>
        new FileInfo(assembly.Location).Directory;

    /// <summary>
    /// Return a directory suitable for storing user-specific application settings.
    /// </summary>
    public static DirectoryInfo GetAppSettingsPath(this Assembly assembly)
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        if (string.IsNullOrEmpty(appDataPath))
        {
            var homePath = Environment.GetEnvironmentVariable("HOME");
            if (string.IsNullOrEmpty(homePath))
            {
                // Fallback to using ~ if HOME environment variable is not set
                homePath = "~";
            }

            appDataPath = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Path.Combine(homePath, "Library", "Preferences") : homePath;
        }

        return new DirectoryInfo(appDataPath).CreateSubdirectory(assembly.GetProductName().ToSafeFileName());
    }
}
