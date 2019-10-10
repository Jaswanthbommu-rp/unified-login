using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Preferred Contact Method Repository
	/// </summary>
	public class PreferredContactMethodRepository : BaseRepository, IPreferredContactMethodRepository
	{
		#region Constructor
		/// <summary>
		/// Preferred Contact Method base Constructor
		/// </summary>
		public PreferredContactMethodRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
	}
	#endregion

	#region public PreferredContactMethod Repository methods
	/// <summary>
	/// Get a list of Preferred Contact Methods
	/// </summary>
	/// <returns>List of Preferred Contact Methods</returns>
	public IList<PreferredContactMethod> ListPreferredContactMethod()
	{
		using (var repository = GetRepository())
		{
			IList<PreferredContactMethod> result = repository.GetMany<PreferredContactMethod>(StoredProcNameConstants.SP_ListPreferredContactMethods, null).ToList();
			return result;
		}
	}
	#endregion
}
}