using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
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


        /// <summary>
        /// Default constructor
        /// </summary>
        public ShellController()
        {
            // DONT USE USERCLAIM IN BASE, IT IS NULL AT THIS POINT. MOVE TO Initialize FUNCTION
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        /// <param name="userClaims"></param>
       // ///  <param name="_personaRepository"></param>
        public ShellController(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims)
        {
            var personaRightRepository = new PersonaRightRepository(repository);
                          
            _userRepository = new UserRepository(repository, userClaims, messageHandler);
            _manangeSecurityLogic = new ManageSecurity(userClaims, personaRightRepository);
            _userClaims = userClaims;
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
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
            IOrganizationRepository organizationRepository = new OrganizationRepository();
            IUserLoginRepository userLoginRepository = new UserLoginRepository();
            _productInternalSettingRepository = new ProductInternalSettingRepository();
            _personaRepository = new PersonaRepository(organizationRepository, userLoginRepository,_userClaims);

        }

        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get the side menu navigation items")]
        [Route("sidemenu")]
        [HttpGet]
        [AuthorizeScope("enterpriseapi")]
        public List<NavigationMenuTree> GetSideMenuNavigation()
        {
            var rights = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(_personaId, "sidemenu")?.obj?.Rights;
            if (_userClaims.ImpersonatedBy != Guid.Empty)
            {
                // Pass GUID ID and Company Id  will get Persona Id Information.
                var productSettingsInternal = GetProductSettings(3);
                var impersonaUserId = _personaRepository.ListPersona(_userClaims.ImpersonatedBy);
                var imper = impersonaUserId.FirstOrDefault(x => x.Organization.RealPageId.Equals(DefaultUserClaim.EmployeeCompanyRealPageId));
                var Impersonarights = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(imper.PersonaId, "sidemenu")?.obj?.Rights;
                if (Impersonarights != null)
                {
                    var productInternalSettingsByType = _productInternalSettingRepository.GetProductSettingByType("ImpersonationRightsToBeExcluded");
                    foreach (var productSetting in productInternalSettingsByType)
                    {
                        string[] types = productSetting.Value.Split(',');
                        foreach (string right in Impersonarights.ToList()) 
                        {
                            if (types.Contains(right))
                            {
                                Impersonarights.Remove(right);
                            }
                        } 
                    }
                  
                }
                foreach (var impersonateRightName in Impersonarights.ToList())
                {
                    if (!rights.Contains(impersonateRightName))
                    { 
                        rights.Add(impersonateRightName);
                    }
                }
                rights = rights.Distinct().OrderBy(x => x).ToList();
            }
            var navigationMenu = _userRepository.GetNavigationMenu();
            var navigationMenuRights = _userRepository.GetNavigationMenuRights();
            var navigationMenuSettingAccess = _userRepository.GetNavigationMenuSettingsUnaccessable(_orgPartyId);

            var filteredMenuEntries = navigationMenu.Where(
                nmw => !navigationMenuRights.Any(w => w.NavigationMenuId == nmw.Id)
                    || navigationMenuRights.Where(w => w.NavigationMenuId == nmw.Id).Any(a => rights.Contains(a.RightName))
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
        private IList<ProductInternalSetting> GetProductSettings(int productId)
        {
            ProductInternalSettingRepository settings = new ProductInternalSettingRepository();
           return settings.GetProductInternalSettings(productId);
        }
    }
}