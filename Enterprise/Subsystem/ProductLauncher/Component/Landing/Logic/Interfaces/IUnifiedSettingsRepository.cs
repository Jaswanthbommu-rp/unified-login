using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface IUnifiedSettingsRepository
    {
        IList<Setting> GetUnifiedSettings(long PartyId, string Category);
        RepositoryResponse UpdateUnifiedSettings(IList<Setting> settings, long PartyId, string Category, long userId);
    }
}
