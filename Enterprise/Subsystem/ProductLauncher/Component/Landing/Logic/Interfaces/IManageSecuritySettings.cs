using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	/// <summary>
	/// Interface for ManageSecuritySettings
	/// </summary>
	public interface IManageSecuritySettings
	{
		/// <summary>
		/// Get Security Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="bookMasterId">Book MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>Security Settings List objects (KeyValue pairs)</returns>
		IList<Setting> GetSecuritySettings(long bookMasterId, int bookMasterTypeId = (int)BookMasterType.CompanyMasterId);

		/// <summary>
		/// Update an existing Security Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
		/// <param name="bookMasterId">BlackBookId MasterBook Id</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdateSecuritySettings(IList<Setting> settings, long bookMasterId, int bookMasterTypeId = (int)BookMasterType.CompanyMasterId);
	}
}