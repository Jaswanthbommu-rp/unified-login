using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.EmployeeAccess;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

/// <summary>
/// Stepping-stone async wrapper around IManageEmployeeAccess.
/// Exposes async signatures to remove Task.Run() from the controller layer;
/// the underlying sync logic will be replaced with truly-async calls in a future pass.
/// </summary>
public sealed class ManageEmployeeAccessAsync : IManageEmployeeAccessAsync
{
    private readonly IManageEmployeeAccessAsync _manageEmployeeAccess;

    public ManageEmployeeAccessAsync(IManageEmployeeAccessAsync manageEmployeeAccess)
    {
        _manageEmployeeAccess = manageEmployeeAccess ?? throw new ArgumentNullException(nameof(manageEmployeeAccess));
    }

    public async Task<ListResponse> GetCompaniesAsync(long editorPersonaId, string filter, CancellationToken cancellationToken = default)
        => await _manageEmployeeAccess.GetCompaniesAsync(editorPersonaId, filter, cancellationToken);

    public async Task<ListResponse> GetUsersAsync(long editorPersonaId, string filter, CancellationToken cancellationToken = default)
        => await _manageEmployeeAccess.GetUsersAsync(editorPersonaId, filter, cancellationToken);

    public async Task<EmployeePersona> GetOrCreateEmployeePersonaIdAsync(Guid companyRealPageId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default)
        => await _manageEmployeeAccess.GetOrCreateEmployeePersonaIdAsync(companyRealPageId, userClaim, cancellationToken);

    public async Task<string> CreateEmployeeProductUserAsync(int productId, long personaId, CancellationToken cancellationToken = default)
    {
        var result = await _manageEmployeeAccess.CreateEmployeeProductUserAsync(productId, personaId, cancellationToken);
        if (result.Equals("DeletedProductLogin", StringComparison.OrdinalIgnoreCase))
        {
            // Product login was disabled; retry once to see if another AD group is assignable
            result = await _manageEmployeeAccess.CreateEmployeeProductUserAsync(productId, personaId, cancellationToken);
        }
        return result;
    }
}
