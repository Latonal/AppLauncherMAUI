namespace AppLauncherMAUI.MVVM.Models;

public class ExecutionRule
{
    public required string Type { get; set; } // Metadata, Name, Extension, ExactMatch
    public string? Value { get; set; }
    public List<MetadataCondition>? Conditions { get; set; }
}
