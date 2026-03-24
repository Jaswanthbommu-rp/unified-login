using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	public interface IGlobalSettingRepositoryAsync
	{
		Task<IList<GlobalSetting>> GetGlobalSettingsAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Exposed from the implementation — absent from the original sync interface.
		/// </summary>
		Task<RepositoryResponse> UpdateGlobalSettingAsync(GlobalSetting setting, CancellationToken cancellationToken = default);
	}
}