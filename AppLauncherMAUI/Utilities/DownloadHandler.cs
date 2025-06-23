using AppLauncherMAUI.Config;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Xml.Linq;

namespace AppLauncherMAUI.Utilities;

internal class DownloadHandler
{
    private static readonly HttpClient client = HttpService.Client;

    public static bool CheckValidUri(string url)
    {
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);

        //other:
        //return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    public static async Task<string?> GetHeaderAsync(string url)
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Head, url);
            using HttpResponseMessage response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentType?.MediaType;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"(DownloadHandler) [HttpRequestException] {ex.Message}");
        }

        return null;
    }

    public static async Task<bool> CheckIfValidHeader(string url)
    {
        if (!CheckValidUri(url)) {
            Console.Error.WriteLine("[DownloadHandler] The given url (" + url + ") is not well formatted.");
            return false;
        }

        string? val = await GetHeaderAsync(url);
        if (val == null) return false;
        return ExternalApplicationManager.GetAllowedContentType(val) != ExternalApplicationManager.AllowedContentType.Unknown;
    }

    public static async Task<ExternalApplicationManager.AllowedContentType> GetContentType(string url)
    {
        if (!CheckValidUri(url))
        {
            Console.Error.WriteLine("[DownloadHandler] The given url (" + url + ") is not well formatted.");
            return ExternalApplicationManager.AllowedContentType.Unknown;
        }

        string? val = await GetHeaderAsync(url);
        if (val == null) return ExternalApplicationManager.AllowedContentType.Unknown;
        return ExternalApplicationManager.GetAllowedContentType(val);
    }

    public static async Task DownloadFileAsync(string url, string filePath, CancellationToken cancellationToken, IProgress<double>? progress = null)
    {
        /* Test
        //Debug.WriteLine(progress?.ToString());
        //for (int i = 1; i <= 100; i++)
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    await Task.Delay(50);
        //    progress?.Report(i / 100.0);
        //    //Debug.WriteLine("Progress: " + i);
        //} 
        */

        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1L;
            bool canTrackProgress = totalBytes != -1 && progress != null;

            using Stream stream = await response.Content.ReadAsStreamAsync();
            int bufferSize = 4096;
            using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

            byte[] buffer = new byte[bufferSize];
            long totalRead = 0;
            int read;

            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await fileStream.WriteAsync(buffer.AsMemory(0, read));
                totalRead += read;

                if (canTrackProgress)
                    progress?.Report((double)totalRead / totalBytes);
            }

            progress?.Report(1);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Message:{0}", e.Message);
        }
    }

    public static string GetDefaultZipPath(string fileName, bool createFolder = true)
    {
        string filePath = Path.Combine(AppPaths.CacheDirectory, "TempZip");
        if (createFolder) 
            Directory.CreateDirectory(filePath);
        if (fileName != null)
            filePath = Path.Combine(filePath, fileName + ".zip");
        return filePath;
    }

    public static string GetDefaultAppPath(string fileName, bool createFolder = true)
    {
        string filePath = AppPaths.DownloadedAppPath(fileName);
        if (createFolder)
            Directory.CreateDirectory(filePath);
        return filePath;
    }

    public static void ExtractZip(string zipFilePath, string destinationPath)
    {
        Directory.CreateDirectory(destinationPath);

        ZipFile.ExtractToDirectory(zipFilePath, destinationPath, true);
    }

    /// <summary>
    /// !!Be cautious when using this method!!
    /// <para>If a file is given as a path, it will delete the parent folder and its content.</para>
    /// <para>If a folder is given as a path, it will delete it and its content.</para>
    /// </summary>
    /// <param name="path">Path to clean</param>
    public static void DeleteFolder(string path)
    {
        if (path == null) return;
        if (File.Exists(path))
            path = Path.GetDirectoryName(path) ?? "";

        if (path == "") return;

        if (Directory.Exists(path))
            Directory.Delete(path, true);
    }

    /// <summary>
    /// Auto clean the destination path if there is only one folder as root
    /// by putting back up of one level all of the files and folder in it.
    /// (As .zip always have a root folder when extracted)
    /// </summary>
    /// <param name="destinationPath">Path where the application has been extracted to</param>
    public static void CleanDestinationPath(string destinationPath)
    {
        string[] directories = Directory.GetDirectories(destinationPath);
        string[] files = Directory.GetFiles(destinationPath);

        if (directories.Length == 1 && files.Length == 0)
        {
            string innerFolder = directories[0];

            foreach (string file in Directory.GetFiles(innerFolder))
            {
                string destinationFile = Path.Combine(destinationPath, Path.GetFileName(file));
                File.Move(file, destinationFile, true);
            }
            foreach (string folder in Directory.GetDirectories(innerFolder))
            {
                string destinationDir = Path.Combine(destinationPath, Path.GetFileName(folder));
                Directory.Move(folder, destinationDir);
            }

            Directory.Delete(innerFolder, true);
        }
    }

    public static async Task<string> GetRemoteHash(string url)
    {
        string hash = await client.GetStringAsync(url);
        return hash;
    }

    public static string GetLocalHash(string filePath)
    {
        using SHA256 s = SHA256.Create();
        using Stream stream = File.OpenRead(filePath);
        byte[] hash = s.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    public static async Task CheckLogic(string localAppPath, string remoteHashUrl)
    {
        if (GetLocalHash(localAppPath) != await GetRemoteHash(remoteHashUrl))
            //await DownloadFileAsync(localAppPath, remoteHashUrl); // wrong atm
            Debug.WriteLine("Update app");
        else
            Debug.WriteLine("Launch app");
    }
}
