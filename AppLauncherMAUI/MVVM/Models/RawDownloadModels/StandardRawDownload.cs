namespace AppLauncherMAUI.MVVM.Models.RawDownloadModels;

public class StandardRawModel : IRawDownload
{
    public string? Name { get; set; }
    public string? Path { get; set; }
    public string? Type { get; set; }
    public string? DownloadUrl { get; set; }
    public string? DirectoryUrl { get; set; }
    public string? Hash { get; set; }
    public int? Size { get; set; }
}
