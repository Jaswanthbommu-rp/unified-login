using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Security;
using UnifiedLogin.DataAccess;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Rum;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
    /// <summary>
    /// Shell Controller
    /// </summary>
    [Authorize]
    [ApiController]
    [ApiVersion("2")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ShellController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IManageSecurity _manangeSecurityLogic;
        private readonly IPersonaRepository _personaRepository;
        private readonly IProductInternalSettingRepository _productInternalSettingRepository;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IUserClaimsAccessor _userClaimsAccessor;

        /// <summary>
        /// Constructor with dependency injection for shell controller.
        /// Follows modern ASP.NET Core patterns for testable, maintainable code.
        /// All dependencies are injected as interfaces for loose coupling and testability.
        /// </summary>
        /// <param name="userRepository">User repository for user-related operations</param>
        /// <param name="manageSecurity">Security management service</param>
        /// <param name="personaRepository">Persona repository for persona-related operations</param>
        /// <param name="productInternalSettingRepository">Product settings repository</param>
        /// <param name="organizationRepository">Organization repository for company-related operations</param>
        /// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
        public ShellController(
            IUserRepository userRepository,
            IManageSecurity manageSecurity,
            IPersonaRepository personaRepository,
            IProductInternalSettingRepository productInternalSettingRepository,
            IOrganizationRepository organizationRepository,
            IUserClaimsAccessor userClaimsAccessor)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _manangeSecurityLogic = manageSecurity ?? throw new ArgumentNullException(nameof(manageSecurity));
            _personaRepository = personaRepository ?? throw new ArgumentNullException(nameof(personaRepository));
            _productInternalSettingRepository = productInternalSettingRepository ?? throw new ArgumentNullException(nameof(productInternalSettingRepository));
            _organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
            _userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
        }

        /// <summary>
        /// Get the side menu navigation items based on user rights and permissions
        /// </summary>
        /// <returns>Hierarchical navigation menu tree</returns>
        [SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse((int)HttpStatusCode.OK, Description = "Get the side menu navigation items", Type = typeof(List<NavigationMenuTree>))]
        [Route("sidemenu")]
        [HttpGet]
        [AuthorizeScope("enterpriseapi")]
        public List<NavigationMenuTree> GetSideMenuNavigation()
        {
            var existingProducts = _organizationRepository.GetProductsByCompany(_userClaimsAccessor.OrganizationRealPageGuid);
            var rights = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(_userClaimsAccessor.PersonaId, "sidemenu")?.obj?.ProductRights;

            if (rights == null || existingProducts == null)
            {
                return new List<NavigationMenuTree>();
            }

            var filterRights = rights
                .Join(existingProducts, r => r.ProductId, ext => ext.ProductId, (r, ext) => r.RightName)
                .ToList();

            // Handle impersonation scenario
            if (_userClaimsAccessor.ImpersonatedBy != Guid.Empty)
            {
                filterRights = MergeImpersonatorRights(filterRights, existingProducts);
            }

            // Get navigation menu and apply filtering
            var navigationMenu = ApplyRightsBasedFiltering(_userRepository.GetNavigationMenu(), filterRights);
            var navigationMenuRights = _userRepository.GetNavigationMenuRights();
            var navigationMenuSettingAccess = _userRepository.GetNavigationMenuSettingsUnaccessable(_userClaimsAccessor.OrganizationPartyId);

            var filteredMenuEntries = FilterMenuEntriesByRights(navigationMenu, navigationMenuRights, filterRights);
            var accessibleMenuEntries = FilterMenuEntriesBySettings(filteredMenuEntries, navigationMenuSettingAccess);

            return BuildNavigationMenuTree(accessibleMenuEntries);
        }

        /// <summary>
        /// Merges impersonator rights with the current user's rights when impersonation is active
        /// </summary>
        /// <param name="filterRights">Current user's filtered rights</param>
        /// <param name="existingProducts">Products available to the organization</param>
        /// <returns>Merged and deduplicated list of rights</returns>
        private List<string> MergeImpersonatorRights(List<string> filterRights, IList<ProductUI> existingProducts)
        {
            var isUserImpersonated = _userRepository.CheckOrganizationAdminUser(
                _userClaimsAccessor.UserRealPageGuid,
                _userClaimsAccessor.OrganizationPartyId);

            if (!isUserImpersonated)
            {
                return filterRights;
            }

            var impersonatorPersonaList = _personaRepository.ListPersona(_userClaimsAccessor.ImpersonatedBy);
            var impersonatedUser = impersonatorPersonaList
                .FirstOrDefault(x => x.Organization?.RealPageId.Equals(SharedObjects.Landing.DefaultUserClaim.EmployeeCompanyRealPageId) == true);

            if (impersonatedUser == null)
            {
                return filterRights;
            }

            var impersonatorRights = _manangeSecurityLogic
                .GetPersonaRightsAndActionsByRoute(impersonatedUser.PersonaId, "sidemenu")?.obj?.ProductRights;

            if (impersonatorRights == null)
            {
                return filterRights;
            }

            var filteredImpersonatorRights = impersonatorRights
                .Join(existingProducts, r => r.ProductId, ext => ext.ProductId, (r, ext) => r.RightName)
                .ToList();

            // Remove excluded impersonation rights
            RemoveExcludedImpersonationRights(filteredImpersonatorRights);

            // Merge rights
            foreach (var impersonateRightName in filteredImpersonatorRights)
            {
                if (!filterRights.Contains(impersonateRightName))
                {
                    filterRights.Add(impersonateRightName);
                }
            }

            return filterRights.Distinct().OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Removes impersonation rights that are configured to be excluded
        /// </summary>
        /// <param name="filteredImpersonatorRights">List of impersonator rights to filter</param>
        private void RemoveExcludedImpersonationRights(List<string> filteredImpersonatorRights)
        {
            var productInternalSettingsByType = _productInternalSettingRepository
                .GetProductSettingByType("ImpersonationRightsToBeExcluded");

            if (productInternalSettingsByType == null)
            {
                return;
            }

            foreach (var productSetting in productInternalSettingsByType)
            {
                if (string.IsNullOrEmpty(productSetting.Value))
                {
                    continue;
                }

                var excludedRights = productSetting.Value.Split(',', StringSplitOptions.RemoveEmptyEntries);
                filteredImpersonatorRights.RemoveAll(right => excludedRights.Contains(right));
            }
        }

        /// <summary>
        /// Applies rights-based filtering to the navigation menu for employee users
        /// </summary>
        /// <param name="navigationMenu">Base navigation menu</param>
        /// <param name="filterRights">User's filtered rights</param>
        /// <returns>Filtered navigation menu</returns>
        private List<NavigationMenuEntry> ApplyRightsBasedFiltering(
            IList<NavigationMenuEntry> navigationMenu,
            List<string> filterRights)
        {
            var menu = navigationMenu.ToList();

            // Filter out user management menu if user lacks required rights
            if (!HasUserManagementRights(filterRights) &&
                IsEmployeeCompany())
            {
                menu = menu.Where(a => a.PageId != "users").ToList();
            }

            // Filter out report management menu if user lacks required rights
            if (!filterRights.Contains("ManageBestPracticeReportGroupsAD") &&
                IsEmployeeCompany())
            {
                menu = menu.Where(a => a.PageId != "manage reports").ToList();
            }

            return menu;
        }

        /// <summary>
        /// Checks if the user has user management rights
        /// </summary>
        private bool HasUserManagementRights(List<string> filterRights)
        {
            return filterRights.Contains("RealPageEmployeeUserManagement") ||
                   filterRights.Contains("RealPageEmployeeUserManagementViewOnly");
        }

        /// <summary>
        /// Checks if the current organization is the employee company
        /// </summary>
        private bool IsEmployeeCompany()
        {
            return _userClaimsAccessor.OrganizationRealPageGuid.Equals(SharedObjects.Landing.DefaultUserClaim.EmployeeCompanyRealPageId);
        }

        /// <summary>
        /// Filters menu entries based on navigation menu rights
        /// </summary>
        /// <param name="navigationMenu">Navigation menu to filter</param>
        /// <param name="navigationMenuRights">Available navigation menu rights</param>
        /// <param name="filterRights">User's filtered rights</param>
        /// <returns>Filtered menu entries</returns>
        private List<NavigationMenuEntry> FilterMenuEntriesByRights(
            List<NavigationMenuEntry> navigationMenu,
            IList<NavigationMenuRightEntry> navigationMenuRights,
            List<string> filterRights)
        {
            return navigationMenu.Where(nmw =>
                !navigationMenuRights.Any(w => w.NavigationMenuId == nmw.Id) ||
                navigationMenuRights
                    .Where(w => w.NavigationMenuId == nmw.Id)
                    .Any(a => filterRights.Contains(a.RightName))
            ).ToList();
        }

        /// <summary>
        /// Filters menu entries based on navigation menu settings accessibility
        /// </summary>
        /// <param name="filteredMenuEntries">Pre-filtered menu entries</param>
        /// <param name="navigationMenuSettingAccess">Inaccessible navigation menu settings</param>
        /// <returns>Accessible menu entries</returns>
        private List<NavigationMenuEntry> FilterMenuEntriesBySettings(
            List<NavigationMenuEntry> filteredMenuEntries,
            IList<NavigationMenuSetting> navigationMenuSettingAccess)
        {
            return filteredMenuEntries
                .Where(fme => !navigationMenuSettingAccess.Any(n => n.NavigationMenuId == fme.Id))
                .ToList();
        }

        /// <summary>
        /// Builds a hierarchical navigation menu tree from flat menu entries
        /// </summary>
        /// <param name="entries">List of navigation menu entries</param>
        /// <param name="parentId">Parent ID to build tree from (null for root level)</param>
        /// <returns>Hierarchical list of navigation menu tree items, or null if empty</returns>
        private List<NavigationMenuTree> BuildNavigationMenuTree(List<NavigationMenuEntry> entries, int? parentId = null)
        {
            var ret = new List<NavigationMenuTree>();

            foreach (var entry in entries.Where(w => w.ParentId == parentId).OrderBy(o => o.OrderIndex))
            {
                ret.Add(new NavigationMenuTree()
                {
                    Title = entry.Title,
                    Icon = entry.Icon,
                    PageId = entry.PageId,
                    URL = entry.URL,
                    Origin = entry.Origin,
                    Items = BuildNavigationMenuTree(entries, entry.Id)
                });
            }

            return ret.Count > 0 ? ret : null;
        }
    }
}
