using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    public interface IUnifiedSettingsRepository
    {
        IList<Setting> GetUnifiedSettings(long PartyId, string Category);
       
    }
}
