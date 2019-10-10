using RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.WinService.UnityGBDisableUser.Repository
{
    public interface IDisableUser
	{
		Task<string> DisableExpiredUsers(List<ProcessUserLogin> userLogin);
	}
}
