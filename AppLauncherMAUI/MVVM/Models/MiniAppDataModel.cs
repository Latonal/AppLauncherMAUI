namespace AppLauncherMAUI.MVVM.Models;

public class MiniAppDataModel
{
    public int Id { get; set; }
    public MiniAppAppearanceModel? Appearance { get; set; }
}

public class MiniAppAppearanceModel
{
    public string? Name { get; set; }
    public BannersModel? Banners { get; set; }
}