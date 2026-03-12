using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Services.User;

/// <summary>
/// Service for creating new users with async operations
/// </summary>
public interface IUserCreationService
{
    /// <summary>
    /// Creates a new user with personas and products (Async)
    /// </summary>
    Task<CreateUserResponse<IErrorData>> CreateUserAsync(
        ProfileDetail newProfile,
        IList<Persona> persona,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates user creation prerequisites (Async)
    /// </summary>
    Task<ValidationResult> ValidateUserCreationAsync(
        ProfileDetail profile,
        Guid organizationRealPageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get starter profile options (Async)
    /// </summary>
    Task<StarterProfileOptionsResponse> GetStarterProfileOptionsAsync(
        string enterpriseUserName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Validation result with detailed error messages
/// </summary>
public record ValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public string ErrorMessage => string.Join("; ", Errors);
}