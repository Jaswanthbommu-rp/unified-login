namespace UnifiedLogin.BatchProcessor.Models;

public class BatchProcessorInput
{
    public int ProductBatchId { get; set; }
    public Guid RealPageId { get; set; }
    public int ProductId { get; set; }
    public long CreateUserPersonaId { get; set; }
    public long AssignUserPersonaId { get; set; }
    public string InputJson { get; set; }
    public int BatchProcessType { get; set; }
    public Guid CorrelationId { get; set; }
    public string ProcessApiEndPoint { get; set; }
    public int BatchProcessorGroupId { get; set; }
    public long ImpersonatorUserId { get; set; }
}
