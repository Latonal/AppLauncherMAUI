using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.MVVM.Models.RawDownloadModels;
using AppLauncherMAUI.Utilities.Singletons;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http.Headers;

namespace AppLauncherMAUI.Utilities.DownloadUtilities;

internal class DownloadHandler
{
    public static async Task<bool> Download(string appPath, string[] urls, CancellationToken cancellationToken, IProgress<double>? progress = null)
    {
        foreach (string url in urls)
        {
            bool done = await Download(appPath, url, cancellationToken, progress);
            if (done) return true;
        }

        return false;
    }

    public static async Task<bool> Download(string appPath, string url, CancellationToken cancellationToken, IProgress<double>? progress = null)
    {
        try
        {
            string host = Common.GetUriHost(url);
            if (string.IsNullOrEmpty(host)) return false;

            DomainDownloadModel ddm = GlobalData.DDMList.Search(host);
            if (!ddm.IsDownloadable()) return false;

            HttpClient client = HttpService.Client;
            HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

            // Header
            HttpContentHeaders headers = response.Content.Headers;
            if (headers == null || headers.ContentType?.MediaType == null) return false;

            ddm.Update(headers);

            ExternalApplicationManager.AllowedContentType contentType = ExternalApplicationManager.GetAppAllowedContentType(headers.ContentType.MediaType);
            if (contentType == ExternalApplicationManager.AllowedContentType.Unknown) return false;

            // Body
            string body = await response.Content.ReadAsStringAsync();

            switch (contentType)
            {
                case ExternalApplicationManager.AllowedContentType.Zip:
                    await DownloadHandler.DownloadZipContent(url, appPath, cancellationToken, progress);
                    break;
                case ExternalApplicationManager.AllowedContentType.Json:
                    await DownloadHandler.DownloadRawContent(url, appPath, cancellationToken, progress);
                    break;
                default:
                    Console.Error.WriteLine("(DownloadHandler) Type '" + contentType + "' is not supported for downloading.");
                    return false;
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine("Message:{0}", e.Message);
            return false;
        }

        return true;
    }
    public static async Task DownloadFileAsync(string url, string filePath, CancellationToken cancellationToken, IProgress<double>? progress = null, string? sha = null)
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
            // todo: use sha for downloadurl

            using HttpResponseMessage response = await HttpService.Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
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

            if (sha != null)
                await SaveVersionFile(Common.GetWorkingDirectory(filePath), sha);

            progress?.Report(1);
        }
        catch (Exception e)
        {
            Debug.WriteLine("Message:{0}", e.Message);
        }
    }

    public static async Task SaveVersionFile(string appName, string hash)
    {
        UpdateTracker ut = new();
        appName = Path.GetFileNameWithoutExtension(appName.ToLower());

        await ut.Load(appName);
        ut.AUIM?.Update(new AppUpdateInfosModel { Name = appName, Hash = hash, LastDifferentHash = hash, LastChecked = Common.GetCurrentUnixTimestamp() });
        if (ut.AUIM != null)
            await UpdateTracker.Save(ut.AUIM);
    }

    #region Download content by type
    public static async Task DownloadZipContent(string downloadUrl, string appPath, CancellationToken cancellationToken, IProgress<double>? progress)
    {
        try
        {
            string zipPath = GetDefaultZipPath(Common.GetWorkingDirectory(appPath));

            string? sha = await GetCommit(downloadUrl);
            await DownloadFileAsync(downloadUrl, zipPath, cancellationToken, progress, sha);
            DeleteFolder(appPath);
            ExtractZip(zipPath, appPath);
            DeleteFolder(zipPath);
            CleanDestinationPath(appPath);
        }
        catch (Exception ex)
        {
            HandleDownloadException(ex, "zip");
        }

        return;
    }

    public static async Task<string?> GetCommit(string url)
    {
        string host = Common.GetUriHost(url);
        // add more host in switch if needed

        // no token used
        string? sha = host switch
        {
            "github.com" => await GithubDownloadHandler.GetCommitShaNoToken(url, GithubDownloadHandler.GithubUrlType.github),
            "api.github.com" => await GithubDownloadHandler.GetCommitShaNoToken(url, GithubDownloadHandler.GithubUrlType.api),
            _ => throw new Exception($"[DownloadHandler] url ({host}) hostname is not supported."),
        };

        // token used if empty
        sha ??= host switch
        {
            "github.com" => await GithubDownloadHandler.GetCommitShaNoToken(url, GithubDownloadHandler.GithubUrlType.github),
            "api.github.com" => await GithubDownloadHandler.GetCommitShaNoToken(url, GithubDownloadHandler.GithubUrlType.api),
            _ => throw new Exception($"[DownloadHandler] url ({host}) hostname is not supported."),
        };

        return sha;
    }

    #region Raw download
    private static async Task<(List<StandardRawModel>, string)> GetFilesDataByModelType(string downloadUrl, string path, CancellationToken cancellationToken)
    {
        string host = Common.GetUriHost(downloadUrl);
        string appName = Common.GetWorkingDirectory(path);

        return host switch
        {
            "api.github.com" => await GithubDownloadHandler.GetGithubRawFiles(downloadUrl, appName, cancellationToken),
            _ => throw new Exception($"[DownloadHandler] url ({downloadUrl}) hostname is not supported."),
        };
    }

    public static async Task DownloadRawContent(string downloadUrl, string appPath, CancellationToken cancellationToken, IProgress<double>? progress)
    {
        string appName = Common.GetWorkingDirectory(appPath);

        (List<StandardRawModel> files, string hash) = await GetFilesDataByModelType(downloadUrl, appName, cancellationToken);
        if (files == null || files.Count <= 0) throw new Exception($"[DownloadHandler] url ({downloadUrl}) returned no file.");

        await DownloadRawFiles(files, hash, appPath, appName, downloadUrl, cancellationToken, progress);
    }

    public static async Task DownloadRawFiles(List<StandardRawModel> files, string hash, string appPath, string appName, string downloadUrl, CancellationToken cancellationToken, IProgress<double>? progress)
    {
        int totalBytes = files.Sum(x => x.Size) ?? 0;
        int totalRead = 0;
        int filesDownloaded = 0;

        HttpClient client = HttpService.Client;

        foreach (StandardRawModel file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(file.DownloadUrl) || string.IsNullOrEmpty(file.Path)) continue;

            try
            {
                string destinationPath = Path.Combine(appPath, file.Path);
                string directory = Path.GetDirectoryName(destinationPath)!;

                Directory.CreateDirectory(directory);

                if (File.Exists(destinationPath))
                {
                    if (CompareFiles(destinationPath, downloadUrl, file.Hash ?? ""))
                    {
                        totalRead += file.Size ?? 0;
                        continue;
                    }
                }

                await DownloadFileWithRetry(file.DownloadUrl, destinationPath, client, cancellationToken);

                filesDownloaded++;
                totalRead += file.Size ?? 0;
                progress?.Report((double)totalRead / totalBytes);
            }
            catch (Exception ex)
            {
                throw new Exception($"[DownloadHandler] Error when downloading ({file.DownloadUrl}) at path {file.Path}. {ex.Message}");
            }
        }

        await SaveVersionFile(appName, hash);

        progress?.Report(1);
    }

    private static async Task DownloadFileWithRetry(string downloadUrl, string destinationPath, HttpClient client, CancellationToken cancellationToken)
    {
        int maxRetries = 3;
        int retryDelay = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using Stream stream = await client.GetStreamAsync(downloadUrl, cancellationToken).ConfigureAwait(false);
                int bufferSize = 4096;
                using FileStream fileStream = new(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

                await stream.CopyToAsync(fileStream, cancellationToken);
                return;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
                retryDelay *= 2;
            }
            catch (Exception ex)
            {
                if (i < maxRetries - 1)
                    await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
                else
                {
                    Debug.WriteLine($"[DownloadHandler] (Exception) {ex.Message}");
                    throw;
                }
            }
        }
    }
    #endregion Raw download

    public static bool CompareFiles(string filePath, string downloadUrl, string receivedSha)
    {
        string host = Common.GetUriHost(downloadUrl);

        return host switch
        {
            "api.github.com" => GithubDownloadHandler.GetGitFileSha1(filePath) == receivedSha,
            _ => throw new Exception($"[DownloadHandler] url ({downloadUrl}) hostname is not supported."),
        };
    }

    private static void HandleDownloadException(Exception ex, string from)
    {
        if (ex is OperationCanceledException)
        {
            Console.WriteLine("Stopped Download");
        }
        else
        {
            throw new Exception($"(SingleAppViewModel) Something happened while downloading ({from}): {ex.Message}");
        }
    }
    #endregion Download content by type

    // TODO: Redo
    public static string GetDefaultZipPath(string fileName, bool createFolder = true)
    {
        string filePath = Path.Combine(AppPaths.CacheDirectory, "TempZip");
        if (createFolder)
            Directory.CreateDirectory(filePath);
        if (fileName != null)
            filePath = Path.Combine(filePath, fileName + ".zip");
        return filePath;
    }

    // TODO: Redo
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
        if (String.IsNullOrEmpty(path)) return;
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
}
