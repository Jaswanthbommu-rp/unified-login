namespace UnifiedLogin.BatchProcessor.Models;

/// <summary>
/// Input model for batch processing API calls.
/// </summary>
public class BatchProcessorInput
{
    public long RealPageId { get; set; }
    public long ProductBatchId { get; set; }
    public long CreateUserPersonaId { get; set; }
    public long AssignUserPersonaId { get; set; }
    public int ProductId { get; set; }
    public string? InputJson { get; set; }
    public Guid CorrelationId { get; set; }
    public int BatchProcessType { get; set; }
    public string? ProcessApiEndPoint { get; set; }
    public long? BatchProcessorGroupId { get; set; }
    public long? ImpersonatorUserId { get; set; }
}

/// <summary>
/// Response model from batch processing API calls.
/// </summary>
public class BatchProcessorResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }
}
