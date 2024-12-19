using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.OneSite;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using Swashbuckle.Swagger.Annotations;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product
{
    /// <summary>
    /// Controller for all product management related APIs
    /// </summary>
    public class ProductOneSiteController : BaseApiController
    {
        IDictionary<object, object> globals = new Dictionary<object, object>();
	    private IManageProductOneSite _manageProductOneSite;
        private IManageOrganization _manageOrganization;

        //Persona _userPersona;

        /// <summary>
        /// Used to initialize the base class before the controller to get the users persona
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            //When API is NOT called from test class
            _manageOrganization = new ManageOrganization(_userClaims);
            _manageProductOneSite = new ManageProductOneSite(base._userClaims);
            //ManagePersona mp = new ManagePersona();
            //if (base._EnterpriseUserId != 0)
            //{
            //    _userPersona = mp.GetActivePersona(_realpageUserId);
            //}
        }

        #region Public Methods

        /// <summary>
        /// Used to get a list of properties for the given OneSite user
        /// </summary>
        /// <remarks>A datafilter can be used to filter the properties using sitename, cityname, statename, sitezip, sitephone or excludeassigned</remarks>
        /// <param name="editorPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="userPersonaId"></param>
        /// <param name="assignedOnly"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given user", Type = typeof(ProductProperty))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertyExample))]
        [Route("products/onesite/user/properties")]
		[AuthorizeScope("rplandingapi", "userinfoapi")]
		[Authorize]
        [HttpGet]
        public ListResponse GetOneSitePropertyList(long editorPersonaId, long userPersonaId, bool assignedOnly, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ListResponse response = new ListResponse();
            response = _manageProductOneSite.GetOneSitePropertyList(editorPersonaId, userPersonaId, assignedOnly, datafilter);
            return response;
        }

        /// <summary>
        /// Used to get a list of users for the given property
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using username or excludeassigned (0/1)</remarks>
        ///<param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <param name="propertyId">The role to get the list of users for</param>
        /// <param name="assignedOnly">only return assigned users</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of users for the given role", Type = typeof(ProductUserProperitiesRoles))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductUserProperitiesRoles), typeof(ProductUser.UserExample))]
        [Route("products/onesite/property/users")]
        [Authorize]
        [HttpGet]
        public ListResponse GetOneSitePropertyUsersList(long editorPersonaId, [FromUri]RequestParameter datafilter, int propertyId, bool assignedOnly = false)
        {
            ListResponse response = _manageProductOneSite.GetUsersForProperty(editorPersonaId, propertyId, assignedOnly, datafilter);
            return response;
        }

        /// <summary>
        /// Used to update the list of properties for the given OneSite user
        /// </summary>
        /// <param name="editorPersonaId">The person of the person making the changes to the user</param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="propertyList">The list of property ids to assign to the user. </param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successfully updated information", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "The required content was missing in the request")]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/user/properties")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateOneSiteUserProperties(long editorPersonaId, long userPersonaId, List<string> propertyList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            if (propertyList.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No Data");
            }
            string result = _manageProductOneSite.UpdatePropertiesForUser(editorPersonaId, userPersonaId, propertyList, out List<AdditionalParameters> additionalParameters);
            if (!string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK, result.ToString() + " Records Updated");
            }
            return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");
        }

        /// <summary>
        /// Used to get a list of users for the given role
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using username or excludeassigned (0/1)</remarks>
        /// <param name="editorPersonaId">The id of the user making the changes</param>
        /// <param name="datafilter"></param>
        /// <param name="roleId">The role to get the list of users for</param>
        /// <param name="assignedOnly">only return assigned users</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of users for the given role", Type = typeof(Component.SharedObjects.Product.ProductUser))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(Component.SharedObjects.Product.ProductUser), typeof(Component.SharedObjects.Product.ProductUser.UserExample))]
        [Route("products/onesite/role/users")]
        [Authorize]
        [HttpGet]
        public ListResponse GetOneSiteRoleUsersList(long editorPersonaId, [FromUri]RequestParameter datafilter, int roleId, bool assignedOnly = false)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            ListResponse response = _manageProductOneSite.GetUsersForRole(editorPersonaId, roleId, assignedOnly, datafilter);
            return response;
        }

        /// <summary>
        /// Used to get a list of roles for the given OneSite user
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using rolename, roletype (1=Internal, 0=Custom) or excludeassigned (0/1)</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="assignedOnly"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given user", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(ProductRole.RoleExample))]
        [Route("products/onesite/user/roles")]
        [Authorize]
        [HttpGet]
        public ListResponse GetOneSiteRoleList(long editorPersonaId, long userPersonaId, bool assignedOnly, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ListResponse response = new ListResponse();
            if (userPersonaId > 0)
            {
                response = _manageProductOneSite.GetOneSiteRoleList(editorPersonaId, userPersonaId, assignedOnly, datafilter);
            }
            else
            {
                response = _manageProductOneSite.GetOneSiteRoleListAll(editorPersonaId, datafilter);
            }
            return response;
        }

        /// <summary>
        /// Used to update the list of roles for the given OneSite user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="roleList">THe list of role ids to add to the given user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/user/roles")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateOneSiteUserRoles(long editorPersonaId, long userPersonaId, List<string> roleList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            if (roleList.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No Data");
            }
            string result = _manageProductOneSite.UpdateRolesForUser(editorPersonaId, userPersonaId, roleList, out List<AdditionalParameters> additionalParameters);
            if (!string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK, result.ToString() + " Records Updated");
            }
            return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");
        }

        /// <summary>
        /// Used to get a list of roles for the PMC or the given user if provided
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using rolename, roletype (1=Internal, 0=Custom) or excludeassigned (0/1)</remarks>
        /// <param name="editorPersonaId"></param>
        /// /// <param name="upfmId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(ProductRole.RoleExample))]
        [Route("products/onesite/role")]
        [Authorize]
        [HttpGet]
        public ListResponse GetOneSiteRoleListAll(long editorPersonaId, [FromUri]RequestParameter datafilter, Guid? upfmId = null)
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (editorPersonaId == 0)
            {
                if (currentClaimPrincipal.HasClaim("scope", "internalapi") && _userClaims.PersonaId == 0)
                {
                    if (!string.IsNullOrEmpty(upfmId.ToString()))
                    {
                        Guid AdminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
                        if (AdminCreatorRealPageId == Guid.Empty)
                        {
                            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                            errorResponse.Errors.Add(new Error { Title = "Error", Source = "/product", Detail = "Invalid UPFMId.", StatusCode = "" });
                        }
                        RecreateClaimsForClient(AdminCreatorRealPageId);
                        editorPersonaId = _userClaims.PersonaId;
                        _manageProductOneSite = new ManageProductOneSite(_userClaims);
                    }
                }
            }
            ListResponse response = _manageProductOneSite.GetOneSiteRoleListAll(editorPersonaId, datafilter);
            return response;
        }

        /// <summary>
        /// Used to update the rights assigned to the given custom role
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleId">The role being deleted</param>
        /// <param name="rightsToAddRemove">A list of rights to add/remove from the role</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/role/rights")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateRoleRights(long editorPersonaId, int roleId, RightsAddRemoveList rightsToAddRemove)
        {
            if (editorPersonaId == 0 || roleId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            
            if (rightsToAddRemove.RightsToAdd == null && rightsToAddRemove.RightsToDelete == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No Data");
            }
            string result = _manageProductOneSite.UpdateRoleToRights(editorPersonaId, roleId, rightsToAddRemove.RightsToAdd, rightsToAddRemove.RightsToDelete);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                if (result.ToUpper() == "ROLE NOT FOUND")
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, result);
                }
            }
        }

        /// <summary>
        /// Used to update the status of a given user
        /// </summary>
        /// <param name="editorPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="userPersonaId"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/user/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateOneSiteUserStatus(long editorPersonaId, long userPersonaId, bool active)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            string result = _manageProductOneSite.EnableOneSiteUser(editorPersonaId, userPersonaId, active);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to create a new OneSite account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/user")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage CreateOneSiteUser(long editorPersonaId, long userPersonaId, OneSiteRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            string result = _manageProductOneSite.ManageOneSiteUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, out List<AdditionalParameters> additionalParameters);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to update a OneSite account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/user")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateOneSiteUser(long editorPersonaId, long userPersonaId, OneSiteRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            if (rolepropList == null)
            {
                rolepropList = new OneSiteRoleAndPropertyList();
            }
            string result = _manageProductOneSite.ManageOneSiteUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, out List<AdditionalParameters> additionalParameters);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to delete the given OneSite User
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the OneSite login to update</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/user")]
        [Authorize]
        [HttpDelete]
        public HttpResponseMessage DeleteOneSiteUser(long editorPersonaId, long userPersonaId)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            string result = _manageProductOneSite.DeleteOneSiteUser(editorPersonaId, userPersonaId);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to get the PMC URL for the given user
        /// </summary>
        /// <param name="userPersonaId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "The PMC info for the given user", Type = typeof(PMCInfo))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponseExamples(typeof(PMCInfo), typeof(PMCInfoExample))]
        [Route("products/onesite/pmcurl")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetPMCURL(long userPersonaId)
        {
            if (userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            PMCInfo pmc = _manageProductOneSite.GetPMCURL(userPersonaId);
            if (string.IsNullOrEmpty(pmc?.PMCURL) || pmc == null)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            return Request.CreateResponse(HttpStatusCode.OK, pmc);
        }
        #endregion

        /// <summary>
        /// Used to update the roles assigned to a given right
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="rightId">The right being assigned</param>
        /// <param name="roleList">A list of role ids to update</param>
        /// <param name="assignStatus">Is the right being added or removed</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/right/roles")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateOneSiteRolesWithRight(long editorPersonaId, int rightId, List<string> roleList, bool assignStatus)
        {
            if (rightId == 0 || editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            if (roleList.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No Data");
            }
            string result = _manageProductOneSite.UpdateRightToRoles(editorPersonaId, rightId, roleList, assignStatus);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Roles Updated");
            }
            return Request.CreateResponse(HttpStatusCode.NoContent, "No Data");
        }

        /// <summary>
        /// Used to get a list of roles assigned to the right
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using rolename, roletype (1=Internal, 0=Custom) or excludeassigned (0/1)</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <param name="rightId"></param>
        /// <param name="assignedOnly"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(ProductRole.RoleExample))]
        [Route("products/onesite/right/roles")]
        [Authorize]
        [HttpGet]
        public ListResponse GetRolesForRight(long editorPersonaId, [FromUri]RequestParameter datafilter, int rightId, bool assignedOnly)
        {
            ListResponse response = _manageProductOneSite.GetRolesForRight(editorPersonaId, rightId, assignedOnly, datafilter);
            return response;
        }

        /// <summary>
        /// Used to get a list of centers available for the PMC to filter rights
        /// </summary>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(List<String>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(List<String>), typeof(RightCenterExample))]
        [Route("products/onesite/right/center")]
        [Authorize]
        [HttpGet]
        public ListResponse GetOneSiteRightCenters(long editorPersonaId)
        {
            ListResponse response = _manageProductOneSite.GetOneSiteRightsCenters(editorPersonaId);
            return response;
        }

        /// <summary>
        /// Used to get a list of rights
        /// </summary>
        /// <remarks>A datafilter can be used to filter the rights using RightDescription or CenterName</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter">A datafilter used to filter the rights. RightDescription or CenterName can be used</param>
        /// <param name="roleId">If passed, return if the rights are assigned to the given role or not</param>
        /// <param name="assignedToRoleOnly">Only return rights assigned to the given role id, if given</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(ProductRight))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductRight), typeof(ProductRight.RightExample))]
        [Route("products/onesite/rights")]
        [Authorize]
        [HttpGet]
        public ListResponse GetOneSiteRights(long editorPersonaId, [FromUri]RequestParameter datafilter, int roleId = 0, bool assignedToRoleOnly = false, Guid? upfmId = null)
        {
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (editorPersonaId == 0)
            {
                if (currentClaimPrincipal.HasClaim("scope", "internalapi") && _userClaims.PersonaId == 0)
                {
                    if (!string.IsNullOrEmpty(upfmId.ToString()))
                    {
                        Guid AdminCreatorRealPageId = _manageOrganization.GetOrganizationAdminUserRealPageId(upfmId ?? default(Guid));
                        if (AdminCreatorRealPageId == Guid.Empty)
                        {
                            var errorResponse = new ErrorResponse { Errors = new List<Error>() };
                            errorResponse.Errors.Add(new Error { Title = "Error", Source = "/product", Detail = "Invalid UPFMId.", StatusCode = "" });
                        }
                        RecreateClaimsForClient(AdminCreatorRealPageId);
                        editorPersonaId = _userClaims.PersonaId;
                        _manageProductOneSite = new ManageProductOneSite(_userClaims);
                    }
                }
            }
            ListResponse response = _manageProductOneSite.GetOneSiteRights(editorPersonaId, datafilter, roleId, assignedToRoleOnly);
            return response;
        }

        /// <summary>
        /// Used to add a new custom role
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleName">The name of the role</param>
        /// <param name="inheritRoleId">The id of the role this role was created from</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(ListResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponseExamples(typeof(ListResponse), typeof(ResponseExample))]
        [Route("products/onesite/role")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage AddRole(long editorPersonaId, string roleName, string inheritRoleId = null)
        {
            ListResponse response = _manageProductOneSite.AddUpdateRole(editorPersonaId, 0, roleName, inheritRoleId);
            if (!string.IsNullOrEmpty(response.ErrorReason))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }
        /// <summary>
        /// Used to update a new custom role
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleId">The role being deleted</param>
        /// <param name="roleName">The name of the role</param>
        /// <param name="inheritRoleId">The id of the role this role was created from</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(ListResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponseExamples(typeof(ListResponse), typeof(ResponseExample))]
        [Route("products/onesite/role")]
        [Authorize]
        [HttpPut]
        public ListResponse UpdateRole(long editorPersonaId, int roleId, string roleName, string inheritRoleId = null)
        {
            ListResponse response = _manageProductOneSite.AddUpdateRole(editorPersonaId, roleId, roleName, inheritRoleId);
            return response;
        }

        /// <summary>
        /// Used to delete a custom role
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleId">The role being deleted</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given PMC", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/role")]
        [Authorize]
        [HttpDelete]
        public HttpResponseMessage DeleteRole(long editorPersonaId, int roleId)
        {
            string result = _manageProductOneSite.DeleteRole(editorPersonaId, roleId);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                if (result.ToUpper() == "ROLE NOT FOUND")
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, result);
                }
            }
        }

        /// <summary>
        /// Used to ResetVerificationCode of OneSiteUser
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Reset Verification Code Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/resetverificationcode")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage ResetVerificationCode(long editorPersonaId, long userPersonaId)
        {
            string result = _manageProductOneSite.ResetVerificationCode(editorPersonaId, userPersonaId);
            
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            else
            {
                if (result.ToUpper() == "NO RESULT")
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, result);
                }
            }
        }

        #region User-Status

        /// <summary>
        /// Enables the one site user.
        /// </summary>
        /// <param name="produtUser">The produt user.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Enabled Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesite/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateOneSiteUserStatus(ProductUser produtUser)
        {
            if (!_manageProductOneSite.ChangeUserStatus(_personaId, produtUser.UserName))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Enabling OneSite user failed.");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Success");
        }

        #endregion

        #region Migration API
        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List OneSite users", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/onesite/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListOneSiteMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

            base._userClaims.UserRealPageGuid = persona.RealPageId;
            _manageProductOneSite = new ManageProductOneSite(base._userClaims);

            var result = _manageProductOneSite.GetMigrationUsers(editorPersonaId, datafilter);
            if(!result.IsError)
                return Request.CreateResponse(HttpStatusCode.OK, result);
            else
                return Request.CreateResponse(HttpStatusCode.Forbidden, result);
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark  OneSite users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/onesite/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            return Request.CreateResponse(HttpStatusCode.OK, _manageProductOneSite.UpdateUsersMigrationStatus(_personaId, migrateUsers));
        }
        #endregion


        #region Privates

        #endregion

        /// <summary>
        /// Used to recreate claims for client
        /// </summary>
        /// <param name="realpageUserId">RealPage UserId</param>
        private void RecreateClaimsForClient(Guid realpageUserId)
        {
            if (string.IsNullOrEmpty(realpageUserId.ToString())) return;

            var rpCache = new RPObjectCache();
            _realpageUserId = realpageUserId;

            var cacheKey = $"recreateClaimsForClient_{realpageUserId}";
            _userClaims = rpCache.GetFromCache<DefaultUserClaim>(cacheKey, 180, () =>
            {
                IManagePerson personLogic = new ManagePerson();
                Person person = personLogic.GetPerson(realpageUserId);
                if (person == null)
                {
                    throw new Exception($"Missing persona information for client_info user while Recreation of Claims For Client.  realPageId: {realpageUserId}");
                }

                IManageUserLogin userLoginLogic = new ManageUserLogin();
                IManageUserRoleRight userRoleRight = new ManageUserRoleRight();
                var userLogin = userLoginLogic.GetUserLoginOnly(realpageUserId);

                IManagePersona managePersona = new ManagePersona();
                //Active Persona is linked to one organization
                Persona persona = managePersona.GetActivePersonaWithoutRights(realpageUserId); // this user can only be under 1 company to work correctly
                var roles = userRoleRight.GetAssignedRoleForPersona(ProductEnum.UnifiedPlatform, persona.PersonaId);
                var claim = new DefaultUserClaim
                {
                    UserId = (int)userLogin.UserId,
                    OrganizationPartyId = persona.Organization.PartyId,
                    LoginName = userLogin.LoginName,
                    OrganizationMasterId = (long)persona.Organization.BooksMasterId,
                    CustomerMasterId = (long)persona.Organization.BooksMasterId,
                    OrganizationName = persona.Organization.Name,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    PersonaId = persona.PersonaId,
                    OrganizationRealPageGuid = persona.Organization.RealPageId,
                    UserRealPageGuid = _realpageUserId,
                    CorrelationId = Guid.NewGuid(),
                    RealPageEmployee = persona.Organization.RealPageId == DefaultUserClaim.EmployeeCompanyRealPageId,
                };
                ClaimsPrincipal userPrincipal = ClaimsPrincipal.Current;
                var identity = (ClaimsIdentity)userPrincipal.Identity;
                identity.AddClaims(roles.Select(r => new Claim("roleid", r.RoleID.ToString())).ToList());

                claim.Rights = BaseUserRights.GetUserRightsBy(userPrincipal, claim);
                return claim;

            });

        }

        /// <summary>
        /// Used to add/remove rights from a given role
        /// </summary>
        public class RightsAddRemoveList
        {
            /// <summary>
            /// A list of rights to add to the role
            /// </summary>
            public List<string> RightsToAdd;
            /// <summary>
            /// A list of rights to remove from the role
            /// </summary>
            public List<string> RightsToDelete;
        }

        #region Get Examples


        /// <summary>
        /// Used to document examples of the RIghtCenter Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class RightCenterExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Role example</returns>
            public object GetExamples()
            {
                IList<string> list = new List<string>();
                list.Add("Center 1");
                list.Add("Center 2");
                list.Add("Center 3");

                ListResponse output = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = 1,
                    RowsPerPage = 1000,
                    TotalPages = 1
                };
                return output;
            }
        }

        /// <summary>
        /// Used to document examples of the Response webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ResponseExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Response example</returns>
            public object GetExamples()
            {
                HttpResponseMessage example = new HttpResponseMessage(HttpStatusCode.OK);

                return example;
            }
        }

        /// <summary>
        /// Used to document examples of the Response webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class PMCInfoExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns></returns>
            public object GetExamples()
            {
                PMCInfo pmc = new PMCInfo();
                pmc.ID = 1234567;
                pmc.PMCURL = "somepmcurl.onesite.realpage.com";
                return pmc;
            }
        }
        #endregion

    }
}
