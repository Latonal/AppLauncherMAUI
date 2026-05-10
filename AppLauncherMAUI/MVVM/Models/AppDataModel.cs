namespace AppLauncherMAUI.MVVM.Models;

public class AppDataModel
{
    public int Id { get; set; }
    public DownloadUrlsModel? Download { get; set; }
    public ExecutionRuleModel[]? ExecutionRules { get; set; }
    public AppearanceModel? Appearance { get; set; }
}

public class DownloadUrlsModel
{
    public string[]? Install { get; set; }
    public string[]? Update { get; set; }
    public VersionFileModel? Version { get; set; }
}

public class VersionFileModel
{
    public string[]? Remote { get; set; }
    public string? Local { get; set; }
}

public class AppearanceModel
{
    public string? Name { get; set; }
    public LanguagesModel? Description { get; set; }
    public BannersModel? Banners { get; set; }
    public MediaModel[]? Medias { get; set; }

}