using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class TwoFactorLogicAsync : ITwoFactorLogicAsync
{
    private readonly ITwoFactorRepositoryAsync _twoFactorRepository;
    private readonly IUserLoginRepositoryAsync _userLoginRepository;

    public TwoFactorLogicAsync(
        ITwoFactorRepositoryAsync twoFactorRepository,
        IUserLoginRepositoryAsync userLoginRepository)
    {
        _twoFactorRepository = twoFactorRepository ?? throw new ArgumentNullException(nameof(twoFactorRepository));
        _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
    }

    public async Task<int> DeleteUserAppAuthTokenAsync(Guid realPageId, CancellationToken cancellationToken = default)
    {
        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
        if (userLogin == null)
            return 0;

        return await _twoFactorRepository.ResetAuthenticatorKeyAsync(userLogin.UserId, string.Empty, cancellationToken);
    }

    public async Task<int> UpdateUserTwoFactorStatusAsync(Guid realPageId, int status, CancellationToken cancellationToken = default)
    {
        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
        if (userLogin == null)
            return 0;

        return await _twoFactorRepository.UpdateUserTwoFactorStatusAsync(userLogin.UserId, status, cancellationToken);
    }
}
