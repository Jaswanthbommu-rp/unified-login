using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Hots;
using UnifiedLogin.SharedObjects.Landing;
using System;

namespace UnifiedLogin.BusinessLogic.Logic
{
    public interface IManageHotsCloneUsers
	{
		ClonedUsers CloneUsersFromBaseLineCompany(CloneUsers cloneUsers, long basePartyId, long clonePartyId, DefaultUserClaim baseOrgAdminClaim, long personaId);
		Guid GetBaseCompanyUPFMId(Guid cloneUpfmId);

        /// <summary>
        /// Used to link a cloned company to a baseline company when using HOTS
        /// </summary>
        /// <param name="baselineCompanyRealPageId"></param>
        /// <param name="cloneCompanyRealPageId"></param>
        /// <param name="userId"></param>
        RepositoryResponse InsertHotsCompanyRelationship(Guid baselineCompanyRealPageId, Guid cloneCompanyRealPageId, int userId);

        /// <summary>
        /// Used to link a cloned property to a baseline property when using HOTS
        /// </summary>
        /// <param name="baselinePropertyInstanceId"></param>
        /// <param name="clonePropertyInstanceId"></param>
        /// <param name="cloneCompanyRealPageId"></param>
        /// <param name="userId"></param>
        RepositoryResponse InsertHotsPropertyRelationship(Guid baselinePropertyInstanceId, Guid clonePropertyInstanceId, Guid cloneCompanyRealPageId, int userId);
    }
}
