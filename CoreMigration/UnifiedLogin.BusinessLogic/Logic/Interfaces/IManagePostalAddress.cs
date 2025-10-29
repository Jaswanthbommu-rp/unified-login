using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage Postal Address repository calls
	/// </summary>
	public interface IManagePostalAddress
	{
		/// <summary>
		/// List postal address details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Postal Address details for a person</returns>
		IList<PostalAddress> ListPostalAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName);
	}
}