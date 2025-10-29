using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository
{
	public class GlobalSettingRepository : BaseRepository, IGlobalSettingRepository
	{

		#region Constructor

		/// <summary>
		/// Profile base Constructor
		/// </summary>
		public GlobalSettingRepository() : base(DbConnectionEnum.IdpConfigurationDb) { }
		#endregion

		#region public Profile methods

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public IEnumerable<GlobalSetting> GetGlobalSettings()
		{
			using (var repository = GetRepository())
			{
				var allGlobalSettings = repository.GetMany<GlobalSetting>(StoredProcNameConstants.SP_GetGlobalSettings, null);
				return allGlobalSettings;
			}

		}


        public RepositoryResponse UpdateGlobalSetting(GlobalSetting setting)
        {
            dynamic param = new
            {
                @MasterConfigurationSettingId = setting.MasterConfigurationSettingId,
                @Value = setting.Value
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateMasterConfigurationSetting, param);
                return result;
            }
        }

		#endregion
	}
}