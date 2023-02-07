using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Controller for all product management related APIs
	/// </summary>
	public class ProductRentersInsuranceController : BaseApiController
	{
		//Persona _userPersona;

		/// <summary>
		/// Used to initialize the base class before the controller to get the users persona
		/// </summary>
		/// <param name="controllerContext"></param>
		protected override void Initialize(HttpControllerContext controllerContext)
		{
			base.Initialize(controllerContext);
			//When API is NOT called from test class
			//ManagePersona mp = new ManagePersona();
			//if (base._EnterpriseUserId != 0)
			//{
			//	_userPersona = mp.GetActivePersona(_realpageUserId);
			//}
		}

		#region Public Methods
		/// <summary>
		/// Returns Properties  
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List properties", Type = typeof(ListResponse))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/rentersinsurance/properties")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage ListProperties(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
			{
				return Request.CreateResponse(HttpStatusCode.OK, "editorPersonaId not supplied.");
			}

			if ((_realpageUserId == Guid.Empty) || (_realpageUserId == null))
			{
				return Request.CreateResponse(HttpStatusCode.OK, "RealPageId empty.");
			}

			ListResponse listResponse = new ListResponse();
			ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(base._userClaims);
			listResponse = manageProductRentersInsurance.ListProperties(editorPersonaId, userPersonaId, datafilter);

			return Request.CreateResponse(HttpStatusCode.OK, listResponse);
		}

		/// <summary>
		/// Returns Roles  
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List roles", Type = typeof(ProductRole))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/rentersinsurance/roles")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage ListRoles(long editorPersonaId, long userPersonaId)
		{
			ObjectListOutput<ProductRole, IErrorData> output = new ObjectListOutput<ProductRole, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.Status = errorStatus;

			if (editorPersonaId == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductRentersInsurance.ListRoles.1";
				errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if ((_realpageUserId == Guid.Empty) || (_realpageUserId == null))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductRentersInsurance.ListRoles.2";
				errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			ListResponse listResponse = new ListResponse();
			ManageProductRentersInsurance manageProductRentersInsurance = new ManageProductRentersInsurance(base._userClaims);
			IList<ProductRole> productRoleList = manageProductRentersInsurance.ListRoles(editorPersonaId, userPersonaId);
			if (productRoleList == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductRentersInsurance.ListRoles.3";
				errorStatus.ErrorMsg = "Product Renters Insurance - List Roles: No data";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			output.list = productRoleList;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}
        #endregion

        #region Migration API
        /// <summary>
        /// Returns product users of an organization for given user.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List Marketing Center users", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/rentersinsurance/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListRentersInsuranceMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

			base._userClaims.UserRealPageGuid = persona.RealPageId;
			var manageProductMarketingCenter = new ManageProductRentersInsurance(base._userClaims);

            var result = manageProductMarketingCenter.GetMigrationUsers(editorPersonaId, datafilter);
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
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark Renters Insurance users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/rentersinsurance/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            var manageProductMarketingCenter = new ManageProductRentersInsurance(base._userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, manageProductMarketingCenter.UpdateUsersMigrationStatus(_personaId, migrateUsers));
        }

        #region User-Status
        /// <summary>
        /// Disables the Renters Insurance user.
        /// </summary>
        /// <param name="productUser">The product user.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Disabled Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [Route("products/rentersinsurance/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateRentersInsuranceUserStatus(ProductUser productUser)
        {
            var manageProductRentersInsurance = new ManageProductRentersInsurance(base._userClaims);
            if (!manageProductRentersInsurance.ChangeUserStatus(_personaId, productUser.UserId))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Disabling Renters Insurance user failed.");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");
        }

        #endregion

        #endregion
    }
}