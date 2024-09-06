using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers
{
    [RoutePrefix("shell")]
    public class ShellController : BaseApiController
    {
        private IUserRepository _userRepository;

        private IManageSecurity _manangeSecurityLogic;
        private IPersonaRepository _personaRepository;
        private ProductInternalSettingRepository _productInternalSettingRepository;
        private IOrganizationRepository _organizationRepository;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ShellController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Unit test constructor v2
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        /// <param name="userClaims"></param>
        public ShellController(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims)
        {
            var personaRightRepository = new PersonaRightRepository(repository);
                          
            _userRepository = new UserRepository(repository, userClaims, messageHandler);
            _manangeSecurityLogic = new ManageSecurity(userClaims, personaRightRepository);
            _userClaims = userClaims;
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
            _personaRepository = new PersonaRepository(repository);
            _organizationRepository = new OrganizationRepository(repository);

        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _userRepository = new UserRepository(_userClaims);
            _manangeSecurityLogic = new ManageSecurity(_userClaims);
            _personaRepository = new PersonaRepository(_userClaims);
            _productInternalSettingRepository = new ProductInternalSettingRepository(_userClaims);
            _organizationRepository = new OrganizationRepository(_userClaims);
        }

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get the side menu navigation items")]
        [Route("sidemenu")]
        [HttpGet]
        [AuthorizeScope("enterpriseapi")]
        public List<NavigationMenuTree> GetSideMenuNavigation()
        {
            var existingProducts = _organizationRepository.GetProductsByCompany(_userClaims.OrganizationRealPageGuid);
            var rights = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(_personaId, "sidemenu")?.obj?.ProductRights;
            var filterRights = rights.Join(existingProducts, r => r.ProductId, ext => ext.ProductId, (r, ext) => r.RightName).ToList();

            if (_userClaims.ImpersonatedBy != Guid.Empty)
            {
                // Pass GUID ID and Company Id  will get Persona Id Information.
                var impersonatorPersonaList = _personaRepository.ListPersona(_userClaims.ImpersonatedBy);
                var impersonatedUser = impersonatorPersonaList.FirstOrDefault(x => x.Organization.RealPageId.Equals(DefaultUserClaim.EmployeeCompanyRealPageId));
                var impersonatorRights = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(impersonatedUser.PersonaId, "sidemenu")?.obj?.ProductRights;
                var filteredImpersonatorRights = impersonatorRights.Join(existingProducts, r => r.ProductId, ext => ext.ProductId, (r, ext) => r.RightName).ToList();
                if (filteredImpersonatorRights != null)
                {
                    var productInternalSettingsByType = _productInternalSettingRepository.GetProductSettingByType("ImpersonationRightsToBeExcluded");
                    if (productInternalSettingsByType != null)
                    {
                        foreach (var productSetting in productInternalSettingsByType)
                        {
                            string[] types = productSetting.Value.Split(',');
                            foreach (string right in filteredImpersonatorRights.ToList())
                            {
                                if (types.Contains(right))
                                {
                                    filteredImpersonatorRights.Remove(right);
                                }
                            }
                        }
                    }
                }
                foreach (var impersonateRightName in filteredImpersonatorRights.ToList())
                {
                    if (!filterRights.Contains(impersonateRightName))
                    {
                        filterRights.Add(impersonateRightName);
                    }
                }
                filterRights = filterRights.Distinct().OrderBy(x => x).ToList();
            }


            var navigationMenu = _userRepository.GetNavigationMenu();
            navigationMenu = _userRepository.GetNavigationMenu();
            if (!filterRights.Contains("RealPageEmployeeUserManagement") && !filterRights.Contains("RealPageEmployeeUserManagementViewOnly") && _userClaims.OrganizationRealPageGuid.Equals(DefaultUserClaim.EmployeeCompanyRealPageId))
            {
                navigationMenu = navigationMenu.Where(a => a.PageId != "users").ToList();
            }
            if (!filterRights.Contains("ManageBestPracticeReportGroupsAD") && _userClaims.OrganizationRealPageGuid.Equals(DefaultUserClaim.EmployeeCompanyRealPageId))
            {
                navigationMenu = navigationMenu.Where(a => a.PageId != "manage reports").ToList();
            }
            var navigationMenuRights = _userRepository.GetNavigationMenuRights();
            var navigationMenuSettingAccess = _userRepository.GetNavigationMenuSettingsUnaccessable(_orgPartyId);

            var filteredMenuEntries = navigationMenu.Where(
                nmw => !navigationMenuRights.Any(w => w.NavigationMenuId == nmw.Id)
                    || navigationMenuRights.Where(w => w.NavigationMenuId == nmw.Id).Any(a => filterRights.Contains(a.RightName))
                );

            var accessibleMenuEntries = filteredMenuEntries.Where(
                fme => !navigationMenuSettingAccess.Any(n => n.NavigationMenuId == fme.Id)
                ).ToList();

            return BuildNavigationMenuTree(accessibleMenuEntries);
        }
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