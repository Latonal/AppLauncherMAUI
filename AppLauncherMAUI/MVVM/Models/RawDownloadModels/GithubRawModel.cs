using System.Text.Json.Serialization;

namespace AppLauncherMAUI.MVVM.Models.RawDownloadModels;

public class GithubRawModel : IRawDownload
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
    public int? Size { get; set; }
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