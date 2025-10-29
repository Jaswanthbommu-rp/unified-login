using System;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Shared Data Repository
	/// </summary>
	public interface ISharedDataRepository
	{
		/// <summary>
		/// Used to get a list of products ids for a company by the company guid
		/// </summary>
		/// <param name="organizationRealPageId"></param>
		/// <returns></returns>
		IList<int> GetProductIdsByCompany(Guid organizationRealPageId);

		/// <summary>
		/// Used to get a list of products ids for a company by the company party id
		/// </summary>
		/// <param name="organizationPartyId"></param>
		/// <returns></returns>
		IList<int> GetProductIdsByCompany(long organizationPartyId);

	}
}