namespace UnifiedLogin.BatchProcessor.Models;

/// <summary>
/// Represents a batch record for product user operations.
/// </summary>
public class Batch
{
    public long BatchProcessorId { get; set; }
    public Guid CorrelationId { get; set; }
    public long EditorUserPersonaId { get; set; }
    public long SubjectUserPersonaId { get; set; }
    public long EditorUserRealPageId { get; set; }
    public int ProductId { get; set; }
    public int RetryCount { get; set; }
    public int StatusTypeId { get; set; }
    public int BatchProcessTypeId { get; set; }
    public string? InputJson { get; set; }
    public long? BatchProcessorGroupId { get; set; }
    public long? ImpersonatorUserId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime? ModifiedDateTime { get; set; }
}

/// <summary>
/// Represents a batch record for enterprise role updates.
/// </summary>
public class EnterpriseRoleBatch
{
    public long EnterpriseRoleBatchProcessId { get; set; }
    public long EditorUserPersonaId { get; set; }
    public long SubjectUserPersonaId { get; set; }
    public int EnterpriseRoleTemplateId { get; set; }
    public int StatusTypeId { get; set; }
    public int BatchProcessTypeId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime? ModifiedDateTime { get; set; }
}

/// <summary>
/// Represents a batch record for primary property updates.
/// </summary>
public class PrimaryPropertyBatch
{
    public long PrimaryPropertyBatchProcessId { get; set; }
    public long EditorUserPersonaId { get; set; }
    public long SubjectUserPersonaId { get; set; }
    public int StatusTypeId { get; set; }
    public int BatchProcessTypeId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime? ModifiedDateTime { get; set; }
}

/// <summary>
/// Represents a batch record for bulk user updates.
/// </summary>
public class BulkUserBatch
{
    public long BulkUserBatchProcessId { get; set; }
    public long EditorUserPersonaId { get; set; }
    public long SubjectUserPersonaId { get; set; }
    public int StatusTypeId { get; set; }
    public int BatchProcessTypeId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime? ModifiedDateTime { get; set; }
}

/// <summary>
/// Represents a batch record for company property updates.
/// </summary>
public class CompanyPropertyBatch
{
    public long CompanyBatchJobId { get; set; }
    public long CompanyInstanceSourceId { get; set; }
    public bool IsActive { get; set; }
    public long CreateUserPersonaId { get; set; }
    public int BatchProcessorTypeId { get; set; }
    public int StatusTypeId { get; set; }
    public DateTime CreatedDateTime { get; set; }
    public DateTime? ModifiedDateTime { get; set; }
}

/// <summary>
/// Batch status types.
/// </summary>
public enum BatchStatusType
{
    Waiting = 1,
    Processing = 2,
    Completed = 3,
    Error = 4,
    Retry = 5
}
