using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	public interface IGlobalSettingRepository
	{
		IEnumerable<GlobalSetting> GetGlobalSettings();
	}
}