using AppLauncherMAUI.MVVM.Models.RawDownloadModels;
using AppLauncherMAUI.Utilities.Interfaces;
using AppLauncherMAUI.Utilities.Singletons;
using Microsoft.Maui.Graphics.Text;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AppLauncherMAUI.Utilities.DownloadUtilities;

internal class GithubDownloadHandler
{
    public static async Task<(List<StandardRawModel>, string)> GetGithubRawFiles(string downloadUrl, string appName, CancellationToken cancellationToken)
    {
        HttpClient client = HttpService.Client;

        UpdateTracker ut = new();
        await ut.Load(appName);

        if (ut.AUIM == null) return new();

        bool shouldWeCheckAgain = ut.AUIM.CanCheckUpdate();
        string sha = "";

        if (!shouldWeCheckAgain)
        {
            sha = ut.AUIM.LastDifferentHash ?? "";
            if (String.IsNullOrEmpty(sha))
                shouldWeCheckAgain = true;
            Console.WriteLine($"using sha: {sha}");
        }
        
        if (shouldWeCheckAgain)
        {
            HttpResponseMessage response = await client.GetAsync(downloadUrl, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[DownloadHandler] Error with GitHub API: {response.StatusCode}");
                return ([], "");
            }

            sha = await GetSha(response, cancellationToken);
        }

        if (String.IsNullOrEmpty(sha)) return ([], "");

        List<StandardRawModel> files = [];

        Uri uri = new(downloadUrl);
        string[] segments = uri.Segments;

        string owner = segments[2].TrimEnd('/');
        string repo = segments[3].TrimEnd('/');
        string branch = segments[5].TrimEnd('/');


        string treeUrl = $"https://api.github.com/repos/{owner}/{repo}/git/trees/{sha}?recursive=1";
        string standardRawUrl = $"https://raw.githubusercontent.com/{owner}/{repo}/{branch}/"; // add path

        HttpResponseMessage responseFullTree = await client.GetAsync(treeUrl, cancellationToken);
        string jsonTree = await responseFullTree.Content.ReadAsStringAsync(cancellationToken);

        List<GithubRawModel>? treeInfo = JsonSerializer.Deserialize<GithubTreeModel>(jsonTree)?.Tree;
        if (treeInfo?.Count <= 0 || treeInfo == null) return ([], "");

        foreach (GithubRawModel file in treeInfo)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (file.Type == "tree") continue;

            StandardRawModel converted = ModelConverter.RawGitToStandard(file);
            converted.DownloadUrl = standardRawUrl + converted.Path;
            files.Add(converted);
        }

        return (files, sha);
    }

    public static async Task<string> GetSha(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string json = await response.Content.ReadAsStringAsync(cancellationToken);

        BranchInfoModel? branchInfo = JsonSerializer.Deserialize<BranchInfoModel>(json);
        return branchInfo?.Commit?.Sha ?? "";
    }

    public static string GetSha(string json)
    {
        BranchInfoModel? branchInfo = JsonSerializer.Deserialize<BranchInfoModel>(json);
        return branchInfo?.Commit?.Sha ?? "";
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

    public static async Task<string?> GetCommitShaNoToken(string url, GithubUrlType guf = 0)
    {
        (string owner, string repo, string branch) = GithubUrlFormat(url, guf);

        string finalUrl = $"https://github.com/{owner}/{repo}/info/refs?service=git-upload-pack";

        using HttpResponseMessage response = await HttpService.Client.GetAsync(finalUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(content, $@"([0-9a-f]{{40}})\srefs/heads/{branch}");

        return match.Success ? match.Groups[1].Value : null;
    }

    public static async Task<string?> GetCommitSha(string url, GithubUrlType guf = 0)
    {
        (string owner, string repo, string branch) = GithubUrlFormat(url, guf);

        string finalUrl = $"https://api.github.com/repos/{owner}/{repo}/commits/{branch}";

        using HttpResponseMessage response = await HttpService.Client.GetAsync(finalUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        string content = await response.Content.ReadAsStringAsync();
        string? sha = JsonDocument.Parse(content).RootElement.GetProperty("sha").GetString();

        Debug.WriteLine($"Token left for {guf}: " + GetTokenLeft(response));

        return sha;
    }

    public static (string owner, string repo, string branch) GithubUrlFormat(string url, GithubUrlType guf)
    {
        Uri uri = new(url);
        string[] segments = uri.Segments;
        string owner;
        string repo;
        string branch;

        switch (guf)
        {
            case GithubUrlType.github:
                owner = segments[1].TrimEnd('/');
                repo = segments[2].TrimEnd('/');
                branch = segments[6].TrimEnd('/').Split(".")[0];
                break;
            case GithubUrlType.api:
                owner = segments[2].TrimEnd('/');
                repo = segments[3].TrimEnd('/');
                branch = segments[5].TrimEnd('/');
                break;
            default:
                throw new Exception($"[GithubDownloadHandler] url ({url}) is not supported.");
        }

        return (owner, repo, branch);
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

    public static int GetTokenLeft(HttpResponseMessage header)
    {
        HttpResponseHeaders headers = header.Headers;

        return headers.TryGetValues("X-RateLimit-Remaining", out IEnumerable<string>? rem) ? int.Parse(rem.First()) : 0;
    }

    public enum GithubUrlType {
        github, // github.com
        api // api.github.com
    }
}
