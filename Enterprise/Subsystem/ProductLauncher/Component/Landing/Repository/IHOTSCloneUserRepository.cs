using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Hots;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	public interface IHOTSCloneUserRepository
	{
		IList<BaseLineCustomerCompanyUser> ListUsers(long OrganizationId);
		List<PersonaProductUserDetails> GetUserProducts(long personaId);
		Guid GetBaseCompanyUPFMId(Guid cloneUpfmId);
		HotsUser CreateUser(long partyId, BaseLineCustomerCompanyUser user, IProfileDetail baseUserProfile, List<ProductBatch> productBatch);
	}
}
