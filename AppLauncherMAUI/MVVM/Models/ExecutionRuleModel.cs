namespace AppLauncherMAUI.MVVM.Models;

public class ExecutionRuleModel
{
    public required string Type { get; set; } // Metadata, Name, Extension, ExactMatch
    public string? Value { get; set; }
    public List<MetadataCondition>? Conditions { get; set; }
}
