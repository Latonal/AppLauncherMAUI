namespace AppLauncherMAUI.MVVM.Models;

public class VersionCheckerModel
{
    public string? VersionUrl { get; set; }
    public required string Type { get; set; } // Hash, Raw
}
