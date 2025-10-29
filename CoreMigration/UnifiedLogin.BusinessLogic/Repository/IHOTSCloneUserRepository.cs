using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Repository
{
    public interface IHOTSCloneUserRepository
	{
		IList<BaseLineCustomerCompanyUser> ListUsers(long OrganizationId);
		List<PersonaProductUserDetails> GetUserProducts(long personaId);
		Guid GetBaseCompanyUPFMId(Guid cloneUpfmId);

        UserLoginOnly GetUserLoginOnly(string enterpriseUserName);

        IList<Persona> ListPersona(Guid realPageId);

        HotsUser CreateUser(DefaultUserClaim cloneCompanyAdminUserClaim, long partyId, BaseLineCustomerCompanyUser user, IProfileDetail baseUserProfile, List<ProductBatch> productBatch, UserLogin userLogin);

        /// <summary>
        /// Used to link a cloned company to a baseline company when using HOTS
        /// </summary>
        /// <param name="baselineCompanyRealPageId"></param>
        /// <param name="cloneCompanyRealPageId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        RepositoryResponse InsertHotsCompanyRelationship(Guid baselineCompanyRealPageId, Guid cloneCompanyRealPageId, int userId);

        /// <summary>
        /// Used to link a cloned property to a baseline property when using HOTS
        /// </summary>
        /// <param name="baselinePropertyInstanceId"></param>
        /// <param name="clonePropertyInstanceId"></param>
        /// <param name="cloneCompanyRealPageId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        RepositoryResponse InsertHotsPropertyRelationship(Guid baselinePropertyInstanceId, Guid clonePropertyInstanceId, Guid cloneCompanyRealPageId, int userId);
    }
}
