using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for ManageContactMechanismUsageType
	/// </summary>
	public interface IManageContactMechanismUsageType
	{
		/// <summary>
		/// Get a list of Contact Mechanism Usage Types
		/// </summary>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Contact Mechanism Usage Types</returns>
		IList<ContactMechanismUsageType> ListContactMechanismUsageType(string ContactMechanismUsageTypeName);
	}
}