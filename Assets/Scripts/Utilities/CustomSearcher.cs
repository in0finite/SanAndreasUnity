using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class CustomSearcher
{
    public static IEnumerable<string> GetDirectories(string path, string searchPattern = "*",
        SearchOption searchOption = SearchOption.TopDirectoryOnly)
    {
        if (searchOption == SearchOption.TopDirectoryOnly)
            return Directory.GetDirectories(path, searchPattern).AsEnumerable();

        var directories = GetDirectories(path, searchPattern).ToList();

        for (var i = 0; i < directories.Count(); i++)
            directories.AddRange(GetDirectories(directories.ElementAt(i), searchPattern));

        return directories.AsEnumerable();
    }

    private static IEnumerable<string> GetDirectories(string path, string searchPattern)
    {
        try
        {
            return Directory.GetDirectories(path, searchPattern).AsEnumerable();
        }
        catch (UnauthorizedAccessException)
        {
            return default(IEnumerable<string>);
        }
    }
}