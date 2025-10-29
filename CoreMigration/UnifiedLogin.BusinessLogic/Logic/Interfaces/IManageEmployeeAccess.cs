using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.EmployeeAccess;
using System;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    public interface IManageEmployeeAccess
    {
        /// <summary>
        /// Returns Companies (Companies in GB)
        /// </summary>
        ListResponse GetCompanies(long editorPersonaId, string filter);

        /// <summary>
        /// Returns Users in Unified Login
        /// </summary>
        ListResponse GetUsers(long editorPersonaId, string filter);

        EmployeePersona GetOrCreateEmployeePersonaId(Guid companyRealPageId, DefaultUserClaim userClaim);

        string CreateEmployeeProductUser(int productId, long personaId);

    }
}