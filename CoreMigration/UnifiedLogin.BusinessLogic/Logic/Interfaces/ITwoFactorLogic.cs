using System;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
    public interface ITwoFactorLogic
    {
        int DeleteUserAppAuthToken(Guid realPageId);
        int UpdateUserTwoFactorStatus(Guid realPageId, int status);
    }
}
