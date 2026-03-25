using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for all password policy operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManagePasswordPolicy"/>
/// + blocking <c>Task.Run()</c> calls in <c>PasswordPolicyController</c>.
/// </summary>
public interface IManagePasswordPolicyAsync
{
    /// <summary>
    /// Creates a new password policy for an organisation.
    /// Replaces: <c>ManagePasswordPolicy.CreatePasswordPolicy</c>.
    /// </summary>
    Task<RepositoryResponse> CreatePasswordPolicyAsync(
        IPasswordPolicy passwordPolicy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the password policy for the given organisation party.
    /// Returns <see langword="null"/> when no policy exists for that party.
    /// Replaces: <c>ManagePasswordPolicy.GetPasswordPolicy</c>.
    /// </summary>
    Task<PasswordPolicy?> GetPasswordPolicyAsync(
        long partyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing password policy.
    /// Replaces: <c>ManagePasswordPolicy.UpdatePasswordPolicy</c>.
    /// </summary>
    Task<RepositoryResponse> UpdatePasswordPolicyAsync(
        IPasswordPolicy passwordPolicy,
        CancellationToken cancellationToken = default);
}
