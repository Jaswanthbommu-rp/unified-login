namespace UnifiedLogin.BatchProcessor.Models;

public class PrimaryPropertyBatch
{
    public int PrimaryPropertyBatchProcessId { get; set; }
    public long EditorUserPersonaId { get; set; }
    public long SubjectUserPersonaId { get; set; }
    public int StatusTypeId { get; set; }
    public int BatchProcessTypeId { get; set; }
}
