using System.Diagnostics;

namespace AppLauncherMAUI.Utilities;

internal static class ApplicationHandler
{
    public static string[]? ReturnFilesByPatterns(string path)
    {
        // add more pattern possible with parameter
        string[] patterns = [".exe", ".lnk", "main", "index.html", "index"];
        string[]? foundFiles = TryFindMatchingFile(DownloadHandler.GetDefaultAppPath(path, false), patterns);

        Debug.WriteLine("Found files:" + (foundFiles?.Length > 0 ? string.Join("", foundFiles) : ""));

        return foundFiles;
    }

    public static string[]? TryFindMatchingFile(string folderPath, string[] patterns)
    {
        if (!Directory.Exists(folderPath))
            return [];

        foreach (var pattern in patterns)
        {
            string trimmed = pattern.Trim();

            if (string.IsNullOrWhiteSpace(trimmed))
                continue;

            // looking for files with a specific extension (.exe, .html...)
            // working examples: ".exe", "exe"
            if (!trimmed.Contains('.') || trimmed.StartsWith('.'))
            {
                string extension = trimmed.StartsWith('.') ? trimmed : "." + trimmed;
                string[] files = Directory.GetFiles(folderPath, $"*{extension}", SearchOption.TopDirectoryOnly);

                if (files.Length > 0)
                    return files;
            }

            // looking for files with a specific name (main, index...)
            else if (!Path.HasExtension(trimmed))
            {
                string[] matches = Directory.GetFiles(folderPath, trimmed, SearchOption.TopDirectoryOnly);

                if (matches != null)
                    return matches;
            }

            // looking for a file with the corresponding name and extension
            // working examples : "main.exe", "index.html"
            else
            {
                string fullPath = Path.Combine(folderPath, trimmed);
                if (File.Exists(fullPath))
                    return [fullPath];
            }
        }

        return [];
    }
}
