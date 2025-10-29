using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    public interface IManageUnifiedSettings
    {
        /// <summary>
        /// Get Company Settings
        /// </summary>
        /// <param name="category">Setting Category type</param>
        /// <param name="partyId">Company Id</param>
        /// <returns>Security Settings List objects (KeyValue pairs)</returns>
        IList<Setting> GetUnifiedSettings(string category, long partyId);

        IList<Setting> GetUnifiedSettingsCached(string category, long partyId);

		/// <summary>
		/// Send Company Instance to Unified settings
		/// </summary>
		/// <param name="upfmCompany"></param>
		/// <param name="requestType"></param>
		/// <returns></returns>
		bool CreateUpdateCompanyInSetting(UnifiedSettingCompanyPropertyPayload upfmCompany, HttpMethod requestType);

		/// <summary>
		///Send Property Instance to Unified settings
		/// </summary>
		/// <param name="upfmProperties">upfmProperties</param>
		/// <param name="requestType">requestType</param>
		/// <returns></returns>
		bool CreateUpdatePropertyInSetting(UnifiedSettingCompanyPropertyPayload upfmProperties, HttpMethod requestType);

		/// <summary>
		/// Delete Property In Setting
		/// </summary>
		/// <param name="propertyInstance"></param>
		/// <returns></returns>
		bool DeletePropertyInSetting(string propertyInstance);

		InternalSettingResponse GetCompanyInternalSettings(Guid companyId, string source, string settingType);
	}
}
