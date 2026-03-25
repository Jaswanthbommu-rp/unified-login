using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

/// <summary>
/// Async-first interface for user login operations.
/// Replaces: sync <see cref="UnifiedLogin.BusinessLogic.Logic.Interfaces.IManageUserLogin"/> + blocking <c>Task.Run</c> calls.
/// </summary>
public interface IManageUserLoginAsync
{
    Task<IList<Organization>> ListOrganizationByEnterpriseUserIdAsync(Guid realPageId, string relationshipType = null, CancellationToken cancellationToken = default);
    Task<UserLoginOnly> GetUserLoginOnlyAsync(Guid realPageId, CancellationToken cancellationToken = default);
    Task<UserLogin> GetUserLoginAsync(Guid realPageId, long orgPartyId, CancellationToken cancellationToken = default);
    Task<IList<UserOrganization>> GetUserPersonaOrganizationAsync(string loginName, Guid? organizationRealPageId = null, CancellationToken cancellationToken = default);
}
