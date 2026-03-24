using System.Collections.Generic;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for PreferredContactMethodRepository
	/// </summary>
	public interface IPreferredContactMethodRepositoryAsync
	{
		/// <summary>
		/// Get a list of Preferred Contact Methods
		/// </summary>
		/// <returns>List of Preferred Contact Methods</returns>
		Task<IList<PreferredContactMethod>> ListPreferredContactMethodAsync(CancellationToken cancellationToken = default);
	}
}