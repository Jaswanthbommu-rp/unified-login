using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product
{
	/// <summary>
	/// Handles APIs for all sub-products under Asset Optimization
	/// </summary>
	public class ProductAssetOptimizationController : BaseApiController
	{
		/// <summary>
		/// Returns Companies  
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param>
		/// <param name="productName">product division name in AO (BI/AX/PO/PA)</param>
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		/// <param name="userLoginName">User Login Name</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/ao/companies")]
		[HttpGet]
		public HttpResponseMessage GetCompanies(long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "")
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var manageProductAoBi = new ManageProductAssetOptimization(base._userClaims);
			var result = manageProductAoBi.GetCompanies(editorPersonaId, userPersonaId, productName, datafilter, userLoginName);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns assignable or assigned Property Groups for user
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">To user</param>
		/// <param name="selectedCompanies"></param>
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		/// <param name="productName"></param>
		/// <param name="userLoginName">User Login Name</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/ao/propertygroups")]
		[HttpGet]
		public HttpResponseMessage GetPropertyGroups(long editorPersonaId, long userPersonaId, string productName, [FromUri] IList<int> selectedCompanies, RequestParameter datafilter, string userLoginName = "")
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var manageProductAoBi = new ManageProductAssetOptimization(base._userClaims);
			var result = manageProductAoBi.GetPropertyGroups(editorPersonaId, userPersonaId, productName, selectedCompanies);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns properties in group
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">To user</param> 
		/// <param name="propertyGroupId">Property Group Id to select properties</param>
		/// <param name="datafilter">A datafilter used to filter the properties.</param> 
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/ao/groupproperties")]
		[HttpGet]
		public HttpResponseMessage GetPropertiesInGroups(long editorPersonaId, long userPersonaId, int propertyGroupId, RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var manageProductAoBi = new ManageProductAssetOptimization(base._userClaims);
			var result = manageProductAoBi.GetPropertiesInGroup(editorPersonaId, userPersonaId, propertyGroupId);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}


		/// <summary>
		/// Returns companies and associated roles for a product
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">To user</param> 
		/// <param name="productName">AO product name</param>
		/// <param name="datafilter">A datafilter used to filter.</param> 
		/// <param name="userLoginName">User Login Name</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/ao/companyroles")]
		[HttpGet]
		public HttpResponseMessage GetCompaniesWithRoles(long editorPersonaId, long userPersonaId, string productName, RequestParameter datafilter, string userLoginName = "")
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var manageProductAoBi = new ManageProductAssetOptimization(base._userClaims);
			var result = manageProductAoBi.GetCompaniesWithRoles(editorPersonaId, userPersonaId, productName, datafilter, userLoginName);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns companies and associated properties for a product
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">To user</param> 
		/// <param name="productName">AO product name</param>
		/// <param name="datafilter">A datafilter used to filter.</param> 
		/// <param name="userLoginName">User Login Name</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest,
			 Description =
				 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")
		]
		[Route("products/ao/companyproperties")]
		[HttpGet]
		public HttpResponseMessage GetCompaniesWithProperties(long editorPersonaId, long userPersonaId,
			string productName, RequestParameter datafilter, string userLoginName = "")
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var manageProductAoBi = new ManageProductAssetOptimization(base._userClaims);
			var result = manageProductAoBi.GetCompaniesWithProperties(editorPersonaId, userPersonaId, productName, datafilter, userLoginName);

			if (result.IsError)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
        /// Returns companies and associated properties for a product
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">To user</param> 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest,
             Description =
                 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")
        ]
        [Route("products/ao/operatorproperties")]
        [HttpGet]
        public HttpResponseMessage GetOperatorsWithProperties(long editorPersonaId, long userPersonaId)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var manageProductAo = new ManageProductAssetOptimization(base._userClaims);
            var result = manageProductAo.GetOperators(editorPersonaId, userPersonaId);

            if (result.IsError)
                Request.CreateResponse(HttpStatusCode.Forbidden, result);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        /// <summary>
        /// Returns companies and associated properties for a product
        /// </summary>
        /// <param name="editorPersonaId">Assign user Id</param>
        /// <param name="userPersonaId">To user</param> 
        /// <param name="operatorCode">AO operator code</param>
        /// <param name="operatorValue">AO operator value.</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest,
             Description =
                 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")
        ]
        [Route("products/ao/propertiesbyoperators")]
        [HttpGet]
        public HttpResponseMessage GetPropertiesWithOperators(long editorPersonaId, long userPersonaId,string operatorCode, string operatorValue)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var manageProductAo = new ManageProductAssetOptimization(base._userClaims);
            var result = manageProductAo.GetPropertiesWithOperators(editorPersonaId, userPersonaId, operatorCode, operatorValue);

            if (result.IsError)
                Request.CreateResponse(HttpStatusCode.Forbidden, result);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
		
        #region User-Status

        /// <summary>
        /// Deactivate the resident portal user.
        /// </summary>
        /// <param name="productUser">The produt user.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "User Deleted Successfully", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
		[Route("products/ao/user/MT/status")]
		[Authorize]
		[HttpPut]
		public HttpResponseMessage UpdateAOUserStatus(ProductUser productUser)
		{
			var manageProductAoBi = new ManageProductAssetOptimization(base._userClaims);
			if (!manageProductAoBi.ChangeUserStatus(_personaId, productUser.UserName, productUser.FirstName, productUser.LastName))
			{
				if (productUser.IsAssigned)
				{
					return Request.CreateResponse(HttpStatusCode.BadRequest, "Activate ao user failed.");
				}
				return Request.CreateResponse(HttpStatusCode.BadRequest, "Deactivate ao user failed.");
			}
			return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");
		}
		#endregion

		#region Migration API
		/// <summary>
		/// List Client portal users
		/// </summary>
		/// 
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List Asset Optimization users", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/ao/migration-users")]
		[Authorize] // Todo: Need to implement Resource Scope Based Authorization
		[HttpGet]
		public HttpResponseMessage ListAssetOptimizationMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			ManagePersona managePersona = new ManagePersona();
			var persona = managePersona.GetPersona(editorPersonaId);
			if (persona == null)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");
			base._userClaims.UserRealPageGuid = persona.RealPageId;
			var manageProductAoBi = new ManageProductAssetOptimization(base._userClaims);

            var result = manageProductAoBi.GetMigrationUsers(editorPersonaId, datafilter);
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
		[SwaggerResponse(HttpStatusCode.OK, Description = "Mark Asset Optimization users to migrated", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/ao/migrate-users")]
		[Authorize]
		[HttpPut]
		public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
		{
			var manageProductAoBi = new ManageProductAssetOptimization(base._userClaims);
			return Request.CreateResponse(HttpStatusCode.OK, manageProductAoBi.UpdateUsersMigrationStatus(_personaId, migrateUsers));
		}
		#endregion

		#region Unity Migrated Users
		/// <summary>
		/// Returns unity migrated users for Asset Optimization
		/// </summary>
		/// <param name="editorPersonaId">Editor user persona id</param>
		/// <param name="productCode">AO product code to filter users (e.g. AIRM, YS, MA)</param>
		/// <param name="datafilter">A datafilter used to filter and paginate results.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List Asset Optimization unity migrated users", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/ao/unity-migrated-users")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage GetUnityMigratedUsers(long editorPersonaId, string productCode, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			ManagePersona managePersona = new ManagePersona();
			var persona = managePersona.GetPersona(editorPersonaId);
			if (persona == null)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");
			base._userClaims.UserRealPageGuid = persona.RealPageId;
			var manageProductAo = new ManageProductAssetOptimization(base._userClaims);

			var result = manageProductAo.GetUnityMigratedUsers(base._userClaims.PersonaId, productCode, datafilter);
			if (!result.IsError)
				return Request.CreateResponse(HttpStatusCode.OK, result);
			else
				return Request.CreateResponse(HttpStatusCode.Forbidden, result);
		}
		#endregion
	}
}