using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AppLauncherUpdaterMAUI
{
    // Todo: test this whole app when the other is ready

    public partial class MainPage : ContentPage
    {
        static string appName = "appName";
        static string version = "version";

        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (OperatingSystem.IsIOS())
            {
                // Todo: make an alternative console app?
                Console.Error.WriteLine("The updater is not supported by your OS.");
                StatusLabel.Text = "Could not update. Your OS does not allow it. Please update it manually.";
                return;
            }

            string baseDir = AppContext.BaseDirectory;
            string otherAppName = "Set the other app name here";

            string otherAppExePath = Path.Combine(baseDir, otherAppName);
            string otherAppProcessName = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(otherAppExePath))[0].ProcessName;
            string[] downloadUrls = ["working api url", "working zip url"]; // Todo: replace urls
            string tempFilePath = Path.Combine(FileSystem.CacheDirectory, "UpdatedZip");

            // Todo: Compare local version and versionFileUrl
            //string versionFileUrl = "url to version file or check hash?";

            (HttpResponseMessage response, HeaderType headerType, string workingUrl) = await GetHeaderType(downloadUrls);

            if (workingUrl == "" || workingUrl == String.Empty)
            {
                StatusLabel.Text = "An error happened while retrieving the header type. Please try again later.";
                return;
            }

            KillOtherApp(otherAppProcessName);
            StatusLabel.Text = "Downloading...";

            await UpdateApplication(response, headerType, workingUrl, tempFilePath, otherAppExePath);

            StatusLabel.Text = "Launching application";
            try
            {
                Process.Start(otherAppExePath);
            }
            catch { }

            await Task.Delay(1000);
            Application.Current!.Quit();
        }

        public static HttpClient GetNewClient()
        {
            HttpClient httpClient = new()
            {
                Timeout = TimeSpan.FromMinutes(5),
            };
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue($"{appName}", $"{version}"));

            return httpClient;
        }

        public static async Task<(HttpResponseMessage responsee, HeaderType headerType, string workingUrl)> GetHeaderType(string[] urls)
        {
            HttpClient client = GetNewClient();
            string headerValue = "";
            string workingUrl = "";

            HttpResponseMessage response = new();

            foreach (string url in urls)
            {
                response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode || !CheckDownloadAvailability(response, url))
                    continue;

                workingUrl = url;
                headerValue = response.Content.Headers.ContentType?.MediaType ?? "";
            }

            if (headerValue == null) return (response, HeaderType.None, String.Empty);

            return headerValue switch
            {
                "application/zip" or "application/x-zip" or "application/x-zip-compressed" => (response, HeaderType.Zip, workingUrl),
                "application/json" => (response, HeaderType.Json, workingUrl),
                "application/octet-stream" or "application/vnd.microsoft.portable-executable" => (response, HeaderType.OctetStream, workingUrl),

                _ => (new HttpResponseMessage(), HeaderType.None, String.Empty),
            };
        }

        public static bool CheckDownloadAvailability(HttpResponseMessage header, string url)
        {
            Uri uri = new(url);

            string host = uri.Host;
            return host switch
            {
                "api.github.com" => CheckGithubApiDownloadAvailability(header),
                "github.com" => true,
                _ => false,
            };
        }

        public static bool CheckGithubApiDownloadAvailability(HttpResponseMessage header)
        {
            HttpResponseHeaders headers = header.Headers;
            int requestsNeeded = 5;

            int remainingCount = headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? rem) ? int.Parse(rem.First()) : 0;

            if (remainingCount >= requestsNeeded)
                return true;

            return false;
        }

        public static void KillOtherApp(string processName)
        {
            if (OperatingSystem.IsIOS()) return;

            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    // Todo : write error log
                }
            }
        }

        public async Task UpdateApplication(HttpResponseMessage response, HeaderType headerType, string workingUrl, string tempPath, string destinationPath)
        {
            switch (headerType)
            {
                case HeaderType.Zip:
                    await DownloadZip(response, tempPath);
                    StatusLabel.Text = "Extracting files...";
                    ExtractingZipWithException(tempPath, destinationPath);
                    break;
                case HeaderType.Json:
                    List<GithubRawModel> files = await GetRawFiles(response, workingUrl);
                    StatusLabel.Text = "Replacing files...";
                    DownloadRawFiles(files);
                    break;
                case HeaderType.OctetStream:
                default:
                    Console.Error.WriteLine("Header type not supported");
                    break;
            }
        }

        #region zip
        public async Task DownloadZip(HttpResponseMessage response, string zipPath)
        {
            long totalBytes = response.Content.Headers.ContentLength ?? -1L;
            bool canTrackProgress = totalBytes != -1 && DownloadProgress != null;

            using Stream stream = await response.Content.ReadAsStreamAsync();
            int bufferSize = 4096;
            using FileStream fileStream = new(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

            byte[] buffer = new byte[bufferSize];
            long totalRead = 0;
            int read;

            while ((read = await stream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read));
                totalRead += read;

                if (canTrackProgress)
                    DownloadProgress!.Progress = (double)totalRead / totalBytes;
            }

            DownloadProgress!.Progress = 1;
        }

        public static void ExtractingZipWithException(string zipPath, string destinationPath)
        {
            Directory.CreateDirectory(destinationPath);

            using ZipArchive archive = ZipFile.OpenRead(zipPath);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                string destinationFilePath = Path.Combine(destinationPath, entry.FullName);

                string currentProgramPath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                if (Path.GetFileName(destinationFilePath).Equals(currentProgramPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                string? destinationDir = Path.GetDirectoryName(destinationFilePath);
                if (!string.IsNullOrWhiteSpace(destinationDir))
                    Directory.CreateDirectory(destinationDir);

                if (File.Exists(destinationFilePath))
                    File.Delete(destinationFilePath);

                entry.Open().CopyTo(File.Create(destinationFilePath));
            }
        }
        #endregion zip

        #region json
        public async Task<List<GithubRawModel>> GetRawFiles(HttpResponseMessage response, string workingUrl)
        {
            Uri? uri = new(workingUrl);
            string[] segments = uri.Segments;

            string owner = segments[2].TrimEnd('/');
            string repo = segments[3].TrimEnd('/');
            string branch = segments[5].TrimEnd('/');

            string json = await response.Content.ReadAsStringAsync();

            BranchInfoModel? branchInfo = JsonSerializer.Deserialize<BranchInfoModel>(json);
            string sha = branchInfo?.Commit?.Sha ?? "";
            if (sha == "") return [];

            string treeUrl = $"https://api.github.com/repos/{owner}/{repo}/git/trees/{sha}?recursive=1";
            string standardRawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/";

            HttpResponseMessage responseFullTree = await GetNewClient().GetAsync(treeUrl);
            string jsonTree = await responseFullTree.Content.ReadAsStringAsync();

            List<GithubRawModel>? treeInfo = JsonSerializer.Deserialize<GithubTreeModel>(jsonTree)?.Tree;
            if (treeInfo?.Count <= 0 || treeInfo == null) return [];

            return treeInfo;
        }

        public static void DownloadRawFiles(List<GithubRawModel> filesToDownload)
        {
            // Todo: Download & replace existing except the current process - compare files with sha
        }
        #endregion json



        public enum HeaderType
        {
            Json,
            Zip,
            OctetStream,
            None
        }
    }
}

public class GithubRawModel
{
    [JsonPropertyName("path")]
    public string? Path { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("download_url")]
    public string? Download_url { get; set; }
    [JsonPropertyName("sha")]
    public string? Sha { get; set; }
    [JsonPropertyName("size")]
    public string? Size { get; set; }
}

public class GithubTreeModel
{
    [JsonPropertyName("tree")]
    public List<GithubRawModel>? Tree { get; set; }
}

public class BranchInfoModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("commit")]
    public CommitInfoModel? Commit { get; set; }
}

public class CommitInfoModel
{
    [JsonPropertyName("sha")]
    public string? Sha { get; set; }
}