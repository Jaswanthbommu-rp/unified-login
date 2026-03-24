using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface IPasswordPolicyRepositoryAsync
{
    Task<PasswordPolicy> GetPasswordPolicyAsync(long partyId, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> UpdatePasswordPolicyAsync(IPasswordPolicy passwordPolicy, CancellationToken cancellationToken = default);
    Task<RepositoryResponse> CreatePasswordPolicyAsync(IPasswordPolicy passwordPolicy, CancellationToken cancellationToken = default);
}