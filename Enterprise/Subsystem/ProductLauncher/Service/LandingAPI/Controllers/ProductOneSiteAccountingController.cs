using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Accounting;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject;
using System.Security.Claims;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	public class ProductOneSiteAccountingController : BaseApiController
    {
        IManageProductOneSiteAccounting _mangeProductOneSiteAccounting;
        private IManageOrganization _manageOrganization;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductOneSiteAccountingController() { }

        /// <summary>
        /// Used for unit testing
        /// </summary>
        /// <param name="mangeProductOneSiteAccounting"></param>
        public ProductOneSiteAccountingController(IManageProductOneSiteAccounting mangeProductOneSiteAccounting)
        {
            _mangeProductOneSiteAccounting = mangeProductOneSiteAccounting;
            base.Request = new HttpRequestMessage();
            base.Request.SetConfiguration(new HttpConfiguration());
        }

        /// <summary>
        /// Used to initialize the base class before the controller to get the users persona
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            _manageOrganization = new ManageOrganization(_userClaims);
            //When API is NOT called from test class
            _mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(base._userClaims);

        }

        /// <summary>
        /// Used to get a list of properties for the given user
        /// </summary>
        /// <remarks>A datafilter can be used to filter the properties using name</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given company", Type = typeof(ProductProperty))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertyExample))]
        [Route("products/onesiteaccounting/user/properties")]
		[AuthorizeScope("rplandingapi", "userinfoapi")]
		[Authorize]
        [HttpGet]
        public ListResponse GetUserProperties(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            ListResponse response = _mangeProductOneSiteAccounting.GetUserPropertiesNew(editorPersonaId, userPersonaId, datafilter);
            return response;
        }

        /// <summary>
        /// Used to get a list of properties for the given user
        /// </summary>
        /// <remarks>A datafilter can be used to filter the properties using name</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of properties for the given company", Type = typeof(ProductProperty))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductProperty.PropertyExample))]
        [Route("products/onesiteaccounting/user/companies")]
        [AuthorizeScope("rplandingapi", "userinfoapi")]
        [Authorize]
        [HttpGet]
        public ListResponse GetUserCompanies(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {            
            ListResponse response = _mangeProductOneSiteAccounting.GetUserCompanies(editorPersonaId, userPersonaId, datafilter);
            return response;
        }

        /// <summary>
        /// Used to get a list of roles for the given user
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using name</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for the given company", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(ProductRole), typeof(ProductRole.RoleExample))]
        [Route("products/onesiteaccounting/user/roles")]
        [Authorize]
        [HttpGet]
        public ListResponse GetUserRoles(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            ListResponse response = _mangeProductOneSiteAccounting.GetUserRoles(editorPersonaId, userPersonaId, datafilter);
            return response;
        }


        /// <summary>
        /// Used to update the properties assigned to the user
        /// </summary>
        /// <remarks>A datafilter can be used to filter the properties using name</remarks>
        /// <param name="editorPersonaId">The person of the person making the changes to the user</param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="propertyList">The list of property ids to assign to the user. </param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successfully updated information", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/user/properties")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUserProperties(long editorPersonaId, long userPersonaId, List<string> propertyList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            if (propertyList.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No Data");
            }
            string result = _mangeProductOneSiteAccounting.UpdatePropertiesToUser(editorPersonaId, userPersonaId, propertyList, false, out var additionalParameters);

            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Records Updated");
            }
            return Request.CreateResponse(HttpStatusCode.NoContent, result);
        }

        /// <summary>
        /// Used to update the roles assigned to the user
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using name</remarks>
        /// <param name="editorPersonaId">The person of the person making the changes to the user</param>
        /// <param name="userPersonaId">The persona to use to find the login to update</param>
        /// <param name="roleList">The list of role ids to assign to the user. </param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Successfully updated information", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/user/roles")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUserRoles(long editorPersonaId, long userPersonaId, List<string> roleList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            if (roleList.Count == 0)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No Data");
            }
            string result = _mangeProductOneSiteAccounting.UpdateRolesToUser(editorPersonaId, userPersonaId, roleList, false, out var additionalParameters);

            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK, "Records Updated");
            }
            return Request.CreateResponse(HttpStatusCode.NoContent, result);
        }

        #region User
        /// <summary>
        /// Used to create a new Accounting account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the Accounting login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.Created, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when Iiformation is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/user")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage CreateAccountingUser(long editorPersonaId, long userPersonaId, AccountingRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            if (rolepropList == null)
            {
                rolepropList = new AccountingRoleAndPropertyList();
            }

            string result = _mangeProductOneSiteAccounting.ManageAccountingUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, rolepropList.CompaniesList, rolepropList.IsAccountingAdmin, rolepropList.HasAccessToSiteSpendManagementOnly, rolepropList.HasAccessToAllCurrentFutureProperties, out List<AdditionalParameters> additionalParameters);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.Created);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to create a new Accounting account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the Accounting login to update</param>
        /// <param name="rolepropList">Used to update the list of roles and properties given to a user</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/user")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateAccountingUser(long editorPersonaId, long userPersonaId, AccountingRoleAndPropertyList rolepropList)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            if (rolepropList == null)
            {
                rolepropList = new AccountingRoleAndPropertyList();
            }

            string result = _mangeProductOneSiteAccounting.ManageAccountingUser(editorPersonaId, userPersonaId, rolepropList.RoleList, rolepropList.PropertyList, rolepropList.CompaniesList, rolepropList.IsAccountingAdmin, rolepropList.HasAccessToSiteSpendManagementOnly, rolepropList.HasAccessToAllCurrentFutureProperties, out List<AdditionalParameters> additionalParameters);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to delete an existing Accounting account for the given GreenBook user persona
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the Accounting login to delete</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Delete successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when a user doesn't exist or information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/user")]
        [Authorize]
        [HttpDelete]
        public HttpResponseMessage DeleteAccountingUser(long editorPersonaId, long userPersonaId)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            string result = _mangeProductOneSiteAccounting.DeleteAccountingUser(editorPersonaId, userPersonaId);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.NoContent);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest, result);
        }

        /// <summary>
        /// Used to update the status of a given user
        /// </summary>
        /// <param name="editorPersonaId">The persona to use to find the login to update</param>
        /// <param name="userPersonaId"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/user/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateAccountingUserStatus(long editorPersonaId, long userPersonaId, bool active)
        {
            if (editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }

            
            ManageProductOneSiteAccounting mpa = new ManageProductOneSiteAccounting(base._userClaims);
	        string result = mpa.ChangeStatusAccountingUser(editorPersonaId, userPersonaId, active);
            if (string.IsNullOrEmpty(result))
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Changing User SSO Claim Status
        /// </summary>
        /// <param name="editorPersonaId">The persona to use to find the login to update</param>
        /// <param name="userPersonaId"></param>
        /// <param name="isLinked"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/user/claimstatus")]
        [Authorize] 
        [HttpPut]
        public HttpResponseMessage UpdateAccountingUserClaimStatus(long editorPersonaId, long userPersonaId, bool isLinked)
        {
            if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            
            ManageProductOneSiteAccounting mpa = new ManageProductOneSiteAccounting(base._userClaims);
            var result = mpa.ChangeAccountingUserClaimStatus(editorPersonaId, userPersonaId, isLinked);
            if (result)
            {
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            return Request.CreateResponse(HttpStatusCode.BadRequest);
        }

        /*
        /// <summary>
        /// Used to create a new Accounting account for the given GreenBook user
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId">The persona to use to find the Accounting login to update</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.NoContent, Description = "Delete successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when a user doesn't exist or information is out of sync with the server)")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/user")]
        [Authorize]
        [HttpGet]
        public ListResponse GetAccountingUser(long editorPersonaId, long userPersonaId)
        {
            if (editorPersonaId == 0 || editorPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
            ManageProductOneSiteAccounting mg = new ManageProductOneSiteAccounting(_realpageUserId);

            ListResponse result = mg.GetAccountingUser(editorPersonaId, userPersonaId);
            return result;
        }
        */
        #endregion

        #region Migration API
        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List Accounting users", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/onesiteaccounting/migration-users")]
        [HttpGet]
        public HttpResponseMessage ListOneSiteAccountingMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

	        base._userClaims.UserRealPageGuid = persona.RealPageId;
            var mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(base._userClaims);

            var result = mangeProductOneSiteAccounting.GetMigrationUsers(editorPersonaId, datafilter);
            if (!result.IsError)
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
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark OneSite Accounting users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/onesiteaccounting/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            var mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(base._userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, mangeProductOneSiteAccounting.UpdateUsersMigrationStatus(_personaId, migrateUsers));
        }
        #endregion


        #region Roles & Rights

        /// <summary>
        /// Used to get a list of roles 
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using name</remarks>
        /// <param name="editorPersonaId"></param>        
        /// <param name="datafilter"></param>
        /// <param name="upfmId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]        
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]        
        [Route("products/onesiteaccounting/rolescount")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRolesCount(long editorPersonaId, [FromUri]RequestParameter datafilter, Guid? upfmId = null)
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
                        if (editorPersonaId == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "invalid request.");
                        }
                        _mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                    }
                }
            }
            ListResponse response = _mangeProductOneSiteAccounting.GetRolesCount(editorPersonaId, datafilter);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Used to get a list of roles 
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using name</remarks>
        /// <param name="editorPersonaId"></param>        
        /// <param name="datafilter"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]        
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]
        [Route("products/onesiteaccounting/roles")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetAllRoles(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            ListResponse response = _mangeProductOneSiteAccounting.GetAllRoles(editorPersonaId, datafilter);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }



        /// <summary>
        /// Used to get a list of rights 
        /// </summary>
        /// <remarks></remarks>
        /// <param name="editorPersonaId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of rights for the given company", Type = typeof(object))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]        
        [Route("products/onesiteaccounting/rights")]
        [Authorize]
        [HttpGet]        
        public HttpResponseMessage GetRights(long editorPersonaId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            ListResponse response = _mangeProductOneSiteAccounting.GetRights(editorPersonaId);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        /// <summary>
        /// Used to get a list of applications 
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using name</remarks>
        /// <param name="editorPersonaId"></param> 
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of apllication centers for the given company", Type = typeof(object))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when Information is out of sync with the server)")]        
        [Route("products/onesiteaccounting/applications")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetApplications(long editorPersonaId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            
            ListResponse response = _mangeProductOneSiteAccounting.GetApplications(editorPersonaId);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        /// <summary>
        /// Used to get a list of roles assigned to the right
        /// </summary>
        /// <remarks>A datafilter can be used to filter the roles using rolename, roletype (1=Internal, 0=Custom) or excludeassigned (0/1)</remarks>
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <param name="rightId"></param>
        /// <param name="assignedOnly"></param>
        /// /// <param name="right"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of roles for a given right", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]        
        [Route("products/onesiteaccounting/right/roles")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRolesForRight(long editorPersonaId, [FromUri]RequestParameter datafilter, int rightId, bool assignedOnly, [FromUri]string right)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            if (rightId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "rightId not supplied.");
            if (string.IsNullOrEmpty(right.Trim()))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "right not supplied.");

            ListResponse response = _mangeProductOneSiteAccounting.GetRolesForRight(editorPersonaId, datafilter, rightId, assignedOnly, JsonConvert.DeserializeObject<ProductRightAcct>(right));
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Used to roles for a right
        /// </summary>        
        /// <param name="editorPersonaId"></param>
        /// <param name="right"></param>
        /// <param name="rightId"></param>
        /// <param name="rolesToAddRemove"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]        
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/onesiteaccounting/right/roles")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateRolesForRight(long editorPersonaId, int rightId, RolesAddRemove rolesToAddRemove, [FromUri]string right)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            if (rightId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "rightId not supplied.");
            if (rolesToAddRemove.RolesToAdd == null) { rolesToAddRemove.RolesToAdd = new List<ProductRoleAcct>(); }
            if (rolesToAddRemove.RolesToDelete == null) { rolesToAddRemove.RolesToDelete = new List<ProductRoleAcct>(); }
            if (rolesToAddRemove.RolesToAdd.Count == 0 && rolesToAddRemove.RolesToDelete.Count == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Roles not supplied to Add or Remove.");
            if (string.IsNullOrEmpty(right.Trim()))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "right not supplied.");

            ListResponse response = _mangeProductOneSiteAccounting.UpdateRolesForRight(editorPersonaId, rightId, rolesToAddRemove.RolesToAdd, rolesToAddRemove.RolesToDelete, JsonConvert.DeserializeObject<ProductRightAcct>(right));
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Used to get a list of rights assigned to the role
        /// </summary>        
        /// <param name="editorPersonaId"></param>
        /// <param name="datafilter"></param>
        /// <param name="roleId"></param>
        /// <param name="roleName"></param>
        /// <param name="upfmId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of rights for the given role", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]        
        [Route("products/onesiteaccounting/role/rights")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetRightsForRole(long editorPersonaId, [FromUri]RequestParameter datafilter, int roleId, string roleName, Guid? upfmId = null)
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
                        if (editorPersonaId == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "invalid request.");
                        }
                        _mangeProductOneSiteAccounting = new ManageProductOneSiteAccounting(_userClaims);
                    }
                }
            }
            if (roleId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");
            if (string.IsNullOrEmpty(roleName.Trim()))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleName not supplied.");
           
            ListResponse response = _mangeProductOneSiteAccounting.GetRightsForRole(editorPersonaId, datafilter, roleName, roleId );
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


        /// <summary>
        /// Used to update a list of rights assigned to the role
        /// </summary>        
        /// <param name="editorPersonaId"></param>        
        /// <param name="roleId"></param>
        /// /// <param name="roleName"></param>
        /// <param name="rightsToAddRemove"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "A list of rights for the given role", Type = typeof(ProductRole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]        
        [Route("products/onesiteaccounting/role/rights")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateRightsForRole(long editorPersonaId,int roleId, string roleName, RightsAddRemove rightsToAddRemove)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            if (roleId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");
            if (rightsToAddRemove.RightsToAdd == null){ rightsToAddRemove.RightsToAdd = new List<ProductRightAcct>(); }
            if (rightsToAddRemove.RightsToRemove == null) { rightsToAddRemove.RightsToRemove = new List<ProductRightAcct>(); }

            if (rightsToAddRemove.RightsToAdd.Count == 0 && rightsToAddRemove.RightsToRemove.Count == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Rights not supplied to Add or Remove.");
            ListResponse response = _mangeProductOneSiteAccounting.UpdateRightsForRole(editorPersonaId,  roleId, roleName, rightsToAddRemove.RightsToAdd, rightsToAddRemove.RightsToRemove);
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

       

        /// <summary>
        /// Used to add a new custom role
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleName">The name of the role</param>        
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Create a New Role", Type = typeof(ListResponse))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponseExamples(typeof(ListResponse), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/role")]
        [Authorize]
        [HttpPost]
        public HttpResponseMessage CreateRole(long editorPersonaId, string roleName)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");            
            if (string.IsNullOrEmpty(roleName.Trim()))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleName not supplied.");

            ListResponse response = _mangeProductOneSiteAccounting.CreateRole(editorPersonaId, roleName);
            if (!string.IsNullOrEmpty(response.ErrorReason))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, response);
            }
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Used to clone a custom role from existing role
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleName">The name of the new role</param>
        /// <param name="inheritedRoleName">The name of the role this role was created from</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Clone Role", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [Route("products/onesiteaccounting/role")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage CloneRole(long editorPersonaId, string inheritedRoleName, string roleName)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");            
            if (string.IsNullOrEmpty(inheritedRoleName.Trim()))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "inheritRoleName not supplied.");
            if (string.IsNullOrEmpty(roleName.Trim()))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleName not supplied.");

            ListResponse listResponse = _mangeProductOneSiteAccounting.CloneRole(editorPersonaId, roleName, inheritedRoleName);
            if (!string.IsNullOrEmpty(listResponse.ErrorReason))
                return Request.CreateResponse(HttpStatusCode.BadRequest, listResponse);
            return Request.CreateResponse(HttpStatusCode.OK, listResponse);
        }


        /// <summary>
        /// Used to delete a custom role
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="roleId">The role Id</param>
        /// <param name="roleName">The role Name</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Delete a Role", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/role")]
        [Authorize]
        [HttpDelete]
        public HttpResponseMessage DeleteRole(long editorPersonaId, long roleId, string roleName)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");
            if (roleId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");
            if (string.IsNullOrEmpty(roleName.Trim()))
                return Request.CreateResponse(HttpStatusCode.BadRequest, "roleName not supplied.");

            ListResponse listResponse = _mangeProductOneSiteAccounting.DeleteRole(editorPersonaId, roleId, roleName);
            if (!string.IsNullOrEmpty(listResponse.ErrorReason))
                return Request.CreateResponse(HttpStatusCode.BadRequest, listResponse);
            return Request.CreateResponse(HttpStatusCode.OK, listResponse);
        }

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

        #endregion



        #region Get Examples

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
        #endregion

        #region User-Status
        /// <summary>
        /// Disables the Accounting user.
        /// </summary>
        /// <param name="productUser">The product user.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Disabled Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/onesiteaccounting/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateAccountingUserStatus(ProductUser productUser)
        {
            var manageProductAccounting = new ManageProductOneSiteAccounting(base._userClaims);
            if (!manageProductAccounting.ChangeUserStatus(_personaId, productUser.UserName))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Disabling Accounting user failed.");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");
        }

        #endregion

    }
}
