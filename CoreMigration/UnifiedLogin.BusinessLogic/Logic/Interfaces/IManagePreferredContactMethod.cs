using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for ManagePreferredContactMethod
	/// </summary>
	public interface IManagePreferredContactMethod
	{
		/// <summary>
		/// Get a list of Preferred Contact Methods
		/// </summary>
		/// <returns>List of Preferred Contact Methods</returns>
		IList<PreferredContactMethod> ListPreferredContactMethod();
	}
}