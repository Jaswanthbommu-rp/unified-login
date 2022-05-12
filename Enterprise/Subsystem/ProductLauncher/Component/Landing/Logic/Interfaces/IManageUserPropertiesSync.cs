using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface IManageUserPropertiesSync
    {
		/// <summary>
		/// Translate and saves the user product primary properties
		/// </summary>
		/// <param name="UserSyncData">userData</param>
		/// <returns></returns>
		RepositoryResponse TranslateAndSaveUserProductProperties(UserSyncJobTask userData);
	}
}
