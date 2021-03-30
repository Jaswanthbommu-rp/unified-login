using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface IManageUnifiedSettings
    {
		/// <summary>
		/// Get Company Settings
		/// </summary>
		/// <param name="category">Setting Category type</param>
		/// <param name="partyId">Company Id</param>
		/// <returns>Security Settings List objects (KeyValue pairs)</returns>
		ISettingResponse GetUnifiedSettings(string category, long partyId);

		IList<Picklist> GetSettingsPickList(string category);

        IList<Setting> GetUnifiedSettingsCached(string category, long partyId);

		/// <summary>
		/// Update an existing Security Settings 
		/// </summary>
		/// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
		/// <param name="category">Settings category</param>
		/// <param name="companyId">Organization</param>
		/// <param name="includes"></param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdateUnifiedSettings(IList<Setting> settings, string category, long companyId, string[] includes = null);

		/// <summary>
		///Send Property Instance to Unified settings
		/// </summary>
		/// <param name="upfmProperties">upfmProperties</param>
		/// <param name="requestType">requestType</param>
		/// <returns></returns>
		bool CreateUpdatePropertyInSetting(UnifiedSettingPropertyPayload upfmProperties, HttpMethod requestType);

		/// <summary>
		/// Delete Property In Setting
		/// </summary>
		/// <param name="propertyInstance"></param>
		/// <returns></returns>
		bool DeletePropertyInSetting(Guid propertyInstance);
		RepositoryResponse SaveTableSettings(long partyId, string category, string operation,  List<SettingRow> rows);
	}
}
