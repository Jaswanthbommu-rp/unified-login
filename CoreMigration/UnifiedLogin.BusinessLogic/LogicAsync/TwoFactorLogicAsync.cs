using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Rum;

namespace UnifiedLogin.BusinessLogic.LogicAsync;

public sealed class TwoFactorLogicAsync : ITwoFactorLogicAsync
{
    private readonly ITwoFactorRepositoryAsync _twoFactorRepository;
    private readonly IUserLoginRepositoryAsync _userLoginRepository;
    private readonly IUserClaimsAccessor _userClaim;
    private readonly IPersonRepositoryAsync _personRepository;

    public TwoFactorLogicAsync(ITwoFactorRepositoryAsync twoFactorRepository,
        IUserClaimsAccessor userClaimAccessor,
        IUserLoginRepositoryAsync userLoginRepository,
        IPersonRepositoryAsync personRepository)
    {
        _twoFactorRepository = twoFactorRepository ?? throw new ArgumentNullException(nameof(twoFactorRepository));
        _userLoginRepository = userLoginRepository ?? throw new ArgumentNullException(nameof(userLoginRepository));
        _userClaim = userClaimAccessor ?? throw new ArgumentNullException(nameof(userClaimAccessor));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
    }

    public async Task<int> DeleteUserAppAuthTokenAsync(Guid realPageId, CancellationToken cancellationToken = default)
    {
        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
        if (userLogin == null)
            return 0;

        var result = await _twoFactorRepository.ResetAuthenticatorKeyAsync(userLogin.UserId, string.Empty, cancellationToken);
        if (result > 0)
        {
             await LogDeleteActivity(realPageId, userLogin);
        }
        return result;
    }

    public async Task<int> UpdateUserTwoFactorStatusAsync(Guid realPageId, int status, CancellationToken cancellationToken = default)
    {
        var userLogin = await _userLoginRepository.GetUserLoginOnlyAsync(realPageId);
        if (userLogin == null)
            return 0;

        return await _twoFactorRepository.UpdateUserTwoFactorStatusAsync(userLogin.UserId, status, cancellationToken);
    }

    private async Task LogDeleteActivity(Guid realPageId, UserLoginOnly userLogin)
    {
        var person = await _personRepository.GetPersonAsync(realPageId);
        if (realPageId == _userClaim.UserRealPageGuid)
        {
            LogActivity.WriteActivity(new ActivityDetails()
            {
                LogActivityTypeName = "Update User",
                LogCategoryName = LogActivityCategoryType.User.ToString(),
                CorrelationId = _userClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                OrganizationPartyId = _userClaim.OrganizationPartyId,
                Message = $"Multi-factor authentication method reset by {person.FirstName} {person.LastName}.",

                FromUserLoginName = _userClaim.LoginName,
                FromUserLoginId = _userClaim.UserId,
                FromUserFirstName = _userClaim.FirstName,
                FromUserLastName = _userClaim.LastName,
                FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),
                BooksProductCode = "UPFM"
            });
        }
        else
        {
            LogActivity.WriteActivity(new ActivityDetails()
            {
                LogActivityTypeName = "Update User",
                LogCategoryName = LogActivityCategoryType.User.ToString(),
                CorrelationId = _userClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                OrganizationPartyId = _userClaim.OrganizationPartyId,
                Message = $"{_userClaim.FirstName} {_userClaim.LastName} reset the multi-factor authentication setup for {person.FirstName} {person.LastName}.",

                FromUserLoginName = _userClaim.LoginName,
                FromUserLoginId = _userClaim.UserId,
                FromUserFirstName = _userClaim.FirstName,
                FromUserLastName = _userClaim.LastName,
                FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),

                ToUserFirstName = person.FirstName,
                ToUserLastName = person.LastName,
                ToUserLoginId = userLogin.UserId,
                ToUserLoginName = userLogin.LoginName,
                ToUserRealpageId = userLogin.RealPageId.ToString(),

                BooksProductCode = "UPFM"
            });
        }
    }
}