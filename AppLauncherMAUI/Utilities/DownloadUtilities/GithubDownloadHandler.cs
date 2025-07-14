using AppLauncherMAUI.MVVM.Models.RawDownloadModels;
using AppLauncherMAUI.Utilities.Interfaces;
using Microsoft.Maui.Graphics.Text;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace AppLauncherMAUI.Utilities.DownloadUtilities;

internal class GithubDownloadHandler
{
    public static async Task<List<StandardRawModel>> GetGithubRawFiles(string downloadUrl, CancellationToken cancellationToken)
    {
        HttpClient client = HttpService.Client;

        HttpResponseMessage response = await client.GetAsync(downloadUrl, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[DownloadHandler] Error with GitHub API: {response.StatusCode}");
            return [];
        }

        List<StandardRawModel> files = [];

        Uri uri = new(downloadUrl);
        string[] segments = uri.Segments;

        string owner = segments[2].TrimEnd('/');
        string repo = segments[3].TrimEnd('/');
        string branch = segments[5].TrimEnd('/');

        string json = await response.Content.ReadAsStringAsync(cancellationToken);

        BranchInfoModel? branchInfo = JsonSerializer.Deserialize<BranchInfoModel>(json);
        string sha = branchInfo?.Commit?.Sha ?? "";
        if (sha == "") return [];

        string treeUrl = $"https://api.github.com/repos/{owner}/{repo}/git/trees/{sha}?recursive=1";
        string standardRawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/"; // add path

        HttpResponseMessage responseFullTree = await client.GetAsync(treeUrl, cancellationToken);
        string jsonTree = await responseFullTree.Content.ReadAsStringAsync(cancellationToken);

        List<GithubRawModel>? treeInfo = JsonSerializer.Deserialize<GithubTreeModel>(jsonTree)?.Tree;
        if (treeInfo?.Count <= 0 || treeInfo == null) return [];

        foreach (GithubRawModel file in treeInfo)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (file.Type == "tree") continue;

            StandardRawModel converted = ModelConverter.RawGitToStandard(file);
            converted.DownloadUrl = standardRawUrl + converted.Path;
            files.Add(converted);
        }

        return files;
    }

    public static bool CheckDownloadAvailability(HttpResponseMessage header)
    {
        HttpResponseHeaders headers = header.Headers;
        int requestsNeeded = 5;

        int remainingCount = headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? rem) ? int.Parse(rem.First()) : 0;

        if (remainingCount >= requestsNeeded)
            return true;

        // Todo : set a popup
        //long resetTime = header.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? res) ? int.Parse(res.First()) : 0;
        // if not ok : set x-ratelimit-reset - currentUnixTimestamp in popup with
        //Common.GetRemainingTime(resetTime);

        return false;
    }

    public static string GetGitFileSha1(string filePath)
    {
        byte[] content = File.ReadAllBytes(filePath);
        string header = $"blob {content.Length}\0";
        byte[] headerBytes = Encoding.UTF8.GetBytes(header);

        byte[] full = new byte[headerBytes.Length + content.Length];
        Buffer.BlockCopy(headerBytes, 0, full, 0, headerBytes.Length);
        Buffer.BlockCopy(content, 0, full, headerBytes.Length, content.Length);

        byte[] hashBytes = SHA1.HashData(full);
        return Convert.ToHexStringLower(hashBytes);
    }
}
