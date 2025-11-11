using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
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