using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface IManageUnifiedSettings
    {
		/// <summary>
		/// Get Security Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="category">Settings category</param>
		/// <param name="companyId">Organization</param>
		/// <param name="includes"></param>
		/// <returns>Security Settings List objects (KeyValue pairs)</returns>
		IList<Setting> GetSecuritySettings(string category, Guid companyId, string[] includes);

		/// <summary>
		/// Update an existing Security Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
		/// <param name="category">Settings category</param>
		/// <param name="companyId">Organization</param>
		/// <param name="includes"></param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdateSecuritySettings(IList<Setting> settings, string category, Guid companyId, string[] includes);
	}
}
