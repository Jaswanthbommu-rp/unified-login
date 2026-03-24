using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Shared Data Repository
	/// </summary>
	public interface ISharedDataRepositoryAsync
	{
		/// <summary>
		/// Used to get a list of products ids for a company by the company guid
		/// </summary>
		/// <param name="organizationRealPageId"></param>
		/// <returns></returns>
		Task<IList<int>> GetProductIdsByCompanyAsync(Guid organizationRealPageId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Used to get a list of products ids for a company by the company party id
		/// </summary>
		/// <param name="organizationPartyId"></param>
		/// <returns></returns>
		Task<IList<int>> GetProductIdsByCompanyAsync(long organizationPartyId, CancellationToken cancellationToken = default);

	}
}