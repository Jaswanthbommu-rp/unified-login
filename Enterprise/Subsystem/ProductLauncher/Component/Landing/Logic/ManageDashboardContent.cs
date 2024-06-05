using System;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Security;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using ZiggyCreatures.Caching.Fusion;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
	/// <summary>
	/// 
	/// </summary>	
	public class ManageDashboardContent : IManageDashboardContent
	{
        #region Private Variables
        IManageProfile _manageProfile;
        IManageProduct _manageProduct;
        IManageSecurity _manangeSecurityLogic;
		DefaultUserClaim _defaultUserClaim;

		#endregion

		#region Constructors
		/// <summary>
		/// Used for dependency injection
		/// </summary>
		public ManageDashboardContent(DefaultUserClaim defaultUserClaim, IManageProfile manageProfile, IManageProduct manageProduct, IManageSecurity manangeSecurityLogic)
        {
            _manageProfile = manageProfile;
            _manageProduct = manageProduct;
            _manangeSecurityLogic = manangeSecurityLogic;
            _defaultUserClaim = defaultUserClaim;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ManageDashboardContent(DefaultUserClaim defaultUserClaim, IFusionCache cache)
        {
	        _defaultUserClaim = defaultUserClaim;
			_manageProfile = new ManageProfile(defaultUserClaim, cache);
            _manageProduct = new ManageProduct(defaultUserClaim, cache: cache);
            _manangeSecurityLogic = new ManageSecurity(defaultUserClaim, cache);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>	
        public DashboardElementResponse GetDashboardElementResponse(Guid realPageId, Persona persona)
		{
            if (realPageId == null || realPageId == Guid.Empty)
            {
                throw new ArgumentNullException(nameof(realPageId), "Null realPageId.");
            }

            if (persona == null)
            {
                throw new ArgumentNullException(nameof(realPageId), "Null persona.");
            }

            var dashboardElementResponse = new DashboardElementResponse();
			dashboardElementResponse.DashboardElements = new DashboardElements();

            ObjectOutput<RouteSecurity, IErrorData> routeSecurity = _manangeSecurityLogic.GetPersonaRightsAndActionsByRoute(persona.PersonaId, "dashboard");
            RouteSecurity security = routeSecurity.obj;

            dashboardElementResponse.DashboardElements
                .ProfileDetail = _manageProfile.GetProfileDetail(realPageId, persona.OrganizationPartyId);

            dashboardElementResponse
                .DashboardElements
                .ProfileDetail
                .AssignedProducts = _manageProduct.GetUserAssignedProductsByPersona(persona: persona, productSelectType: null, security: security).OrderByDescending(p => p.IsFavorite).ThenBy(p => p.ProductName).ToList();

            dashboardElementResponse
                .DashboardElements
                .ProfileDetail
                .SummaryCount
                .TotalAssignedProducts = dashboardElementResponse.DashboardElements.ProfileDetail.AssignedProducts.Count();

            //TODO: Waiting for Master Data Management (black book) integration
            dashboardElementResponse
                .DashboardElements
                .ProfileDetail
                .SummaryCount
                .TotalAssignedProperties = 0;

            //TODO: Waiting for implementation of products and roles
            dashboardElementResponse
                .DashboardElements
                .ProfileDetail
                .SummaryCount
                .TotalAssignedRoles = 0;

            dashboardElementResponse
                .DashboardElements
                .Resources = _manageProduct.GetUserAssignedProductsByPersona(persona: persona, productSelectType: ProductSelectType.ResourcesOnly, security: security);
            
            return dashboardElementResponse;
		}
	}
}