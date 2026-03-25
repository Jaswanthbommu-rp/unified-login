using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Async-first replacement for <see cref="UnifiedLogin.BusinessLogic.Logic.ManagePasswordPolicy"/>.
/// <para>Key improvements over the sync version:</para>
/// <list type="bullet">
///   <item>Single DI constructor — eliminates the <c>new PasswordPolicyRepository()</c> anti-pattern from the parameterless ctor.</item>
///   <item>All data access uses genuine <c>await</c> via <see cref="IPasswordPolicyRepositoryAsync"/> — no <c>Task.Run()</c> wrapping sync methods.</item>
///   <item>Input guard clauses removed from this layer — validation belongs in the controller; throwing from Logic violates the thin-controller contract.</item>
/// </list>
/// </summary>
public sealed class ManagePasswordPolicyAsync : IManagePasswordPolicyAsync
{
    private readonly IPasswordPolicyRepositoryAsync _repo;

    public ManagePasswordPolicyAsync(IPasswordPolicyRepositoryAsync repo)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> CreatePasswordPolicyAsync(
        IPasswordPolicy passwordPolicy,
        CancellationToken cancellationToken = default)
        => _repo.CreatePasswordPolicyAsync(passwordPolicy, cancellationToken);

    /// <inheritdoc/>
    public async Task<PasswordPolicy?> GetPasswordPolicyAsync(
        long partyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repo.GetPasswordPolicyAsync(partyId, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            // Repository throws InvalidOperationException via .First() when no settings rows
            // exist for this partyId — treat as no policy found (preserves the null-check
            // contract that PasswordPolicyController.GetPasswordPolicy relies on).
            return null;
        }
    }

    /// <inheritdoc/>
    public Task<RepositoryResponse> UpdatePasswordPolicyAsync(
        IPasswordPolicy passwordPolicy,
        CancellationToken cancellationToken = default)
        => _repo.UpdatePasswordPolicyAsync(passwordPolicy, cancellationToken);
}
