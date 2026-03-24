using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for ContactMechanismUsageTypeRepository
	/// </summary>
	public interface IContactMechanismUsageTypeRepositoryAsync
	{
		/// <summary>
		/// Get a list of Contact Mechanism Usage Types
		/// </summary>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Contact Mechanism Usage Types</returns>
		Task<IList<ContactMechanismUsageType>> ListContactMechanismUsageTypeAsync(string ContactMechanismUsageTypeName, CancellationToken cancellationToken = default);
	}
}