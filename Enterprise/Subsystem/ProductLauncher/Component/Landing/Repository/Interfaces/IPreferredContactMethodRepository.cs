using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
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