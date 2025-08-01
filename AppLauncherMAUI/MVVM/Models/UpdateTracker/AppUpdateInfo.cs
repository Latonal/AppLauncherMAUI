namespace AppLauncherMAUI.MVVM.Models.UpdateTracker;

public class AppUpdateInfo
{
    public string Hash { get; set; } = "";
    public string LastDifferentHash { get; set; } = "";
    public string LastWorkingUrl { get; set; } = "";
    public long LastChecked { get; set; } = 0;
}