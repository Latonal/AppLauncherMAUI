using System.Text.Json.Serialization;

namespace AppLauncherMAUI.MVVM.Models.RawDownloadModels;

public class GithubRawModel : IRawDownload
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("download_url")]
    public string? Download_url { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("sha")]
    public string? Sha { get; set; }

    [JsonPropertyName("size")]
    public int? Size { get; set; }
}
