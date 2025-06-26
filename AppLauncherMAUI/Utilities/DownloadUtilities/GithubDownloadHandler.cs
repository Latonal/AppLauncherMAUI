using AppLauncherMAUI.MVVM.Models.RawDownloadModels;
using System.Text.Json;

namespace AppLauncherMAUI.Utilities.DownloadUtilities;

internal class GithubDownloadHandler
{
    public static async Task<List<StandardRawModel>> GetGithubRawFiles(string downloadUrl, CancellationToken cancellationToken)
    {
        List<StandardRawModel> files = [];
        Queue<string> queue = new();
        queue.Enqueue(downloadUrl);

        HttpClient client = HttpService.Client;

        while (queue.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string currentPath = queue.Dequeue();
            List<StandardRawModel> newFiles = await GetGithubContent(currentPath, client);

            foreach (StandardRawModel file in newFiles)
            {
                if (file.Type == "dir" && file.DirectoryUrl != null)
                    queue.Enqueue(file.DirectoryUrl);
                else if (file.Type == "file" && file.DownloadUrl != null)
                    files.Add(file);
            }
        }

        return files;
    }

    private static async Task<List<StandardRawModel>> GetGithubContent(string path, HttpClient client)
    {
        HttpResponseMessage response = await client.GetAsync(path);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"[DownloadHandler] Error with GitHub API: {response.StatusCode}");
            return [];
        }

        string json = await response.Content.ReadAsStringAsync();

        // one object
        if (json.TrimStart().StartsWith('{'))
        {
            GithubRawModel? singleFile = JsonSerializer.Deserialize<GithubRawModel>(json);
            return singleFile != null ? [ModelConverter.RawGitToStandard(singleFile)] : [];
        }

        List<GithubRawModel>? files = JsonSerializer.Deserialize<List<GithubRawModel>>(json);
        if (files == null || files.Count <= 0) return [];

        List<StandardRawModel> filesConverted = [];
        foreach (GithubRawModel file in files)
        {
            filesConverted.Add(ModelConverter.RawGitToStandard(file));
        }

        return files != null ? filesConverted : [];
    }
}
