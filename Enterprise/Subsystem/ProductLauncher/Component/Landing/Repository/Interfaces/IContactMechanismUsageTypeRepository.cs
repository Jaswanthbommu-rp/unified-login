using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
	/// <summary>
	/// Interface for ContactMechanismUsageTypeRepository
	/// </summary>
	public interface IContactMechanismUsageTypeRepository
	{
		/// <summary>
		/// Get a list of Contact Mechanism Usage Types
		/// </summary>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Contact Mechanism Usage Types</returns>
		IList<ContactMechanismUsageType> ListContactMechanismUsageType(string ContactMechanismUsageTypeName);
	}
}