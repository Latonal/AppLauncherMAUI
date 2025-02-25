namespace AppLauncherMAUI.MVVM.Models;

public class AppDataModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Text { get; set; }
    public BannersModel? Banners { get; set; }
}
