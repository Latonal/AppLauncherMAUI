namespace AppLauncherMAUI.MVVM.Models;

public class AppDataModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public LanguagesModel? Text { get; set; }
    public BannersModel? Banners { get; set; }
}
