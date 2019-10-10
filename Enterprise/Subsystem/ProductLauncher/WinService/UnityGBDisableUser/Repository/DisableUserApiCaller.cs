using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Repository
{
    public class DisableUserApiCaller : IDisableUser
	{
		public async Task<string> DisableExpiredUsers(List<ProcessUserLogin> userLogin)
		{
			var result = await ApiCaller.PostApi<string, List<ProcessUserLogin>>(userLogin, $"api/disableexpiredusers");
			return result;
		}
	}
}
