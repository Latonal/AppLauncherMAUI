using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.Utilities;
using System.Diagnostics;

namespace AppLauncherMAUI.MVVM.ViewModels;

internal partial class SingleAppViewModel : ExtendedBindableObject
{
    private int _appId = 0;
    public int AppId { get { return _appId; } set { _appId = value; SetData(value); } }

    private string? _text;
    public string? Text { get { return _text; } set { _text = value; RaisePropertyChanged(() => Text); } }

    private string? _name;
    public string? Name { get { return _name; } set { _name = value; RaisePropertyChanged(() => Name); } }

    private string? _fullBanner;
    public string? FullBanner { get { return _fullBanner; } set { _fullBanner = value; RaisePropertyChanged(() => FullBanner); } }

    public SingleAppViewModel(int appId)
	{
        AppId = appId;
	}

    private async void SetData(int id)
    {
        AppDataModel data = await GetData(id);

        Name = data.Name;
        FullBanner = data.Banners?.Full;

        LanguagesModel? texts = data.Text;
        if (texts != null)
            Text = Common.GetTranslatedJsonText(texts);
    }

    private static async Task<AppDataModel> GetData(int id)
    {
        return await JsonFileManager.ReadSingleDataAsync<AppDataModel>(AppPaths.AppsDataJsonName, "Id", id);
    }
}