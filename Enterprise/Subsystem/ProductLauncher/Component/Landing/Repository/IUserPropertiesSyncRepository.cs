using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    public  interface IUserPropertiesSyncRepository
    {
        UserSyncData GetUserSyncData(long syncJobTaskId);
    }
}
