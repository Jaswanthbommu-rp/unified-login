using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Enterprise
{
    interface ISettingRepository
    {
		/// <summary>
		/// Get Organization details
		/// </summary>
		/// <param name="companyId">company unique identifier</param>
		/// <param name="source">source</param>
		/// <param name="settingType">settingType</param>
		/// <returns>Organization object</returns>
		SettingResponse GetCompanyInternationalSettings(string companyId, string source, string settingType);
	}
}
