using AppLauncherMAUI.Utilities;

namespace AppLauncherMAUI.MVVM.Models;

public class DomainDownloadModel
{
    public required string Name { get; set; }
    public int NumberOfTokenUsed { get; set; }
    public int MaxNumberOfTokenUsable { get; set; }
    public long UnixTimestampReset { get; set; }

    public void CheckReset()
    {
        if (Common.GetCurrentUnixTimestamp() > UnixTimestampReset)
            NumberOfTokenUsed = 0;
    }

    public bool IsDownloadable()
    {
        CheckReset();
        if (NumberOfTokenUsed >= MaxNumberOfTokenUsable)
            return false;

        return true;
    }

    public void UpdateTokenUsed(int toAdd = 1)
    {
        NumberOfTokenUsed += toAdd;
    }
}

public class DomainDownloadModelList
{
    public List<DomainDownloadModel> ddmlist = [];

    public bool IsDownloadable(string domainName)
    {
        DomainDownloadModel? ddm = GetDomainData(domainName);
        if (ddm == null) return true; // ???? maybe create at this moment ?
        return ddm.IsDownloadable();
    }

    public DomainDownloadModel? GetDomainData(string domainName)
    {
        return ddmlist.FirstOrDefault(d => d.Name == domainName);
    }

    public bool DoesDomainExist(string domainName)
    {
        return ddmlist.Any(d => d.Name == domainName);
    }

    public void Add(DomainDownloadModel ddm)
    {
        ddmlist.Add(new DomainDownloadModel {
            Name = ddm.Name,
            NumberOfTokenUsed = ddm.NumberOfTokenUsed,
            MaxNumberOfTokenUsable = ddm.MaxNumberOfTokenUsable,
            UnixTimestampReset = ddm.UnixTimestampReset,
        });
    }

    public void Remove(string domainName)
    {
        ddmlist.RemoveAll(d => d.Name == domainName);
    }

    //public void Clean()
    //{
        // TODO: clean unused data from array
    //}
}