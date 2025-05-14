using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;

namespace AppLauncherMAUI.Utilities;

internal class DownloadHandler
{
    static readonly HttpClient client = new();

    public static readonly string[] DownloadableContentTypes =
    [
        "application/octet-stream",
        //"application/x-msdownload",
        "application/zip",
        "application/x-zip",
        "application/x-zip-compressed",
        "application/rar",
        "application/x-rar",
        "application/x-rar-compressed",
        //"application/vnd.android.package-archive",
        //"application/x-msinstaller",
        //"application/pdf",
        //"application/x-tar",
    ];

    public static bool CheckValidUri(string url)
    {
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);

        //other:
        //return Uri.TryCreate(url, UriKind.Absolute, out _);
    }

    public static async Task<string?> GetHeader(string url)
    {

        HttpRequestMessage request = new(HttpMethod.Head, url);
        HttpResponseMessage response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
            return response.Content.Headers.ContentType?.MediaType;

        Console.Error.WriteLine("[DownloadHandler] Error when trying to get the header of \"" + url + "\"");
        return null;
    }

    public static async Task<bool> CheckIfValidHeader(string url)
    {
        if (!CheckValidUri(url)) return false;

        string? val = await GetHeader(url);
        Debug.WriteLine("Header value:" + val);
        return DownloadableContentTypes.Contains(val);
    }

    public static async Task DownloadFileAsync(Uri url, string destinationPath, IProgress<double>? progress = null)
    {
        try
        {
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            long totalBytes = response.Content.Headers.ContentLength ?? -1L;
            bool canTrackProgress = totalBytes != -1 && progress != null;

            using Stream stream = await response.Content.ReadAsStreamAsync();
            int bufferSize = 4096;
            using FileStream fileStream = new(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

            byte[] buffer = new byte[bufferSize];
            long totalRead = 0;
            int read;

            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
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

    public static void ExtractZip(string zipFilePath, string destinationPath)
    {
        if (Directory.Exists(destinationPath)) // ???? not so sure
            Directory.Delete(destinationPath, true);

        ZipFile.ExtractToDirectory(zipFilePath, destinationPath);
    }

    public static async Task<string> GetRemoteHash(Uri url)
    {
        return await client.GetStringAsync(url);
    }

    public static string GetLocalHash(string filePath)
    {
        using SHA256 s = SHA256.Create();
        using Stream stream = File.OpenRead(filePath);
        byte[] hash = s.ComputeHash(stream);
        return Convert.ToHexString(hash);
    }

    public static async Task CheckLogic(string localAppPath, Uri remoteHashUrl)
    {
        if (GetLocalHash(localAppPath) != await GetRemoteHash(remoteHashUrl))
            //await DownloadFileAsync(localAppPath, remoteHashUrl); // wrong atm
            Debug.WriteLine("Update app");
        else
            Debug.WriteLine("Launch app");
    }
}
