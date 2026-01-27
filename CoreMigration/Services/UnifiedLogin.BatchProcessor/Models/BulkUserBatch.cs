namespace UnifiedLogin.BatchProcessor.Models;

public class BulkUserBatch
{
    public int BulkUserBatchProcessId { get; set; }
    public long EditorUserPersonaId { get; set; }
    public long SubjectUserPersonaId { get; set; }
    public int StatusTypeId { get; set; }
    public int BatchProcessTypeId { get; set; }
}
