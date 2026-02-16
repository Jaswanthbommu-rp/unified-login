namespace UnifiedLogin.BatchProcessor.Models;

public class BatchConfiguration
{
    public int BatchProcessTypeId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string ConfigurationTypeName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
