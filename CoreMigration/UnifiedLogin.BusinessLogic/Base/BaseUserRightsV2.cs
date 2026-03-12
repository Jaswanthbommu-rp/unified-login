using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Services.User;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Base
{
    /// <summary>
    /// Refactored from static — now injectable via IBaseUserRights.
    /// Keeps all original logic intact; replaces direct `new` calls with injected services.
    /// </summary>
    public sealed class BaseUserRightsV2 : IBaseUserRights
    {
        private readonly IUserQueryService _userQueryService;
        private readonly IUserRoleRightRepository _userRoleRightRepository;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly ILogger<BaseUserRightsV2> _logger;

        public BaseUserRightsV2(
            IUserQueryService userQueryService,
            IUserRoleRightRepository userRoleRightRepository,
            IProductInternalSettingRepository productInternalSettingRepository,
            ILogger<BaseUserRightsV2> logger)
        {
            _userQueryService = userQueryService
                ?? throw new ArgumentNullException(nameof(userQueryService));
            _userRoleRightRepository = userRoleRightRepository
                ?? throw new ArgumentNullException(nameof(userRoleRightRepository));
            _productInternalSettingRepository = productInternalSettingRepository
                ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
            _logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        public List<string> GetUserRightsBy(ClaimsPrincipal userPrincipal, DefaultUserClaim userClaim)
        {
            ArgumentNullException.ThrowIfNull(userPrincipal);

            if (userClaim.IsRPEmployee
                && userClaim.OrganizationRealPageGuid != DefaultUserClaim.EmployeeCompanyRealPageId)
            {
                userClaim.ImpersonatedBy = userClaim.UserRealPageGuid;
            }

            var identity = (ClaimsIdentity)userPrincipal.Identity;

            // ── Not impersonating ──────────────────────────────────────────────
            if (userClaim.ImpersonatedBy == Guid.Empty)
            {
                return HandleDirectLogin(identity, userClaim);
            }

            // ── Impersonating ──────────────────────────────────────────────────
            return HandleImpersonation(identity, userClaim);
        }

        public List<string> GetImpersonatedUserRights(Guid impersonatedBy, DefaultUserClaim userClaims)
        {
            // Uses IUserQueryService instead of new ManagePersona(userClaims)
            var impersonateUserPersona = _userQueryService.GetActivePersonaWithoutRights(impersonatedBy);
            return BuildImpersonatedRights(impersonateUserPersona, userClaims);
        }

        public List<string> GetImpersonatedUserRightsByPersona(
            Persona impersonateUserPersona,
            DefaultUserClaim userClaims) =>
            BuildImpersonatedRights(impersonateUserPersona, userClaims);

        // ── Private helpers ────────────────────────────────────────────────────

        private List<string> HandleDirectLogin(
            ClaimsIdentity identity,
            DefaultUserClaim userClaim)
        {
            List<string> userRights = [];

            var companyRoles = GetCompanyRoles(userClaim, userClaim.OrganizationPartyId,
                userClaim.OrganizationRealPageGuid);
            var roleIds = ExtractRoleIds(identity);
            var matchedRoles = companyRoles.Where(x => roleIds.Contains(x.RoleId)).ToList();

            foreach (var r in matchedRoles)
                userRights.AddRange(r.UserRights.Select(x => x.RightNickName));

            // RP Employee — add ADGroup rights
            if (userClaim.IsRPEmployee)
            {
                // IUserQueryService replaces new ManagePersona()
                var rpPersona = _userQueryService
                    .ListPersona(userClaim.UserRealPageGuid)
                    .FirstOrDefault(c => c.Organization.RealPageId
                        == DefaultUserClaim.EmployeeCompanyRealPageId);

                if (rpPersona is not null)
                {
                    var adRights = _userRoleRightRepository
                        .GetADGroupRightsByPersonaId(rpPersona.PersonaId)
                        ?.Select(x => x.RightNickName) ?? [];
                    userRights.AddRange(adRights);
                }
            }

            return FinalizeRights(identity, userRights);
        }

        private List<string> HandleImpersonation(
            ClaimsIdentity identity,
            DefaultUserClaim userClaim)
        {
            List<string> userRights = [];

            // IUserQueryService replaces new ManagePersona()
            var rpEmployeePersona = _userQueryService
                .ListPersona(userClaim.ImpersonatedBy)
                .FirstOrDefault(c => c.Organization.RealPageId
                    == DefaultUserClaim.EmployeeCompanyRealPageId);

            if (rpEmployeePersona is null)
            {
                _logger.LogWarning(
                    "No RP employee persona found for impersonatedBy={ImpersonatedBy}",
                    userClaim.ImpersonatedBy);
                return [];
            }

            var adRights = _userRoleRightRepository
                .GetADGroupRightsByPersonaId(rpEmployeePersona.PersonaId)
                ?.Where(m => m.IsExcludeRightFromImpersonation != true)
                .Select(x => x.RightNickName) ?? [];
            userRights.AddRange(adRights);

            if (!IsUserManagementByADGroupEnabled())
            {
                var companyRoles = GetCompanyRoles(userClaim,
                    userClaim.OrganizationPartyId, userClaim.OrganizationRealPageGuid);
                var roleIds = ExtractRoleIds(identity);

                foreach (var r in companyRoles.Where(x => roleIds.Contains(x.RoleId)))
                    userRights.AddRange(
                        r.UserRights
                         .Where(m => m.IsExcludeRightFromImpersonation != true)
                         .Select(x => x.RightNickName));
            }

            var distinctRights = userRights.Distinct().OrderBy(x => x).ToList();

            // IUserQueryService replaces new UserRepository().CheckOrganizationAdminUser(...)
            if (_userQueryService.CheckOrganizationAdminUser(
                    userClaim.UserRealPageGuid, userClaim.OrganizationPartyId))
            {
                var impersonatorRights = BuildImpersonatedRights(rpEmployeePersona, userClaim);
                var persistRights = _userRoleRightRepository.GetPersistRights();

                foreach (var right in persistRights)
                {
                    if (!distinctRights.Contains(right.RightName)
                        && impersonatorRights.Contains(right.RightName))
                    {
                        distinctRights.Add(right.RightName);
                    }
                }
            }

            return FinalizeRights(identity, distinctRights);
        }

        private List<string> BuildImpersonatedRights(
            Persona impersonateUserPersona,
            DefaultUserClaim userClaims)
        {
            List<string> rights = [];

            var companyRoles = GetCompanyRoles(userClaims,
                impersonateUserPersona.OrganizationPartyId,
                impersonateUserPersona.Organization.RealPageId);

            var userRoles = GetUserRoles(
                impersonateUserPersona.PersonaId,
                impersonateUserPersona.OrganizationPartyId);

            var roleIds = userRoles.Select(c => c.RoleID).ToList();

            foreach (var r in companyRoles.Where(x => roleIds.Contains(x.RoleId)))
                rights.AddRange(r.UserRights.Select(x => x.RightNickName));

            return rights;
        }

        private bool IsUserManagementByADGroupEnabled()
        {
            var cacheKey = $"productInternalSetting_{(int)ProductEnum.UnifiedPlatform}";
            var cache = new RPObjectCache();

            var settings = cache.GetFromCache(cacheKey, 120,
                () => _productInternalSettingRepository
                          .GetProductInternalSettings((int)ProductEnum.UnifiedPlatform));

            return settings
                .FirstOrDefault(s => s.Name.Equals(
                    "IsUserManagementByADGroup", StringComparison.OrdinalIgnoreCase))
                ?.Value == "1";
        }

        private IList<UserRoleRights> GetCompanyRoles(
            DefaultUserClaim userClaim, long orgPartyId, Guid orgGuid)
        {
            if (orgGuid == Guid.Empty)
                return [];

            // IUserQueryService replaces new SharedDataRepository(userClaim)
            var productList = _userQueryService.GetProductIdsByCompany(orgGuid);
            int productListHash = productList?.GetHashCode() ?? 0;

            var cache = new RPObjectCache();
            string cacheKey = $"getAllRoleRights_{orgPartyId}_{productListHash}";

            return cache.GetFromCache(cacheKey, 120,
                () => _userRoleRightRepository.GetAllRoleRights(
                          orgPartyId, productList, (int)ProductEnum.UnifiedPlatform));
        }

        private List<SharedObjects.Product.UnifiedLogin.Role> GetUserRoles(
            long personaId, long orgPartyId)
        {
            var cache = new RPObjectCache();
            string cacheKey = $"getRoleByPersona_{orgPartyId}_{personaId}";

            return cache.GetFromCache(cacheKey, 30,
                () => _userRoleRightRepository.ListRoleByPersona(
                          (int)ProductEnum.UnifiedPlatform, personaId, orgPartyId));
        }

        private static List<long> ExtractRoleIds(ClaimsIdentity identity) =>
            identity.Claims
                .Where(p => p.Type.Equals("roleid", StringComparison.OrdinalIgnoreCase)
                         || p.Type.Equals(
                             "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
                             StringComparison.OrdinalIgnoreCase))
                .Select(item => int.TryParse(item.Value, out int id) ? (long?)id : null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

        private static List<string> FinalizeRights(
            ClaimsIdentity identity, IEnumerable<string> rights)
        {
            var distinct = rights.Distinct().OrderBy(x => x).ToList();
            identity.AddClaims(distinct.Select(a => new Claim("right", a)));
            return distinct;
        }
    }
}
