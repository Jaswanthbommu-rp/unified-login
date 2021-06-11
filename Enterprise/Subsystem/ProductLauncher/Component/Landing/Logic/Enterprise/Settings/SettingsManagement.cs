using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Settings
{
    public class SettingsManagement
    {
        #region Private variables
        private DefaultUserClaim _userClaims;
		private ISettingRepository _settingRepository;
		#endregion

		#region Ctor
		/// <summary>
		/// UserManagement Constructor
		/// </summary>
		/// <param name="userClaims"></param>
		public SettingsManagement(DefaultUserClaim userClaims)
		{
			_userClaims = userClaims;
			_settingRepository = new SettingsRepository(userClaims);
		}
		#endregion

		/// <summary>
		/// Get Company International Settings
		/// </summary>
		/// <param name="companyId"></param>
		/// <param name="settingType"></param>
		/// <returns></returns>
		public SettingResponse GetCompanyInternationalSettings(Guid companyId, string settingType)
		{
			return _settingRepository.GetCompanyInternationalSettings(companyId.ToString(), "UPFM", settingType);
		}
	}
}
