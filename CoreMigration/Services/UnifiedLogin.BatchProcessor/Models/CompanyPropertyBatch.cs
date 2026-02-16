namespace UnifiedLogin.BatchProcessor.Models;

public class CompanyPropertyBatch
{
    public long CompanyBatchJobId { get; set; }
    public string CompanyInstanceSourceId { get; set; }
    public int StatusTypeId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime LastRunDateTime { get; set; }
    public long CreatedBy { get; set; }
    public int BatchProcessorTypeId { get; set; }
    public long CreateUserPersonaId { get; set; }
    public int IsActive { get; set; }
}
