namespace UnifiedLogin.BatchProcessor.Models;

/// <summary>
/// Model for processing user login notifications and status updates.
/// Migrated from UserNotification service.
/// </summary>
public class ProcessUserLogin
{
    /// <summary>
    /// User's RealPage ID
    /// </summary>
    public Guid UserRealPageId { get; set; }

    /// <summary>
    /// Organization's RealPage ID
    /// </summary>
    public Guid OrganizationRealPageId { get; set; }

    /// <summary>
    /// Date when the account can be used (UTC)
    /// </summary>
    public DateTime? FromDate { get; set; }
}
