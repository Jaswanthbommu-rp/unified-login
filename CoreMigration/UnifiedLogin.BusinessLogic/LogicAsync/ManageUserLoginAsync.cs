using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper for user login operations.
/// Delegates to the existing sync <see cref="IManageUserLogin"/> via <see cref="Task.FromResult{TResult}"/>.
/// </summary>
public sealed class ManageUserLoginAsync : IManageUserLoginAsync
{
    private readonly IManageUserLogin _manageUserLogin;

    public ManageUserLoginAsync(IManageUserLogin manageUserLogin)
    {
        _manageUserLogin = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
    }

    public Task<IList<Organization>> ListOrganizationByEnterpriseUserIdAsync(Guid realPageId, string relationshipType = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageUserLogin.ListOrganizationByEnterpriseUserId(realPageId, relationshipType));

    public Task<UserLoginOnly> GetUserLoginOnlyAsync(Guid realPageId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageUserLogin.GetUserLoginOnly(realPageId));

    public Task<UserLogin> GetUserLoginAsync(Guid realPageId, long orgPartyId, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageUserLogin.GetUserLogin(realPageId, orgPartyId));

    public Task<IList<UserOrganization>> GetUserPersonaOrganizationAsync(string loginName, Guid? organizationRealPageId = null, CancellationToken cancellationToken = default)
        => Task.FromResult(_manageUserLogin.GetUserPersonaOrganization(loginName, organizationRealPageId));
}
