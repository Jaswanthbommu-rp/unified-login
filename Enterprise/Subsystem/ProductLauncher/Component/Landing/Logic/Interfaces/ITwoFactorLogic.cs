using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface ITwoFactorLogic
    {
        int DeleteUserAppAuthToken(Guid realPageId);
    }
}
