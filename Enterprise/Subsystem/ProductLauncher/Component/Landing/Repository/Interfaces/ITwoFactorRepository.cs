namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
    public interface ITwoFactorRepository
    {
        int ResetAuthenticatorKey(long userId, string authenticatorKey);
    }
}
