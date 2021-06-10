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
        private string _greenBookAccessToken;
		private ISettingRepository _settingRepository;
		IProductInternalSettingRepository productInternalSettingRepository;
		#endregion

		#region Ctor
		/// <summary>
		/// UserManagement Constructor
		/// </summary>
		/// <param name="userClaims"></param>
		/// <param name="greenBookAccessToken"></param>
		public SettingsManagement(DefaultUserClaim userClaims, string greenBookAccessToken)
		{
			_userClaims = userClaims;
			_greenBookAccessToken = greenBookAccessToken;
			_settingRepository = new SettingsRepository(userClaims);
		}
		#endregion

		/// <summary>
		/// Get Company International Settings
		/// </summary>
		/// <param name="loggedInUser"></param>
		/// <param name="settingType"></param>
		/// <returns></returns>
		public SettingResponse GetCompanyInternationalSettings(Guid companyId, string settingType)
		{
			//SettingsRepository settingRepository = new SettingsRepository(_userClaims);
			//string companyId = OrganizationGuid(loggedInUser);
			return _settingRepository.GetCompanyInternationalSettings(companyId.ToString(), "UPFM", settingType);
		}
		//public string OrganizationGuid(DefaultUserClaim cp)
		//{
		//	return (cp.Claims.FirstOrDefault(c => c.Type == "orgId")?.Value
		//					  ?? throw new System.Exception("No orgId claim was present!"));
		//}
	}
}
