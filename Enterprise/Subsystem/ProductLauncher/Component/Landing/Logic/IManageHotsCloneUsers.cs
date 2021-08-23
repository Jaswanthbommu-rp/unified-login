using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Hots;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public interface IManageHotsCloneUsers
	{
		ClonedUsers CloneUsersFromBaseLineCompany(CloneUsers cloneUsers, long basePartyId, long clonePartyId, DefaultUserClaim baseOrgAdminClaim, long personaId);
		Guid GetBaseCompanyUPFMId(Guid cloneUpfmId);
	}
}
