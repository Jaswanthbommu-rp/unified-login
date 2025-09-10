using System;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface ITwoFactorLogic
    {
        int DeleteUserAppAuthToken(Guid realPageId);
        int UpdateUserTwoFactorStatus(Guid realPageId, int status);
        void RemoveDeviceTrust(HttpResponse context, Guid userId);
    }
}
