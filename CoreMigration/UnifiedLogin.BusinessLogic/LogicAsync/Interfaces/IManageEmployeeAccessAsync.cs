using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.EmployeeAccess;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManageEmployeeAccessAsync
{
    Task<ListResponse> GetCompaniesAsync(long editorPersonaId, string filter, CancellationToken cancellationToken = default);
    Task<ListResponse> GetUsersAsync(long editorPersonaId, string filter, CancellationToken cancellationToken = default);
    Task<EmployeePersona> GetOrCreateEmployeePersonaIdAsync(Guid companyRealPageId, DefaultUserClaim userClaim, CancellationToken cancellationToken = default);
    Task<string> CreateEmployeeProductUserAsync(int productId, long personaId, CancellationToken cancellationToken = default);
}
