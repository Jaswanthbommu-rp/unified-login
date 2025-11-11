using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using System.Net.Http;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Product Repository
	/// </summary>
	public class SharedDataRepository : BaseRepository, ISharedDataRepository
	{
		private DefaultUserClaim _userClaim;
        private IRepository _repository;

        #region Ctor
        /// <summary>
        /// base Constructor
        /// </summary>
        public SharedDataRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
			_userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };
		}

		/// <summary>
		/// Used when the user is known
		/// </summary>
		/// <param name="userClaim"></param>
		public SharedDataRepository(DefaultUserClaim userClaim) : base(DbConnectionEnum.IdpConfigurationDb)
		{
			if (userClaim == null)
				userClaim = new DefaultUserClaim { CorrelationId = Guid.NewGuid() };

			_userClaim = userClaim;
		}

		public SharedDataRepository(IRepository repository, DefaultUserClaim userClaims) : base(repository)
        {
            _repository = repository;
			_userClaim = userClaims;

        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Used to get a list of products ids for a company by the company guid
        /// </summary>
        /// <param name="organizationRealPageId"></param>
        /// <returns></returns>
        public IList<int> GetProductIdsByCompany(Guid organizationRealPageId)
		{
			RPObjectCache rpCache = new RPObjectCache();
			var cacheKey = $"getProductIdsByCompanyGuid_{organizationRealPageId}";

			return rpCache.GetFromCache<IList<int>>(cacheKey, 180, () =>
			{
				using (var repository = GetRepository())
				{
                    IList<int> productIdList = new List<int>();
                    IList<ProductUI> productList = repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, new { OrganizationRealPageId = organizationRealPageId }).ToList();
					foreach (ProductUI pui in productList)
					{
						productIdList.Add(pui.ProductId);
					}
                    return productIdList;
                }
			});
		}

		/// <summary>
		/// Used to get a list of products ids for a company by the company party id
		/// </summary>
		/// <param name="organizationPartyId"></param>
		/// <returns></returns>
		public IList<int> GetProductIdsByCompany(long organizationPartyId)
		{
			RPObjectCache rpCache = new RPObjectCache();
			var cacheKey = $"getProductIdsByCompanyPartyId_{organizationPartyId}";

			return rpCache.GetFromCache<IList<int>>(cacheKey, 180, () =>
			{
				using (var repository = GetRepository())
				{
                    IList<int> productIdList = new List<int>();
                    IList<ProductUI> productList = repository.GetMany<ProductUI>(StoredProcNameConstants.SP_ListProductsByOrganization, new { PartyId = organizationPartyId }).ToList();
					foreach (ProductUI pui in productList)
					{
						productIdList.Add(pui.ProductId);
					}
                    return productIdList;
                }
			});
		}

		#endregion
	}
}
