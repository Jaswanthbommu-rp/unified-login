using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace UnifiedLogin.BusinessLogic.Base
{
    /// <summary>
    /// Used to store base user right details
    /// </summary>
    public static class BaseUserRights
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userPrincipal"></param>
        /// <param name="userClaim"></param>
        /// <returns></returns>
        public static List<string> GetUserRightsBy(ClaimsPrincipal userPrincipal, DefaultUserClaim userClaim)
        {
            List<string> userRights = new List<string>();

            // Validate input parameters
            if (userPrincipal == null)
            {
                throw new ArgumentNullException(nameof(userPrincipal), "ClaimsPrincipal cannot be null");
            }

            if (userClaim.IsRPEmployee && userClaim.OrganizationRealPageGuid != DefaultUserClaim.EmployeeCompanyRealPageId)
            {
                userClaim.ImpersonatedBy = userClaim.UserRealPageGuid;
            }

            // get the users rights and add them to the claims
            var identity = (ClaimsIdentity)userPrincipal.Identity;

            //User did not Impersonate
            //User is Customer User or RP employee user logged in as Myself
            if (userClaim.ImpersonatedBy == Guid.Empty)
            {
                // get company roles
                IList<UserRoleRights> companyRoleList = GetCompanyRoles(userClaim, userClaim.OrganizationPartyId, userClaim.OrganizationRealPageGuid);

                // get user roles
                List<Claim> userRoles = identity.Claims.Where(p => p.Type.Equals("roleid", StringComparison.OrdinalIgnoreCase) || p.Type.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", StringComparison.OrdinalIgnoreCase)).ToList();
                List<long> roleIds = new List<long>();
                foreach (var item in userRoles)
                {
                    int roleId;
                    bool converted = int.TryParse(item.Value, out roleId);
                    if (converted)
                    {
                        roleIds.Add(roleId);
                    }
                }

                List<UserRoleRights> companyRoleRights = companyRoleList.Where(x => roleIds.Contains(x.RoleId)).ToList();

                foreach (var r in companyRoleRights)
                {
                    userRights.AddRange(r.UserRights.Select(x => x.RightNickName));
                }

                //If User Login as MySelf add AdGroup rights
                if (userClaim.IsRPEmployee)
                {
                    // get the impersonators details
                    ManagePersona mp = new ManagePersona();
                    Persona rpEmployeePersona = mp.ListPersona(userClaim.UserRealPageGuid).Where(c => c.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId).FirstOrDefault();

                    // RP Employee-Get ADGroup Rights for the persona
                    UserRoleRightRepository urr = new UserRoleRightRepository();
                    List<Right> adGroupRights = urr.GetADGroupRightsByPersonaId(rpEmployeePersona.PersonaId)?.ToList();
                    if (adGroupRights != null && adGroupRights.Count > 0)
                    {
                        List<string> adRights = adGroupRights.Select(x => x.RightNickName).ToList();
                        userRights.AddRange(adRights);
                    }
                }

                var distinctUserRights = userRights.Distinct().OrderBy(x => x).ToList();
                identity.AddClaims(distinctUserRights.Select(a => new Claim("right", a)).ToList());
                return distinctUserRights;
            }
            //Employee user Impersonated to Customer company
            if (userClaim.ImpersonatedBy != Guid.Empty)
            {
                // get the impersonators details
                // Get AdGroup rights for Impersonator only
                ManagePersona mp = new ManagePersona();
                Persona rpEmployeePersona = mp.ListPersona(userClaim.ImpersonatedBy).Where(c => c.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId).FirstOrDefault();

                // RP Employee-Get ADGroup Rights for the persona
                UserRoleRightRepository urr = new UserRoleRightRepository();
                List<Right> adGroupRights = urr.GetADGroupRightsByPersonaId(rpEmployeePersona.PersonaId).ToList();
                if (adGroupRights.Count > 0)
                {
                    List<string> adRights = adGroupRights.Where(m => m.IsExcludeRightFromImpersonation != true).Select(x => x.RightNickName).ToList();
                    userRights.AddRange(adRights);
                }

                // get user roles
                var productInternalSettingRepository = new ProductInternalSettingRepository();
                //var productSettingList = (List<ProductInternalSetting>)productInternalSettingRepository.GetProductInternalSettings(productId: (int)ProductEnum.UnifiedPlatform);

                var rpcache = new RPObjectCache();
                var cacheKey = $"productInternalSetting_{(int)ProductEnum.UnifiedPlatform}";
                var productSettingList = rpcache.GetFromCache(cacheKey, 120, () => productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform));

                bool IsUserManagementByADGroupEnabled = false;
                if (productSettingList.ToList().Any(s => s.Name.Equals("IsUserManagementByADGroup", StringComparison.OrdinalIgnoreCase)))
                {
                    IsUserManagementByADGroupEnabled = productSettingList.ToList().FirstOrDefault(s => s.Name.Equals("IsUserManagementByADGroup", StringComparison.OrdinalIgnoreCase)).Value.Equals("1");
                }
                if (!IsUserManagementByADGroupEnabled)
                {
                    List<Claim> userRoles = identity.Claims.Where(p => p.Type.Equals("roleid", StringComparison.OrdinalIgnoreCase) || p.Type.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", StringComparison.OrdinalIgnoreCase)).ToList();
                    List<long> roleIds = new List<long>();
                    foreach (var item in userRoles)
                    {
                        int roleId;
                        bool converted = int.TryParse(item.Value, out roleId);
                        if (converted)
                        {
                            roleIds.Add(roleId);
                        }
                    }

                    // get company roles
                    IList<UserRoleRights> companyRoleList = GetCompanyRoles(userClaim, userClaim.OrganizationPartyId, userClaim.OrganizationRealPageGuid);
                    List<UserRoleRights> companyRoleRights = companyRoleList.Where(x => roleIds.Contains(x.RoleId)).ToList();
                   
                    foreach (var r in companyRoleRights)
                    {
                        userRights.AddRange(r.UserRights.Where(m => m.IsExcludeRightFromImpersonation != true).Select(x => x.RightNickName));
                    }
                }

                var distinctUserRights = userRights.Distinct().OrderBy(x => x).ToList();

                //When User click on Impersonate User: Check if the user is not admin then remove impersonator user rights
                var userRepo = new UserRepository();
                var isUserImpersonated = userRepo.CheckOrganizationAdminUser(userClaim.UserRealPageGuid, userClaim.OrganizationPartyId);
                if (isUserImpersonated)
                {
                    List<string> impersonateUserRights = GetImpersonatedUserRightsByPersona(rpEmployeePersona, userClaim);
                    List<Right> persistRightsList = GetPersistRights();

                    //New Implementation: Rights will be carry forwarded only if employee user has it

                    foreach (var right in persistRightsList)
                    {

                        if (!distinctUserRights.Contains(right.RightName) && impersonateUserRights.Contains(right.RightName))
                        {
                            distinctUserRights.Add(right.RightName);
                        }
                    }
                }
                identity.AddClaims(distinctUserRights.Select(a => new Claim("right", a)).ToList());
                distinctUserRights = distinctUserRights.Distinct().OrderBy(x => x).ToList();
                return distinctUserRights;
            }
            return new List<string>();
        }
        /// <summary>
        /// Get Persist rights list
        /// </summary>
        /// <returns></returns>
        private static List<Right> GetPersistRights()
        {
            UserRoleRightRepository urr = new UserRoleRightRepository();
            return urr.GetPersistRights().ToList();
        }
        
        public static List<string> GetImpersonatedUserRights(Guid impersonatedBy, DefaultUserClaim userClaims)
        {
            ManagePersona mp = new ManagePersona(userClaims);
            List<string> impersonateUserRights = new List<string>();

            // get the impersonator details
            Persona impersonateUserPersona = mp.GetActivePersonaWithoutRights(impersonatedBy);

            // get impersonator company roles
            IList<UserRoleRights> impersonateCompanyRoleList = GetCompanyRoles(userClaims, impersonateUserPersona.OrganizationPartyId, impersonateUserPersona.Organization.RealPageId);

            // get impersonator user roles
            List<SharedObjects.Product.UnifiedLogin.Role> impersonateUserRoleList = GetUserRoles(impersonateUserPersona.PersonaId, impersonateUserPersona.OrganizationPartyId);
            List<long> impersonateUserRoleIds = impersonateUserRoleList.Select(c => c.RoleID).ToList();

            List<UserRoleRights> impersonatorRoleRights = impersonateCompanyRoleList.Where(x => impersonateUserRoleIds.Contains(x.RoleId)).ToList();

            foreach (var r in impersonatorRoleRights)
            {
                impersonateUserRights.AddRange(r.UserRights.Select(x => x.RightNickName));
            }

            return impersonateUserRights;
        }

        public static List<string> GetImpersonatedUserRightsByPersona(Persona impersonateUserPersona, DefaultUserClaim userClaims)
        {
            //ManagePersona mp = new ManagePersona();
            List<string> impersonateUserRights = new List<string>();

            // get impersonator company roles
            IList<UserRoleRights> impersonateCompanyRoleList = GetCompanyRoles(userClaims, impersonateUserPersona.OrganizationPartyId, impersonateUserPersona.Organization.RealPageId);

            // get impersonator user roles
            List<SharedObjects.Product.UnifiedLogin.Role> impersonateUserRoleList = GetUserRoles(impersonateUserPersona.PersonaId, impersonateUserPersona.OrganizationPartyId);
            List<long> impersonateUserRoleIds = impersonateUserRoleList.Select(c => c.RoleID).ToList();

            List<UserRoleRights> impersonatorRoleRights = impersonateCompanyRoleList.Where(x => impersonateUserRoleIds.Contains(x.RoleId)).ToList();

            foreach (var r in impersonatorRoleRights)
            {
                impersonateUserRights.AddRange(r.UserRights.Select(x => x.RightNickName));
            }

            return impersonateUserRights;
        }        

        /// <summary>
        /// Used to get the roles for the given company id
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="orgPartyId"></param>
        /// <param name="orgGuid"></param>
        private static IList<UserRoleRights> GetCompanyRoles(DefaultUserClaim userClaim, long orgPartyId, Guid orgGuid)
        {
            if (orgGuid.Equals(Guid.Empty))
            {
                return new List<UserRoleRights>();
            }
            SharedDataRepository sdr = new SharedDataRepository(userClaim);
            IList<int> productList = sdr.GetProductIdsByCompany(orgGuid);
            int productListHash = 0;
            if (productList != null)
            {
                productListHash = productList.GetHashCode();
            }

            RPObjectCache rpCache = new RPObjectCache();
            string cacheKey = $"getAllRoleRights_{orgPartyId}_{productListHash}";
            IList<UserRoleRights> userRoleRights = rpCache.GetFromCache(cacheKey, 120, () =>
            {
                UserRoleRightRepository urr = new UserRoleRightRepository();
                return urr.GetAllRoleRights(orgPartyId, productList, (int)ProductEnum.UnifiedPlatform);
            });

            return userRoleRights;
        }

        /// <summary>
        /// Used to get a users roles
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="orgPartyId"></param>
        /// <returns></returns>
        private static List<SharedObjects.Product.UnifiedLogin.Role> GetUserRoles(long personaId, long orgPartyId)
        {
            RPObjectCache rpCache = new RPObjectCache();
            string cacheKey = $"getRoleByPersona_{orgPartyId}_{personaId}";
            List<SharedObjects.Product.UnifiedLogin.Role> userRoles = rpCache.GetFromCache(cacheKey, 30, () =>
            {
                UserRoleRightRepository urr = new UserRoleRightRepository();
                return urr.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, personaId, orgPartyId);
            });
            return userRoles;
        }
    }
}