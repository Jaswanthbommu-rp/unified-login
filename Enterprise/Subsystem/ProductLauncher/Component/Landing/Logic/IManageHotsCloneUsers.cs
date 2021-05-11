using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Hots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
	public interface IManageHotsCloneUsers
	{
		ClonedUsers CloneUsersFromBaseLineCompany(CloneUsers cloneUsers, long basePartyId, long clonePartyId);
		Guid GetBaseCompanyUPFMId(Guid cloneUpfmId);
	}
}
