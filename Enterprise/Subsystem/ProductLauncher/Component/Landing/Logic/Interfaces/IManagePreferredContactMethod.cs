using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
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