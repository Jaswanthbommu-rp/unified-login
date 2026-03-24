using UnifiedLogin.SharedObjects;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for SecuritySettingsRepository
	/// </summary>
	public interface ISecuritySettingsRepositoryAsync
	{
		/// <summary>
		/// Get Security Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Security Settings List objects (KeyValue pairs)</returns>
		Task<IList<Setting>> GetSecuritySettingsAsync(long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Update Security Settings (Password and Activity Configuration Security Settings)
		/// </summary>
		/// <param name="settings">Security Settings (Password and Activity Configuration Security Settings) object of the parameter values</param>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> UpdateSecuritySettingsAsync(IList<Setting> settings, long booksCustomerMasterId, int bookMasterTypeId = (int)BookMasterType.CustomerMasterId, CancellationToken cancellationToken = default);
	}
}