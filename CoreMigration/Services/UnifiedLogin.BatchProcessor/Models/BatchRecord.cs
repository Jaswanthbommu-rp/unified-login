namespace UnifiedLogin.BatchProcessor.Models;

/// <summary>
/// Generic batch record for scheduled job processing.
/// </summary>
public class BatchRecord
{
    public int Id { get; set; }
    public string? Data { get; set; }
}
