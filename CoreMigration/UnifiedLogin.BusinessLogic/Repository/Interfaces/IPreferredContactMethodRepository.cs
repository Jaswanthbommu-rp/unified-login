using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for PreferredContactMethodRepository
	/// </summary>
	public interface IPreferredContactMethodRepository
	{
		/// <summary>
		/// Get a list of Preferred Contact Methods
		/// </summary>
		/// <returns>List of Preferred Contact Methods</returns>
		IList<PreferredContactMethod> ListPreferredContactMethod();
	}
}