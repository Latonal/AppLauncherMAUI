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
    public AppDownloadButtonStates DownloadButtonState { get { return _downloadButtonState; } set { _downloadButtonState = value; RaisePropertyChanged(() => DownloadButtonState); SetSideButtonState(); } }
    public ICommand DownloadButtonStateCommand { get; set; }
    private bool _sideButtonState = false;
    public bool SideButtonState { get { return _sideButtonState; } set { _sideButtonState = value; RaisePropertyChanged(() => SideButtonState); } }
    private double _progressValue;
    public double ProgressValue { get { return _progressValue; } set { _progressValue = value; RaisePropertyChanged(() => ProgressValue); } }
    private readonly CancellationTokenSource cts = new(TimeSpan.FromMinutes(30));

    public SingleAppViewModel(int appId)
    {
        AppId = appId;

        DownloadButtonStateCommand ??= new Command<AppDownloadButtonCommand?>(async(e) => await ActionDownloadButtonClicked(e));
    }

    private async void SetData(int id)
    {
        AppDataModel data = await GetData(id);

        // Functional
        ExecutionRule[]? executionRules = data.ExecutionRules;
        if (executionRules != null)
            ExecutionRules = executionRules;

        VersionFileUrl = GetWorkingVersionFile(data.VersionUrls ?? []);

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

    private static string GetWorkingVersionFile(string[] versionUrls)
    {
        // TODO?: Might need to check if the header is valid too?
        // But be cautious of being rate limited faster
        // depends of number of apps
        if (versionUrls.Length <= 0) return String.Empty;

        foreach (string versionUrl in versionUrls)
        {
            if (Common.CheckValidUri(versionUrl))
                return versionUrl;
        }

        return String.Empty;
    }

    private async Task<string> GetWorkingDownloadUrl(string[] downloadUrls)
    {
        bool shouldWeCheckAgain = await UpdateTracker.ShouldWeCheckValidity(AppId, null);
        if (!shouldWeCheckAgain)
        {
            string? lastWorkingUrl = await UpdateTracker.GetLastWorkingUrl(AppId);
            if (!String.IsNullOrEmpty(lastWorkingUrl)) return lastWorkingUrl;
        }

        if (downloadUrls.Length <= 0) return String.Empty;

        foreach (string downloadUrl in downloadUrls)
        {
            if (!await DownloadHandler.CheckIfValidHeader(downloadUrl, cts.Token)) continue;

            await UpdateTracker.SetUpdateTrackerModelAsync(AppId, "", downloadUrl);
            return downloadUrl;

            // Todo: save for session the current working downloadUrl
            // with timestamp of the check.
            // At the start, check if workingUrl has already been found
            // (user changed page and went back), if so don't make
            // another call. (this part is done)
            // Also, save currently workings hosts (in case API is limited)?
        }

        return String.Empty;
    }

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
                DownloadButtonState = AppDownloadButtonStates.Installing;
            else
            {
                if (!String.IsNullOrEmpty(VersionFileUrl) && await UpdateTracker.IsVersionDifferent(DownloadHandler.GetDefaultAppPath(AppId.ToString()), AppId, VersionFileUrl, cts.Token))
                    DownloadButtonState = AppDownloadButtonStates.Update;
            }
        }
    }

    private async Task ActionDownloadButtonClicked(AppDownloadButtonCommand? cmd)
    {
        if (AppDownloadButtonCommand.Next == cmd)
        {
            switch (DownloadButtonState)
            {
                case AppDownloadButtonStates.Installing:
                    Download();
                    break;
                case AppDownloadButtonStates.Playable:
                    Launch();
                    break;
                case AppDownloadButtonStates.Update:
                    Download();
                    break;
                case AppDownloadButtonStates.Delete:
                    await Delete();
                    break;
            }
        }
        else if (AppDownloadButtonCommand.Cancel == cmd)
        {
            if (DownloadButtonState == AppDownloadButtonStates.Downloading)
                CancelDownload();
        }
        else if (AppDownloadButtonCommand.Download == cmd)
            Download();
        else if (AppDownloadButtonCommand.OpenFolder == cmd)
            OpenAppFolder();
        else if (AppDownloadButtonCommand.Launch == cmd)
            Launch();
        else if (AppDownloadButtonCommand.Delete == cmd)
            await Delete();
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
        ExternalApplicationManager.AllowedContentType type = await DownloadHandler.GetAppContentType(DownloadUrl, cts.Token);

        if (type == ExternalApplicationManager.AllowedContentType.Zip)
        {
            await DownloadHandler.DownloadZipContent(DownloadUrl, zipPath, appPath, cts.Token, progress);
        }
        else if (type == ExternalApplicationManager.AllowedContentType.Json)
        {
            await DownloadHandler.DownloadRawContent(DownloadUrl, appPath, AppId, cts.Token, progress);
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

    private async Task Delete()
    {
        DownloadHandler.DeleteFolder(DownloadHandler.GetDefaultAppPath(AppId.ToString(), false));
        await SetCurrentAppState();
    }

    public void OpenAppFolder()
    {
        Common.OpenFolder(DownloadHandler.GetDefaultAppPath(AppId.ToString(), false));
    }

    private void SetSideButtonState()
    {
        if (DownloadButtonState == AppDownloadButtonStates.Playable || DownloadButtonState == AppDownloadButtonStates.Update)
            SideButtonState = true;
        else
            SideButtonState = false;
        Debug.WriteLine($"SideButton status : {SideButtonState}");
    }
}