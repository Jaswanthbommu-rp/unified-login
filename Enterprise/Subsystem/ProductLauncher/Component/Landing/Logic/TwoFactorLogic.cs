using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class TwoFactorLogic : ITwoFactorLogic
    {
        private readonly ITwoFactorRepository _twoFactorRepository;
        private readonly IUserLoginRepository _userLoginRepository;
        private readonly DefaultUserClaim _userClaim;
        private readonly IPersonRepository _personRepository;
        private const string TRUSTED_DEVICE_COOKIE_PREFIX = "TrustedDevice_";

        public TwoFactorLogic(DefaultUserClaim userClaim, IRepository repository)
        {
            _twoFactorRepository = repository == null ? new TwoFactorRepository() : new TwoFactorRepository(repository);
            _userLoginRepository = repository == null ? new UserLoginRepository() : new UserLoginRepository(repository);
            _personRepository = repository == null ? new PersonRepository() : new PersonRepository(repository);

            _userClaim = userClaim;
        }

        public int DeleteUserAppAuthToken(Guid realPageId)
        {
            var userLogin = _userLoginRepository.GetUserLoginOnly(realPageId);

            if (userLogin != null)
            {
                var result = _twoFactorRepository.ResetAuthenticatorKey(userLogin.UserId, string.Empty);
                
                if (result > 0 && _userClaim != null)
                    LogDeleteActivity(realPageId, userLogin);

                return result;
            }

            return 0;
        }

        public void RemoveDeviceTrust(HttpContext context, Guid userId)
        {
            if (context == null || context.Response == null || context.Response.Cookies == null)
            {
                return;
            }

            // Build possible cookie names to handle naming format differences
            var cookieNames = new System.Collections.Generic.List<string>();
            cookieNames.Add(GetDeviceCookieName(userId));

            // Try current user's RealPageId if available
            if (_userClaim != null && _userClaim.UserRealPageGuid != Guid.Empty && _userClaim.UserRealPageGuid != userId)
            {
                cookieNames.Add(GetDeviceCookieName(_userClaim.UserRealPageGuid));
            }

            // Try internal UserId (long) if we can resolve it from the provided RealPageId
            try
            {
                var userLogin = _userLoginRepository.GetUserLoginOnly(userId);
                if (userLogin != null && userLogin.UserId > 0)
                {
                    cookieNames.Add($"{TRUSTED_DEVICE_COOKIE_PREFIX}{userLogin.UserId}");
                }
            }
            catch
            {
                // ignore resolution failures
            }

            foreach (var cookieName in cookieNames)
            {
                var existing = context.Request != null ? context.Request.Cookies[cookieName] : null;
                // Remove from request collection (server-side)
                context.Request?.Cookies.Remove(cookieName);

                // Build domain candidates: existing domain (if present), current host, and parent domain
                var domainCandidates = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(existing?.Domain))
                {
                    domainCandidates.Add(existing.Domain);
                }

                var requestHost = context.Request?.Url?.Host;
                if (!string.IsNullOrWhiteSpace(requestHost))
                {
                    // exact host
                    domainCandidates.Add(requestHost);

                    // parent domain (e.g., api.realpage.com -> realpage.com)
                    var parts = requestHost.Split('.');
                    if (parts.Length >= 2)
                    {
                        var parent = parts[parts.Length - 2] + "." + parts[parts.Length - 1];
                        domainCandidates.Add(parent);
                        domainCandidates.Add("." + parent);
                    }
                }

                // Always also try with no Domain attribute (host-only cookie)
                domainCandidates.Add(null);

                // Deduplicate
                var seen = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var domain in domainCandidates)
                {
                    var key = (domain ?? "__host_only__") + "|" + cookieName;
                    if (seen.Contains(key)) continue;
                    seen.Add(key);

                    var expiredCookie = new HttpCookie(cookieName, string.Empty)
                    {
                        Expires = DateTime.UtcNow.AddDays(-1),
                        HttpOnly = existing?.HttpOnly ?? true,
                        Secure = existing?.Secure ?? false,
                        Path = existing?.Path ?? "/"
                    };
                    if (!string.IsNullOrWhiteSpace(domain))
                    {
                        expiredCookie.Domain = domain;
                    }
                    context.Response.Cookies.Add(expiredCookie);
                }
            }
        }

        private string GetDeviceCookieName(Guid userId)
        {
            return $"{TRUSTED_DEVICE_COOKIE_PREFIX}{userId}";
        }

        public int UpdateUserTwoFactorStatus(Guid realPageId, int status)
        {
            var userLogin = _userLoginRepository.GetUserLoginOnly(realPageId);
            if (userLogin != null)
            {
                return _twoFactorRepository.UpdateUserTwoFactorStatus(userLogin.UserId, status);
            }

            return 0;
        }

        private void LogDeleteActivity(Guid realPageId, UserLoginOnly userLogin) 
        {
            var person = _personRepository.GetPerson(realPageId);
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
                    Message = $"{_userClaim.FirstName} {_userClaim.LastName} reset the multi-factor authentication setup for { person.FirstName} {person.LastName}.",

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
}
