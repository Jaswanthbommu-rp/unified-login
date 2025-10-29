namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    public interface ITwoFactorRepository
    {
        int ResetAuthenticatorKey(long userId, string authenticatorKey);
        int UpdateUserTwoFactorStatus(long userId, int status);
    }
}
