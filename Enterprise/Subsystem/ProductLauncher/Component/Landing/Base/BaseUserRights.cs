using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base
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

			// get the users rights and add them to the claims
			var identity = (ClaimsIdentity)userPrincipal.Identity;

			// get company roles
			IList<UserRoleRights> companyRoleList = GetCompanyRoles(userClaim, userClaim.OrganizationPartyId, userClaim.OrganizationRealPageGuid);

			// get user roles
			List<Claim> userRoles = identity.Claims.Where(p => p.Type.Equals("role", StringComparison.OrdinalIgnoreCase) || p.Type.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", StringComparison.OrdinalIgnoreCase)).ToList();

			foreach (var userRoleClaim in userRoles)
			{
				foreach (var companyRole in companyRoleList)
				{
					if (!string.IsNullOrEmpty(companyRole.Role) && companyRole.Role.Equals(userRoleClaim.Value, StringComparison.OrdinalIgnoreCase))
					{
						userRights.AddRange(GetRights(companyRoleList, companyRole.RoleId, userClaim.PersonaId, userClaim.OrganizationPartyId));
						identity.AddClaims(userRights.Select(a => new Claim("right", a)).ToList());
						break;
					}
				}
			}

			if (userClaim.ImpersonatedBy != Guid.Empty)
			{
				// get the impersonators details
				ManagePersona mp = new ManagePersona();
				Persona impersonateUserPersona = mp.GetActivePersonaWithoutRights(userClaim.ImpersonatedBy); // safe to use because we just came from it

				// get impersonator company roles
				IList<UserRoleRights> impersonateCompanyRoleList = GetCompanyRoles(userClaim, impersonateUserPersona.OrganizationPartyId, impersonateUserPersona.Organization.RealPageId);

				// get impersonator user roles
				List<Component.SharedObjects.Product.UserManagement.Role> impersonateUserRoleList = GetUserRoles(impersonateUserPersona.PersonaId, impersonateUserPersona.OrganizationPartyId);
				List<long> impersonateUserRoles = new List<long>();
				foreach (Component.SharedObjects.Product.UserManagement.Role role in impersonateUserRoleList)
				{
					impersonateUserRoles.Add(role.RoleID);
				}

                foreach (long roleId in impersonateUserRoles)
                {
                    List<string> impersonateUserRights = GetRights(impersonateCompanyRoleList, roleId, impersonateUserPersona.PersonaId, impersonateUserPersona.OrganizationPartyId);
                    // check for impersonator right
                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "ACCESSTOUNIFIEDPLATFORM");

                    // check for view only access
                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "VIEWONLYSUPPORTTOOLACCESS");

                    // check for unified settings rights
                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "VIEWUNIFIEDSETTINGS");
                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "MANAGEUNIFIEDSETTINGS");

                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "MANAGECUSTOMFIELDS");
                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "MANAGEPLATFORMSECURITY");
                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "MANAGESETTINGSTEMPLATES");
                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "ACCESSSETTINGSADMIN");

                    // check for import user right
                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "ABILITYTOIMPORTUSERS");

                    AddRightFromImpersonator(identity, impersonateUserRights, userRights, "MANAGENOTIFICATIONS");

					AddRemoveRightForCIMPL(identity, impersonateUserRights, userRights, "CIMPLManagePII");
					AddRemoveRightForCIMPL(identity, impersonateUserRights, userRights, "CIMPLManageSensitiveFinancialData");

                    if (!impersonateUserRights.Contains("MigrationTool"))
                    {
						userRights.Remove("MigrationTool");
					}
				}
            }

			return userRights;

		}

		/// <summary>
		/// Used to assign a right from one user to another. If a right needs to be inherited from a support user into an impersonated user.
		/// </summary>
		/// <param name="identity">The current claims of the user being logged in</param>
		/// <param name="roleRights">The list of rights of the impersonator</param>
		/// <param name="userRights">The list of rights assigned to this user</param>
		/// <param name="rightName">The right to check</param>
		private static void AddRightFromImpersonator(ClaimsIdentity identity, List<string> roleRights, List<string> userRights, string rightName)
		{
			if (!identity.Claims.Any(q => q.Type.Equals("Right", StringComparison.OrdinalIgnoreCase) && q.Value.Equals(rightName, StringComparison.OrdinalIgnoreCase)))
			{
				// the user being logged into doesn't have the right, so see if the impersonator does
				if (roleRights.Any(p => p.Equals(rightName, StringComparison.OrdinalIgnoreCase)))
				{
					string right = roleRights.FirstOrDefault(p => p.Equals(rightName, StringComparison.OrdinalIgnoreCase));
					identity.AddClaim(new Claim("right", right));
					if (!userRights.Contains(right))
					{
						userRights.Add(right);
					}
				}
			}
		}

		/// <summary>
		/// Used to assign or unassign a CIMPL related right for a user
		/// CIMPLMANAGEPII
		///	CIMPLMANAGESENSITIVEFINANCIALDATA
		///	Case 1: If both rights are in impersonateUserRights but not in userrights then AddRight From ImpersonatedRights
		///	Case 2: If both rights are not in impersonateUserRights then remove from userRights if user is Impersonated
		///	Realpage QA		CF realpage
		///	Has Right		Has No Right	Add Right
		///	Has No Right	Has Right		Remove Right
		/// </summary>
		/// <param name="identity">The current claims of the user being logged in</param>
		/// <param name="impersonateUserRights">The list of rights of the impersonator</param>
		/// <param name="userRights">The list of rights assigned to this user</param>
		/// <param name="rightName">The right to check</param>
		private static void AddRemoveRightForCIMPL(ClaimsIdentity identity, List<string> impersonateUserRights, List<string> userRights, string rightName)
		{
			if (impersonateUserRights.Contains(rightName) && !userRights.Contains(rightName))
			{
				AddRightFromImpersonator(identity, impersonateUserRights, userRights, rightName);
			}
			if (!impersonateUserRights.Contains(rightName) && userRights.Contains(rightName))
			{
				if (identity.Claims.Any(q => q.Type.Equals("Right", StringComparison.OrdinalIgnoreCase) && q.Value.Equals(rightName, StringComparison.OrdinalIgnoreCase)))
				{
					var claim = (from c in identity.Claims where c.Value.ToUpper() == rightName.ToUpper() select c).Single();
					identity.RemoveClaim(claim);
					userRights.Remove(rightName);
				}
			}
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
		private static List<Component.SharedObjects.Product.UserManagement.Role> GetUserRoles(long personaId, long orgPartyId)
		{
			RPObjectCache rpCache = new RPObjectCache();
			string cacheKey = $"getRoleByPersona_{orgPartyId}_{personaId}";
			List<Component.SharedObjects.Product.UserManagement.Role> userRoles = rpCache.GetFromCache(cacheKey, 30, () =>
			{
				UserRoleRightRepository urr = new UserRoleRightRepository();
				return urr.ListRoleByPersona((int)ProductEnum.UnifiedPlatform, personaId, orgPartyId);
			});
			return userRoles;
		}

		/// <summary>
		/// The roles assigned to the user
		/// </summary>
		/// <param name="rpCache"></param>
		/// <param name="companyRoles"></param>
		/// <param name="roleId">The role to get rights for</param>
		/// <param name="personaId"></param>
		/// <param name="orgPartyId"></param>
		/// <returns></returns>
		private static List<string> GetRights(IList<UserRoleRights> companyRoles, long roleId, long personaId, long orgPartyId)
		{
			List<string> userRights = new List<string>();

			if (companyRoles.Any(r => r.RoleId == roleId))
			{
				foreach (Right right in companyRoles.FirstOrDefault(r => r.RoleId == roleId).UserRights)
				{
					if (!string.IsNullOrWhiteSpace(right.RightNickName) && !string.IsNullOrWhiteSpace(right.RightNickName.Trim()) && !userRights.Contains(right.RightNickName))
					{
						userRights.Add(right.RightNickName);
					}
				}
			}

			return userRights;
		}
	}

}