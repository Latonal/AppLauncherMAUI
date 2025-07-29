using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models.UpdateTracker;
using AppLauncherMAUI.Utilities.DownloadUtilities;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;

namespace AppLauncherMAUI.Utilities;

internal static class UpdateTracker
{
    private static readonly string VersionFilePath = AppPaths.VersionTrackingFilePath;
    private static readonly JsonSerializerOptions s_writeIndentedOptions = new()
    {
        WriteIndented = true,
    };

    private static async Task<UpdateTrackerModel> LoadAsync()
    {
        if (!File.Exists(VersionFilePath))
            return new UpdateTrackerModel();

        string json = await File.ReadAllTextAsync(VersionFilePath);
        return JsonSerializer.Deserialize<UpdateTrackerModel>(json) ?? new UpdateTrackerModel();
    }

    private static async Task SaveAsync(UpdateTrackerModel model)
    {
        string json = JsonSerializer.Serialize(model, s_writeIndentedOptions);
        await File.WriteAllTextAsync(VersionFilePath, json);
    }

    public static async Task<AppUpdateInfo?> GetModel(int appId)
    {
        UpdateTrackerModel model = await LoadAsync();
        if (model.Apps == null) return null;
        return model.Apps.TryGetValue(appId, out AppUpdateInfo? info) ? info : null;
    }

    public static async Task<long> GetLastChecked(int appId)
    {
        UpdateTrackerModel model = await LoadAsync();
        if (model.Apps == null) return 0;
        return model.Apps.TryGetValue(appId, out AppUpdateInfo? info) ? info.LastChecked : 0;
    }

    public static async Task<string?> GetHashAsync(int appId)
    {
        UpdateTrackerModel model = await LoadAsync();
        if (model.Apps == null) return null;
        return model.Apps.TryGetValue(appId, out AppUpdateInfo? info) ? info.Hash : null;
    }

    public static async Task<string?> GetLastWorkingUrl(int appId)
    {
        UpdateTrackerModel model = await LoadAsync();
        if (model.Apps == null) return null;
        return model.Apps.TryGetValue(appId, out AppUpdateInfo? info) ? info.LastWorkingUrl : null;
    }

    public static async Task SetUpdateTrackerModelAsync(int appId, string? hash, string? workingUrl)
    {
        if (String.IsNullOrEmpty(hash)) hash = null;
        if (String.IsNullOrEmpty(workingUrl)) workingUrl = null;

        UpdateTrackerModel model = await LoadAsync();
        model.Apps ??= [];
        AppUpdateInfo oldInfos = model.Apps.TryGetValue(appId, out AppUpdateInfo? value) ? value : new AppUpdateInfo();
        model.Apps[appId] = new AppUpdateInfo
        {
            Hash = hash ?? oldInfos.Hash ?? "",
            LastWorkingUrl = workingUrl ?? oldInfos.LastWorkingUrl ?? "",
            LastChecked = Common.GetCurrentUnixTimestamp()
        };
        await SaveAsync(model);
    }

    public static async Task<bool> IsHashDifferent(int appId, string receivedHash)
    {
        string savedHash = await GetHashAsync(appId) ?? "";
        return savedHash == receivedHash;
    }

    /// <summary>
    /// Return the last saved downloadUrl if the last verification was too recent
    /// Else, return an empty string
    /// </summary>
    /// <param name="elapsedTimeNeeded"></param>
    /// <returns></returns>
    public static async Task<string> ShouldWeCheckValidity(int appId, TimeSpan? elapsedTimeNeeded)
    {
        AppUpdateInfo? infos = await GetModel(appId);
        if (infos == null) return String.Empty;

        TimeSpan elapsedTime = elapsedTimeNeeded ?? TimeSpan.FromSeconds(AppConfig.CacheTime);

        DateTimeOffset unixCurrent = DateTimeOffset.FromUnixTimeSeconds(Common.GetCurrentUnixTimestamp());
        DateTimeOffset unixSaved = DateTimeOffset.FromUnixTimeSeconds(infos.LastChecked);
        TimeSpan difference = unixCurrent - unixSaved;

        if (difference < elapsedTime)
            return infos.LastWorkingUrl;

        return String.Empty;
    }

    public static async Task<bool> IsVersionDifferent(string appPath, int appId, string versionFileUrl, CancellationToken cancellationToken)
    {

        if (String.IsNullOrEmpty(versionFileUrl)) return false;
        using HttpResponseMessage? response = await HttpService.GetResponseAsync(versionFileUrl, cancellationToken);

        if (response == null)
        {
            Console.WriteLine($"[DownloadHandler] Url \"{versionFileUrl}\" seems to not work.");
            return false;
        }

        ExternalApplicationManager.AllowedContentType type = ExternalApplicationManager.GetVersionAllowedContentType(response.Content.Headers.ContentType?.MediaType ?? "");
        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        return type switch
        {
            ExternalApplicationManager.AllowedContentType.Text => IsTxtDifferent(appPath, content),
            ExternalApplicationManager.AllowedContentType.Json => await IsHashDifferent(appId, versionFileUrl, content),

            _ => false
        };

    }

    private static bool IsTxtDifferent(string appPath, string remoteTxt)
    {
        string versionFilePath = Path.Combine(appPath, "version.txt");
        if (!File.Exists(versionFilePath)) return false;
        string localTxt = File.ReadAllText(versionFilePath);

        return localTxt != remoteTxt;
    }

    private static async Task<bool> IsHashDifferent(int appId, string versionFileUrl, string json)
    {
        string localHash = await GetHashAsync(appId) ?? "";

        string host = Common.GetUriHost(versionFileUrl);
        string remoteHash = host switch
        {
            "api.github.com" => GithubDownloadHandler.GetSha(json),
            _ => throw new Exception($"[DownloadHandler] url ({versionFileUrl}) hostname is not supported."),
        };

        return localHash != remoteHash;
    }
}
