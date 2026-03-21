namespace AppLauncherMAUI.MVVM.Models;

public class AppDataModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string[]? InstallUrls { get; set; }
    public string[]? UpdateUrls { get; set; }
    public string[]? VersionUrls { get; set; }
    public LanguagesModel? Description { get; set; }
    public BannersModel? Banners { get; set; }
    public ExecutionRuleModel[]? ExecutionRules { get; set; }
    public MediaModel[]? Medias { get; set; }
}
