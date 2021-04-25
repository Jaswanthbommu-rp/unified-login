using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
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
