using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models;
using AppLauncherMAUI.MVVM.Views.Controls;
using AppLauncherMAUI.Utilities;
using AppLauncherMAUI.Utilities.DownloadUtilities;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace AppLauncherMAUI.MVVM.ViewModels;

internal partial class SingleAppViewModel : ExtendedBindableObject
{
    #region appdata
    private int _appId = 0;
    public int AppId { get { return _appId; } set { _appId = value; SetData(value); } }

    private string? _text;
    public string? Text { get { return _text; } set { _text = value; RaisePropertyChanged(() => Text); } }

    private string _name = "DefaultAppName";
    public string Name { get { return _name; } set { _name = value; RaisePropertyChanged(() => Name); } }
    private string? _downloadUrl;
    public string? DownloadUrl { get { return _downloadUrl; } set { _downloadUrl = value; _ = SetCurrentAppState(); } }
    private string? _versionFileUrl;
    public string? VersionFileUrl { get { return _versionFileUrl; } set { _versionFileUrl = value; } }

    private string? _fullBanner;
    public string? FullBanner { get { return _fullBanner; } set { _fullBanner = value; RaisePropertyChanged(() => FullBanner); } }
    private string? _appCardFullDescription;
    public string? AppCardFullDescription { get { return _appCardFullDescription; } set { _appCardFullDescription = value; RaisePropertyChanged(() => AppCardFullDescription); } }
    private ExecutionRule[]? _executionRules;
    public ExecutionRule[]? ExecutionRules { get => _executionRules; set => _executionRules = value; }
    #endregion appdata
    private AppDownloadButtonStates _downloadButtonState = AppDownloadButtonStates.Loading;
    public AppDownloadButtonStates DownloadButtonState { get { return _downloadButtonState; } set { _downloadButtonState = value; RaisePropertyChanged(() => DownloadButtonState); } }
    public ICommand DownloadButtonStateCommand { get; set; }
    private double _progressValue;
    public double ProgressValue { get { return _progressValue; } set { _progressValue = value; RaisePropertyChanged(() => ProgressValue); } }
    private CancellationTokenSource cts = new(TimeSpan.FromMinutes(30));

    public SingleAppViewModel(int appId)
    {
        AppId = appId;

        DownloadButtonStateCommand ??= new Command<AppDownloadButtonCommand?>(ActionDownloadButtonClicked);
    }

    private async void SetData(int id)
    {
        AppDataModel data = await GetData(id);

        // Functional
        ExecutionRule[]? executionRules = data.ExecutionRules;
        if (executionRules != null)
            ExecutionRules = executionRules;

        //VersionFileUrl = data.VersionFileUrl;

        // This line trigger the whole check (SetCurrentAppState())
        // Any value related to functional must be put above
        // Any value related to visual muse be put below
        DownloadUrl = await GetWorkingDownloadUrl(data.DownloadUrls ?? []);

        // Visual
        Name = data.Name ?? "DefaultAppName";
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

    private static async Task<string> GetWorkingDownloadUrl(string[] downloadUrls)
    {
        if (downloadUrls.Length <= 0) return String.Empty;

        foreach (string downloadUrl in downloadUrls) {
            if (! await DownloadHandler.CheckIfValidHeader(downloadUrl)) continue;

            return downloadUrl;

            // Todo: save for session the current working downloadUrl
            // with timestamp of the check.
            // At the start, check if workingUrl has already been found
            // (user changed page and went back), if so don't make
            // another call.
            // Also, save currently workings hosts (in case API is limited)?
        }

        return String.Empty;
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

        if (DownloadUrl == String.Empty)
        {
            if (!playable)
                DownloadButtonState = AppDownloadButtonStates.Disabled;
            return;
        }
        else
        {
            if (!playable)
                DownloadButtonState = AppDownloadButtonStates.Install;
            else
            {
                if (VersionFileUrl != null && await DownloadHandler.IsVersionDifferent(DownloadHandler.GetDefaultAppPath(AppId.ToString()), VersionFileUrl))
                    DownloadButtonState = AppDownloadButtonStates.Update;
            }
        }
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

    private bool CheckIfPlayable()
    {
        string[]? foundFiles = ApplicationHandler.ReturnFilesByPatterns(AppId.ToString(), ExecutionRules);

        return !(foundFiles == null || foundFiles.Length == 0);
    }

    private async void Download()
    {
        if (DownloadUrl == String.Empty || DownloadUrl == null) return;

        DownloadButtonState = AppDownloadButtonStates.Downloading;
        string zipPath = DownloadHandler.GetDefaultZipPath(AppId.ToString());
        string appPath = DownloadHandler.GetDefaultAppPath(AppId.ToString());

        IProgress<double> progress = new Progress<double>(value => ProgressValue = value);
        ExternalApplicationManager.AllowedContentType type = await DownloadHandler.GetContentType(DownloadUrl);

        if (type == ExternalApplicationManager.AllowedContentType.Zip)
        {
            await DownloadHandler.DownloadZipContent(DownloadUrl, zipPath, appPath, cts.Token, progress);
        }
        else if (type == ExternalApplicationManager.AllowedContentType.Json)
        {
            await DownloadHandler.DownloadRawContent(DownloadUrl, appPath, cts.Token, progress);
        }
        else
        {
            Console.Error.WriteLine("(SingleAppViewModel) Type '" + type + "' is not supported for downloading.");
        }

        progress.Report(0);
        await SetCurrentAppState();
    }

    private void CancelDownload()
    {
        cts.Cancel();
    }

    public ExternalApplicationManager eam = new();

    private void Launch()
    {
        string[]? files = ApplicationHandler.ReturnFilesByPatterns(AppId.ToString(), ExecutionRules);
        // TODO: maybe be a little more selective?
        if (files?.Length > 0) eam.StartApplication(files[0]);
    }

    private static void Delete()
    {
        Debug.WriteLine("Must delete");
    }
}