using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
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

		#endregion
	}
}