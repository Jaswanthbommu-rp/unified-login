using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
	public interface IGlobalSettingRepository
	{
		IEnumerable<GlobalSetting> GetGlobalSettings();
	}
}