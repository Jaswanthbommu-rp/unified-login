using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services.Audit;

/// <summary>
/// Centralized user audit logging with async support
/// </summary>
public interface IUserAuditService
{
    /// <summary>
    /// Logs general user activity (Async)
    /// </summary>
    Task LogActivityAsync(
        string logActivityType,
        LogActivityCategoryType category,
        string message,
        IProfileDetail profile,
        CancellationToken cancellationToken = default,
        List<AdditionalParameters> additionalInfo = null);

    /// <summary>
    /// Logs user profile update audit (Async)
    /// </summary>
    Task LogUserUpdateAsync(
        IProfileDetail oldProfile,
        IProfileDetail newProfile,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs bulk IDP update activity (Async)
    /// </summary>
    Task LogBulkIdpUpdateAsync(
        IList<long> userIds,
        bool isEnabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs individual field change (Async)
    /// </summary>
    Task LogFieldChangeAsync(
        string oldValue,
        string newValue,
        string fieldName,
        string message,
        IProfileDetail profile,
        CancellationToken cancellationToken = default);
}