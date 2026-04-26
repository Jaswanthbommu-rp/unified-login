using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Product;

/// <summary>
/// Builds <see cref="UserActivityLogInfo"/> snapshots and pushes them to the
/// activity log queue. Extracted from the file-level <c>SaveInteralSamlAttrLog</c>
/// inner class so it can be injected and unit-tested independently.
/// </summary>
public sealed class ProductUserActivityLogHelper
{
    private readonly IManagePersonaAsync      _managePersona;
    private readonly IManagePersonAsync       _managePerson;
    private readonly IManageUserLoginAsync    _manageUserLogin;
    private readonly IManageOrganizationAsync _manageOrganization;
    private readonly IUserClaimsAccessor    _defaultUserClaim;

    public ProductUserActivityLogHelper(IUserClaimsAccessor defaultUserClaim,
        IManagePersonaAsync managePersona,
        IManagePersonAsync managePerson,
        IManageUserLoginAsync manageUserLogin,
        IManageOrganizationAsync manageOrganization
        )
    {
        _defaultUserClaim   = defaultUserClaim ?? throw new ArgumentNullException(nameof(defaultUserClaim));
        _managePersona      = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
        _managePerson       = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
        _manageUserLogin    = manageUserLogin ?? throw new ArgumentNullException(nameof(manageUserLogin));
        _manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
    }

    public async Task<UserActivityLogInfo> GetUserActivityLogInfo(long personaId, DefaultUserClaim? userClaim = null)
    {
        if (personaId == 0)
        {
            Guid employeeRealPageId = await _manageOrganization.GetOrganizationAdminUserRealPageIdAsync(DefaultUserClaim.EmployeeCompanyRealPageId);
            var  person    = await _managePerson.GetPersonAsync(employeeRealPageId);
            var  userLogin = await _manageUserLogin.GetUserLoginOnlyAsync(employeeRealPageId);
            var  persona   = await _managePersona.GetActivePersonaAsync(employeeRealPageId);
            return new UserActivityLogInfo
            {
                FirstName                = person.FirstName,
                LastName                 = person.LastName,
                RealPageId               = userLogin.RealPageId,
                LoginName                = userLogin.LoginName,
                BooksOrganizationMasterId = persona.Organization.BooksMasterId,
                OrganizationPartyId      = persona.OrganizationPartyId,
                OrganizationName         = persona.Organization.Name,
                UserId                   = userLogin.UserId,
                ClientCode               = userClaim?.ClientCode
            };
        }

        var p  = await _managePersona.GetPersonaAsync(personaId);
        var ul = await _manageUserLogin.GetUserLoginOnlyAsync(p.RealPageId);
        var pe = await _managePerson.GetPersonAsync(p.RealPageId);
        return new UserActivityLogInfo
        {
            FirstName                = pe.FirstName,
            LastName                 = pe.LastName,
            RealPageId               = ul.RealPageId,
            LoginName                = ul.LoginName,
            BooksOrganizationMasterId = p.Organization.BooksMasterId,
            OrganizationPartyId      = p.OrganizationPartyId,
            OrganizationName         = p.Organization.Name,
            UserId                   = ul.UserId,
            OrganizationRealpageId   = p.Organization.RealPageId
        };
    }

    public void PushToQueue(
        UserActivityLogInfo fromUserLogInfo, UserActivityLogInfo toUserLogInfo,
        string message, string logActivityType,
        List<AdditionalParameters>? additionalParameters = null)
    {
        try
        {
            string activityName = logActivityType switch
            {
                "PRODUCT_ACCESS"      => LogActivityTypeConstants.PRODUCT_ACCESS,
                "USER_UPDATE_INTERNAL" => LogActivityTypeConstants.USER_UPDATE_INTERNAL,
                _                     => string.Empty
            };
            string categoryName = logActivityType switch
            {
                "PRODUCT_ACCESS"      => LogActivityCategoryType.ProductAccess.ToString(),
                "USER_UPDATE_INTERNAL" => LogActivityCategoryType.CompanySetup.ToString(),
                _                     => string.Empty
            };

            LogActivity.WriteActivity(new ActivityDetails
            {
                LogActivityTypeName       = activityName,
                LogCategoryName           = categoryName,
                CorrelationId             = _defaultUserClaim.CorrelationId.ToString(),
                BooksMasterOrganizationId = fromUserLogInfo.BooksOrganizationMasterId,
                OrganizationPartyId       = fromUserLogInfo.OrganizationPartyId,
                Message                   = message,
                FromUserLoginName         = fromUserLogInfo.LoginName,
                FromUserLoginId           = fromUserLogInfo.UserId,
                FromUserFirstName         = fromUserLogInfo.FirstName,
                FromUserLastName          = fromUserLogInfo.LastName,
                FromUserRealpageId        = fromUserLogInfo.RealPageId.ToString(),
                ToUserLoginId             = toUserLogInfo.UserId,
                ToUserLoginName           = toUserLogInfo.LoginName,
                ToUserFirstName           = toUserLogInfo.FirstName,
                ToUserLastName            = toUserLogInfo.LastName,
                ToUserRealpageId          = toUserLogInfo.RealPageId.ToString(),
                AdditionalInformation     = additionalParameters
            });
        }
        catch { /* log sink must never throw */ }
    }
}