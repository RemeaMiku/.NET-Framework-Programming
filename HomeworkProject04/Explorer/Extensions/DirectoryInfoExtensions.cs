using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Explorer.Extensions;

public static class DirectoryInfoExtensions
{
    static readonly EnumerationOptions _options = new()
    {
        IgnoreInaccessible = true,
        RecurseSubdirectories = false,
    };
    public static bool HasAccessibleSubdirectory(this DirectoryInfo directoryInfo)
    => EnumerateAccessibleDirectories(directoryInfo).Any();

    public static IEnumerable<DirectoryInfo> EnumerateAccessibleDirectories(this DirectoryInfo directoryInfo)
    {
        foreach (var info in directoryInfo.EnumerateDirectories("*", _options))
            yield return info;
    }
    public static IEnumerable<FileInfo> EnumerateAccessibleFiles(this DirectoryInfo directoryInfo)
    {
        foreach (var info in directoryInfo.EnumerateFiles("*", _options))
            yield return info;
    }
}
