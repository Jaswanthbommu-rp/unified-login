namespace UnifiedLogin.BusinessLogic.Repository.Interfaces;

public interface ITwoFactorRepositoryAsync
{
    Task<int> ResetAuthenticatorKeyAsync(long userId, string authenticatorKey, CancellationToken cancellationToken = default);
    Task<int> UpdateUserTwoFactorStatusAsync(long userId, int status, CancellationToken cancellationToken = default);
}
