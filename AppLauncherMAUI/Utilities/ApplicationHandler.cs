using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.Utilities.DownloadUtilities;
using System.Data;
using System.Diagnostics;

namespace AppLauncherMAUI.Utilities;

internal static class ApplicationHandler
{
    public static string[]? ReturnFilesByPatterns(string path, ExecutionRule[]? executionRules = null)
    {
        // add more pattern possible with parameter
        ExecutionRule[] patterns = [
            new() { Type = "Extension", Value = "exe" },
            new() { Type = "Extension", Value = "lnk" },
            new() { Type = "Name", Value = "main" },
            new() { Type = "ExactMatch", Value = "index.html" },
            new() { Type = "Name", Value = "index" },
            new() { Type = "Extension", Value = "md" },
        ];

        if (executionRules != null && executionRules.Length > 0)
            patterns = [.. executionRules, .. patterns];

        Debug.WriteLine("----------------Each patterns:");

        string[]? foundFiles = TryFindMatchingFile(DownloadHandler.GetDefaultAppPath(path, false), patterns);

        Debug.WriteLine("Found files:" + (foundFiles?.Length > 0 ? string.Join("", foundFiles) : ""));

        return foundFiles;
    }

    public static string[]? TryFindMatchingFile(string folderPath, ExecutionRule[] executionRules)
    {
        if (!Directory.Exists(folderPath))
            return [];

        foreach (ExecutionRule executionRule in executionRules)
        {
            string? val = null;
            if (executionRule.Value != null && !executionRule.Type.Equals("metadata", StringComparison.CurrentCultureIgnoreCase))
                val = executionRule.Value.Trim();

            switch (executionRule.Type.ToLower()) {
                case "metadata":
                    string[] executables = Directory.GetFiles(folderPath, $"*.exe", SearchOption.TopDirectoryOnly);
                    if (executables.Length == 0) continue;
                    foreach (string executable in executables)
                    {
                        FileVersionInfo infos = FileVersionInfo.GetVersionInfo(executable);
                        bool allMatch = executionRule.Conditions != null && executionRule.Conditions.All(cond => {
                            string? currentMetadata = GetMetadataValue(infos, cond.Property);
                            return currentMetadata != null && currentMetadata.Equals(cond.Value, StringComparison.OrdinalIgnoreCase);
                        });

                        if (allMatch) return [executable];
                    }
                    break;
                case "extension":
                    if (val == null) continue;

                    string extension = val.StartsWith('.') ? val : "." + val;
                    string[] files = Directory.GetFiles(folderPath, $"*{extension}", SearchOption.TopDirectoryOnly);

                    if (files.Length > 0)
                        return files;
                    break;
                case "name":
                    // does not work
                    if (val == null) continue;

                    string[] matches = Directory.GetFiles(folderPath, $"*{val}*", SearchOption.TopDirectoryOnly);
                    if (matches.Length > 0)
                        return matches;
                    break;
                case "exactmatch":
                    if (val == null) continue;

                    string fullPath = Path.Combine(folderPath, val);
                    if (File.Exists(fullPath))
                        return [fullPath];
                    break;
                default:
                    Console.Error.WriteLine($"(ApplicationHandler) Rule type is not recognized: {executionRule.Type}");
                    break;
            }
        }

        return [];
    }

    public static string? GetMetadataValue(FileVersionInfo info, string property) 
    {
        return property.ToLower() switch
        {
            "productname" => info.ProductName,
            "companyname" => info.CompanyName,
            "filedescription" => info.FileDescription,
            "internalname" => info.InternalName,
            "fileversion" => info.FileVersion,
            "originalfilename" => info.OriginalFilename,
            _ => null,
        };
    }

    public static string[]? TryFindMatchingFilee(string folderPath, string[] patterns)
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
