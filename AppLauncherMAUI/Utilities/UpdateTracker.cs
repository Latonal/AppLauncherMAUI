using AppLauncherMAUI.Config;
using AppLauncherMAUI.MVVM.Models.UpdateTracker;
using AppLauncherMAUI.Utilities.DownloadUtilities;
using AppLauncherMAUI.Utilities.Singletons;
using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AppLauncherMAUI.Utilities;

public class AppUpdateInfosModelList
{
    public List<AppUpdateInfosModel> Apps { get; set; } = [];

    public AppUpdateInfosModel Search(string domainName)
    {
        AppUpdateInfosModel? auim = GetData(domainName);
        auim ??= Add(new AppUpdateInfosModel { Name = domainName });
        return auim;
    }

    public AppUpdateInfosModel? GetData(string domainName)
    {
        return Apps.FirstOrDefault(a => a.Name == domainName);
    }

    public AppUpdateInfosModel Add(AppUpdateInfosModel auim)
    {
        AppUpdateInfosModel newAuim = new()
        {
            Name = auim.Name,
            Hash = auim.Hash,
            LastDifferentHash = auim.LastDifferentHash,
            LastWorkingUrl = auim.LastWorkingUrl,
            LastChecked = auim.LastChecked,
        };

        Apps.Add(newAuim);
        return newAuim;
    }
}

public class AppUpdateInfosModel
{
    public required string Name { get; set; }
    public string Hash { get; set; } = "";
    public string LastDifferentHash { get; set; } = "";
    public string LastWorkingUrl { get; set; } = "";
    public long LastChecked { get; set; }

    public void Update(AppUpdateInfosModel auim)
    {
        if (auim.Name != default) Name = auim.Name;
        if (auim.Hash != default) Hash = auim.Hash;
        if (auim.LastDifferentHash != default) LastDifferentHash = auim.LastDifferentHash;
        if (auim.LastWorkingUrl != default) LastWorkingUrl = auim.LastWorkingUrl;
        if (auim.LastChecked != default) LastChecked = auim.LastChecked;
    }

    //public async Task<bool> CanCheckForUpdate(string appPath, string versionFileUrl, CancellationToken cancellationToken)
    //{
    //    if (CanCheckUpdate())
    //        return await IsVersionDifferent(appPath, versionFileUrl, cancellationToken);

    //    return false;
    //}

    public bool CanCheckUpdate(TimeSpan? elapsedTimeNeeded = null)
    {
        TimeSpan elapsedTime = elapsedTimeNeeded ?? TimeSpan.FromSeconds(AppConfig.CacheTime);
        TimeSpan difference = DateTimeOffset.FromUnixTimeSeconds(Common.GetCurrentUnixTimestamp()) - DateTimeOffset.FromUnixTimeSeconds(LastChecked);

        if (difference < elapsedTime)
            return false;

        return true;
    }
}

internal class UpdateTracker
{
    private static readonly string VersionFilePath = AppPaths.VersionTrackingFilePath;
    private static readonly JsonSerializerOptions s_writeIndentedOptions = new()
    {
        WriteIndented = true,
    };

    public AppUpdateInfosModel? AUIM;

    public UpdateTracker() { }

    public async Task Load(string appName)
    {
        if (!File.Exists(VersionFilePath))
        {
            AUIM = new AppUpdateInfosModel { Name = appName };
            return;
        }

        string json = await File.ReadAllTextAsync(VersionFilePath);
        AppUpdateInfosModelList AUIMList = JsonSerializer.Deserialize<AppUpdateInfosModelList>(json) ?? new();

        AUIM = AUIMList.Search(appName) ?? new AppUpdateInfosModel { Name = appName };
    }

    public static async Task Save(AppUpdateInfosModel auim)
    {
        AppUpdateInfosModelList AUIMList;

        if (!File.Exists(VersionFilePath))
        {
            AUIMList = new();
            AUIMList.Add(auim);
        }
        else
        {
            string original = await File.ReadAllTextAsync(VersionFilePath);
            AUIMList = JsonSerializer.Deserialize<AppUpdateInfosModelList>(original) ?? new AppUpdateInfosModelList();
            AUIMList.Search(auim.Name).Update(auim);
        }

        string json = JsonSerializer.Serialize(AUIMList, s_writeIndentedOptions);
        await File.WriteAllTextAsync(VersionFilePath, json);
    }

    public static async Task<bool> IsVersionDifferent(string appId, string versionFileRemoteUrl, string? versionFileLocalPath, string[] updateUrl, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(versionFileRemoteUrl)) return false;
        using HttpResponseMessage? response = await HttpService.GetResponseAsync(versionFileRemoteUrl, cancellationToken);

        if (response == null)
        {
            Console.WriteLine($"[UpdateTracker] Url \"{versionFileRemoteUrl}\" does not seems to work.");
            return false;
        }

        ExternalApplicationManager.AllowedContentType type = ExternalApplicationManager.GetVersionAllowedContentType(response.Content.Headers.ContentType?.MediaType ?? "");
        string content = await response.Content.ReadAsStringAsync(cancellationToken);

        return type switch
        {
            ExternalApplicationManager.AllowedContentType.Text => IsTxtDifferent(DownloadHandler.GetDefaultAppPath(appId), content, versionFileLocalPath),
            ExternalApplicationManager.AllowedContentType.Json => await IsHashDifferent(updateUrl, appId),

            _ => false
        };
    }

    private static bool IsTxtDifferent(string appPath, string remoteTxt, string? localPath)
    {
        string versionFilePath = Path.Combine(appPath, localPath ?? "version.txt");
        if (!File.Exists(versionFilePath)) return false;
        string localTxt = File.ReadAllText(versionFilePath);

        return localTxt != remoteTxt;
    }

    private static async Task<bool> IsHashDifferent(string[] urls, string appName)
    {
        string? commit = null;

        foreach (string url in urls)
        {
            commit = await DownloadHandler.GetCommit(url);
            if (!String.IsNullOrEmpty(commit)) break;
        }

        if (String.IsNullOrEmpty(commit))
        {
            Console.WriteLine($"[UpdateTracker] Could not fetch commit from urls.");
            return false;
        }

        if (!File.Exists(VersionFilePath)) return true;

        string json = await File.ReadAllTextAsync(VersionFilePath);
        AppUpdateInfosModelList AUIMList = JsonSerializer.Deserialize<AppUpdateInfosModelList>(json) ?? new();

        string localHash = AUIMList.Search(appName).Hash;
        return localHash != commit;
    }
}