using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Repository
{
    public  interface IUserPropertiesSyncRepository
    {
        UserSyncData GetUserSyncData(long syncJobTaskId);
    }
}
