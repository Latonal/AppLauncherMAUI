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

    private string _name = "DefaultAppName";
    public string Name { get { return _name; } set { _name = value; RaisePropertyChanged(() => Name); } }
    private string? _downloadUrl;
    public string? DownloadUrl { get { return _downloadUrl; } set { _downloadUrl = value; _ = SetCurrentAppState(); } }

    private string? _fullBanner;
    public string? FullBanner { get { return _fullBanner; } set { _fullBanner = value; RaisePropertyChanged(() => FullBanner); } }
    private string? _appCardFullDescription;
    public string? AppCardFullDescription { get { return _appCardFullDescription; } set { _appCardFullDescription = value; RaisePropertyChanged(() => AppCardFullDescription); } }

    private AppDownloadButtonStates _downloadButtonState = AppDownloadButtonStates.Loading;
    public AppDownloadButtonStates DownloadButtonState { get { return _downloadButtonState; } set { _downloadButtonState = value; RaisePropertyChanged(() => DownloadButtonState); } }
    public ICommand DownloadButtonStateCommand { get; set; }
    private double _progressValue;
    public double ProgressValue { get { return _progressValue; } set { _progressValue = value; RaisePropertyChanged(() => ProgressValue); } }
    //private static readonly DownloadHandler downloadHandler = new();


    public SingleAppViewModel(int appId)
    {
        AppId = appId;

        DownloadButtonStateCommand ??= new Command<AppDownloadButtonCommand?>(ActionDownloadButtonClicked);
    }

    private async void SetData(int id)
    {
        AppDataModel data = await GetData(id);

        Name = data.Name ?? "DefaultAppName";
        FullBanner = data.Banners?.Full;
        DownloadUrl = data.DownloadUrl;

        LanguagesModel? texts = data.Text;
        if (texts != null)
            Text = Common.GetTranslatedJsonText(texts);

        LanguagesModel? desc = data.Banners?.FullDescription;
        if (desc != null)
            AppCardFullDescription = Common.GetTranslatedJsonText(desc);

        Debug.WriteLine("Finished assigning data");
    }

    private static async Task<AppDataModel> GetData(int id)
    {
        return await JsonFileManager.ReadSingleDataAsync<AppDataModel>(AppPaths.AppsDataJsonName, "Id", id);
    }

    /*SetCurrentAppState logic
     * Loading
     * 
     * If Playable
     *      Set Playable
     *      playable = true
     * 
     * If DownloadUrl == null
     *      If !playable
     *          Set Disabled
     *      return
     * 
     * If CheckHeader is valid
     *      If !playable
     *          Set Install
     *      Else CompareVersion
     *          If same
     *              Set Playable (might already be Playable)
     *          Else
     *              Set Update
     */
    private async Task SetCurrentAppState()
    {
        DownloadButtonState = AppDownloadButtonStates.Loading;

        bool playable = false;
        if (CheckIfPlayable())
        {
            DownloadButtonState = AppDownloadButtonStates.Playable;
            playable = true;
        }

        if (DownloadUrl == null)
        {
            if (!playable)
                DownloadButtonState = AppDownloadButtonStates.Disabled;
            return;
        }
        if (DownloadUrl != null && await DownloadHandler.CheckIfValidHeader(DownloadUrl))
        {
            if (!playable)
                DownloadButtonState = AppDownloadButtonStates.Install;
            else
            {
                // CompareVersion
                // If same 
                // Set Playable
                // Else
                // Set Update
            }
        }
        else
            DownloadButtonState = AppDownloadButtonStates.Disabled;
    }

    private void ActionDownloadButtonClicked(AppDownloadButtonCommand? cmd)
    {
        if (AppDownloadButtonCommand.Next == cmd)
        {
            switch (DownloadButtonState)
            {
                case AppDownloadButtonStates.Install:
                    Download();
                    break;
                case AppDownloadButtonStates.Playable:
                    Launch();
                    break;
                case AppDownloadButtonStates.Update:
                    Download();
                    break;
                case AppDownloadButtonStates.Delete:
                    Delete();
                    break;
            }

            //ChangeAppDownloadButtonState();
        }
        else if (AppDownloadButtonCommand.Cancel == cmd)
        {
            if (DownloadButtonState == AppDownloadButtonStates.Downloading)
                CancelDownload();
        }
        else if (AppDownloadButtonCommand.Launch == cmd)
            Launch();
        else if (AppDownloadButtonCommand.Delete == cmd)
            Delete();
    }


    private CancellationTokenSource cts = new();

    private bool CheckIfPlayable()
    {
        string[]? foundFiles = ApplicationHandler.ReturnFilesByPatterns(Name);

        return !(foundFiles == null || foundFiles.Length == 0);
    }

    private async void Download()
    {
        DownloadButtonState = AppDownloadButtonStates.Downloading;
        string zipPath = DownloadHandler.GetDefaultZipPath(Name);
        string appPath = DownloadHandler.GetDefaultAppPath(Name);

        if (DownloadUrl == null || Name == null) return;
        IProgress<double> progress = new Progress<double>(value => ProgressValue = value);
        try
        {
            ExternalApplicationManager.AllowedContentType type = await DownloadHandler.GetContentType(DownloadUrl);

            if (type == ExternalApplicationManager.AllowedContentType.Zip)
            {
                cts = new();
                await DownloadHandler.DownloadFileAsync(DownloadUrl, zipPath, cts.Token, progress);
                // Todo: should clean destination folder?(appPath)
                DownloadHandler.ExtractZip(zipPath, appPath);
                DownloadHandler.DeleteFolder(zipPath);
                DownloadHandler.CleanDestinationPath(appPath);
            } else
            {
                Console.Error.WriteLine("(SingleAppViewModel) Type '" + type + "' is not supported for downloading.");
            }
        }
        catch (Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                Console.WriteLine("Stopped Download");
            }
            else
            {
                throw new Exception($"(SingleAppViewModel) Something happened while downloading: {ex.Message}");
            }
            progress.Report(0);
        }
        await SetCurrentAppState();
    }

    private void CancelDownload()
    {
        cts.Cancel();
    }

    public ExternalApplicationManager eam = new();

    private void Launch()
    {
        string[]? files = ApplicationHandler.ReturnFilesByPatterns(Name);
        // TODO: maybe be a little more selective?
        if (files?.Length > 0) eam.StartApplication(files[0]);
    }

    private static void Delete()
    {
        Debug.WriteLine("Must delete");
    }
}