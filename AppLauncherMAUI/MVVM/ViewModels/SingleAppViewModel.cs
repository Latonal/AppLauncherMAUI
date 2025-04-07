using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.MVVM.Views.Controls;
using AppLauncherMAUI.Utilities;
using System.Diagnostics;
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

        DownloadButtonState = AppDownloadButtonStates.Install;
        DownloadButtonStateCommand ??= new Command<AppDownloadButtonCommand?>(ActionDownloadButtonClicked);
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

    private void ActionDownloadButtonClicked(AppDownloadButtonCommand? cmd)
    {
        if (AppDownloadButtonCommand.Next == cmd)
        {
            switch (DownloadButtonState)
            {
                case AppDownloadButtonStates.Install:
                    Install();
                    break;
                case AppDownloadButtonStates.Playable:
                    Launch();
                    break;
                case AppDownloadButtonStates.Update:
                    Install();
                    break;
                case AppDownloadButtonStates.Delete:
                    Delete();
                    break;
            }

            ChangeAppDownloadButtonState();
        }
        else if (AppDownloadButtonCommand.Cancel == cmd)
        {
            if (DownloadButtonState == AppDownloadButtonStates.Downloading)
                CancelInstall();
        }
        else if (AppDownloadButtonCommand.Launch == cmd)
            Launch();
        else if (AppDownloadButtonCommand.Delete == cmd)
            Delete();
    }

    private void ChangeAppDownloadButtonState()
    {
        switch (DownloadButtonState)
        {
            case AppDownloadButtonStates.Install:
                DownloadButtonState = AppDownloadButtonStates.Downloading;
                break;
            case AppDownloadButtonStates.Downloading:
                DownloadButtonState = AppDownloadButtonStates.Playable;
                break;
            case AppDownloadButtonStates.Playable:
                DownloadButtonState = AppDownloadButtonStates.Update;
                break;
            case AppDownloadButtonStates.Update:
                DownloadButtonState = AppDownloadButtonStates.Install;
                break;
        }
    }

    private static void Install()
    {
        Debug.WriteLine("Must install");
    }

    private static void CancelInstall()
    {
        Debug.WriteLine("Must cancel install");
    }

    private static void Launch()
    {
        Debug.WriteLine("Must launch");
    }

    private static void Delete()
    {
        Debug.WriteLine("Must delete");
    }
}