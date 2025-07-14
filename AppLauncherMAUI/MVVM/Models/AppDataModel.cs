namespace AppLauncherMAUI.MVVM.Models;

public class AppDataModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string[]? DownloadUrls { get; set; }
    public VersionCheckerModel[]? VersionChecker { get; set; }
    public LanguagesModel? Text { get; set; }
    public BannersModel? Banners { get; set; }
    public ExecutionRule[]? ExecutionRules { get; set; }
}
