namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface ITwoFactorLogicAsync
{
    Task<int> DeleteUserAppAuthTokenAsync(Guid realPageId, CancellationToken cancellationToken = default);
    Task<int> UpdateUserTwoFactorStatusAsync(Guid realPageId, int status, CancellationToken cancellationToken = default);
}
