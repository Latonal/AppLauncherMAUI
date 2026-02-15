using AppLauncherMAUI.Utilities;
using System.Net.Http.Headers;

namespace AppLauncherMAUI.MVVM.Models;

public class DomainDownloadModel
{
    public required string Name { get; set; }
    public int NumberOfTokenUsed { get; set; }
    public int MaxNumberOfTokenUsable { get; set; }
    public long UnixTimestampReset { get; set; }

    public void Update(DomainDownloadModel ddm)
    {
        if (ddm.Name != default) Name = ddm.Name;
        if (ddm.NumberOfTokenUsed != default) NumberOfTokenUsed = ddm.NumberOfTokenUsed;
        if (ddm.MaxNumberOfTokenUsable != default) MaxNumberOfTokenUsable = ddm.MaxNumberOfTokenUsable;
        if (ddm.UnixTimestampReset != default) UnixTimestampReset = ddm.UnixTimestampReset;
    }

    public void Update(HttpContentHeaders headers)
    {
        // Based on Github, might not apply to every host...
        // If so, create a switch depending of host
        if (headers.TryGetValues("X-RateLimit-Used", out IEnumerable<string>? valueTokenUsed)
            && int.TryParse(valueTokenUsed.FirstOrDefault(), out int tokenUsed))
            NumberOfTokenUsed = tokenUsed;

        if (headers.TryGetValues("X-RateLimit-Used", out IEnumerable<string>? valueTokenUsable)
            && int.TryParse(valueTokenUsable.FirstOrDefault(), out int tokenUsable))
            MaxNumberOfTokenUsable = tokenUsable;

        if (headers.TryGetValues("X-RateLimit-Reset", out IEnumerable<string>? valueResetTime)
            && int.TryParse(valueResetTime.FirstOrDefault(), out int resetTime))
            UnixTimestampReset = resetTime;
    }

    public void CheckReset()
    {
        if (Common.GetCurrentUnixTimestamp() > UnixTimestampReset)
            NumberOfTokenUsed = 0;
    }

    public bool IsDownloadable()
    {
        CheckReset();
        if (NumberOfTokenUsed >= MaxNumberOfTokenUsable && MaxNumberOfTokenUsable != default)
            return false;

        return true;
    }
}

public class DomainDownloadModelList
{
    public List<DomainDownloadModel> ddmlist = [];

    public DomainDownloadModel Search(string domainName)
    {
        DomainDownloadModel? ddm = GetData(domainName);
        ddm ??= Add(new DomainDownloadModel { Name = domainName });
        return ddm;
    }

    public DomainDownloadModel? GetData(string domainName)
    {
        return ddmlist.FirstOrDefault(d => d.Name == domainName);
    }

    public bool Exist(string domainName)
    {
        return ddmlist.Any(d => d.Name == domainName);
    }

    public DomainDownloadModel Add(DomainDownloadModel ddm)
    {
        ddmlist.Add(new DomainDownloadModel {
            Name = ddm.Name,
            NumberOfTokenUsed = ddm.NumberOfTokenUsed,
            MaxNumberOfTokenUsable = ddm.MaxNumberOfTokenUsable,
            UnixTimestampReset = ddm.UnixTimestampReset,
        });
        return ddm;
    }

    public void Remove(string domainName)
    {
        ddmlist.RemoveAll(d => d.Name == domainName);
    }

    //public void Clean()
    //{
        // TODO: clean unused data from array
    //}

    public void Clear()
    {
        ddmlist.Clear();
    }
}