using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for PostalAddressRepository
	/// </summary>
	public interface IPostalAddressRepository
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