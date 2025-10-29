using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;

namespace UnifiedLogin.BusinessLogic.Repository
{
    public class UnifiedSettingsRepository : BaseRepository, IUnifiedSettingsRepository
	{
        #region Constructor
        /// <summary>
        /// Unified Settings base Constructor
        /// </summary>
        public UnifiedSettingsRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        public UnifiedSettingsRepository(IRepository repository) : base(repository)
        {

        }
		#endregion
		#region public Security Settings methods
		/// <summary>
		/// Get Unified Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="PartyId">partyid</param>
		/// <param name="Category">setting category</param>
		/// <returns> Settings List objects (KeyValue pairs)</returns>
		public IList<Setting> GetUnifiedSettings(long PartyId, string Category)
		{
			dynamic param = new
			{
				PartyId = PartyId,
				Category = Category
			};

			using (var repository = GetRepository())
			{
				return repository.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, param);
			}
		}

		
		
		#endregion
	}
}
