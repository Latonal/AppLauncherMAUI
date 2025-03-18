using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.MVVM.Views.Controls;
using AppLauncherMAUI.Utilities;
using System.Windows.Input;

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
    private string? _appCardFullDescription;
    public string? AppCardFullDescription { get { return _appCardFullDescription; } set { _appCardFullDescription = value; RaisePropertyChanged(() => AppCardFullDescription); } }

    private AppDownloadButtonStates? _downloadButtonState;
    public AppDownloadButtonStates? DownloadButtonState { get { return _downloadButtonState; } set { _downloadButtonState = value; RaisePropertyChanged(() => DownloadButtonState); } }
    public ICommand DownloadButtonStateCommand { get; set; }



    public SingleAppViewModel(int appId)
	{
        AppId = appId;

        DownloadButtonState = AppDownloadButtonStates.ToDownload;
        DownloadButtonStateCommand ??= new Command(ActionDownloadButtonClicked);
	}

    private async void SetData(int id)
    {
        AppDataModel data = await GetData(id);

        Name = data.Name;
        FullBanner = data.Banners?.Full;

        LanguagesModel? texts = data.Text;
        if (texts != null)
            Text = Common.GetTranslatedJsonText(texts);

        LanguagesModel? desc = data.Banners?.FullDescription;
        if (desc != null)
            AppCardFullDescription = Common.GetTranslatedJsonText(desc);
    }

    private static async Task<AppDataModel> GetData(int id)
    {
        return await JsonFileManager.ReadSingleDataAsync<AppDataModel>(AppPaths.AppsDataJsonName, "Id", id);
    }

    private void ActionDownloadButtonClicked()
    {
        if (DownloadButtonState == AppDownloadButtonStates.ToDownload)
            DownloadButtonState = AppDownloadButtonStates.Downloading;
    }
}