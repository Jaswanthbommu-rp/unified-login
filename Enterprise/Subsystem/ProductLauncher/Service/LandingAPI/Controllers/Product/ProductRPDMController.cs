using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product
{
	public class ProductRPDMController : BaseApiController
    {
		private IManageProductRPDocumentManagement _manageProductRPDocumentManagement;

	    #region Constructor
	    /// <summary>
	    /// Default constructor
	    /// </summary>
	    public ProductRPDMController() : base() { }

	    /// <summary>
	    /// Used for unit testing
	    /// </summary> 
	    public ProductRPDMController(IManageProductRPDocumentManagement manageProductRPDocumentManagement)
	    {
		    _manageProductRPDocumentManagement = manageProductRPDocumentManagement;
		    base.Request = new HttpRequestMessage();
		    base.Request.SetConfiguration(new HttpConfiguration());
	    }

	    /// <summary>
	    /// Used to initialize the base class  
	    /// </summary>
	    /// <param name="controllerContext"></param>
	    protected override void Initialize(HttpControllerContext controllerContext)
	    {
		    base.Initialize(controllerContext);
		    _manageProductRPDocumentManagement = new ManageProductRPDocumentManagement(_userClaims);
	    }

		#endregion

		/// <summary>
		/// Used to get a list of roles
		/// </summary>
		/// <param name="editorPersonaId">The user who is adding/editing the user</param>
		/// <param name="userPersonaId">The user being added/edited</param>
		/// <param name="datafilter">A datafilter used to filter the roles. (Not currently implemented)</param>
		/// <returns>A list of product roles</returns>
		/// <remarks>Roles that contain a roletype can be called using role/classifier/roleid to get a list of data associated with the role that can be assigned to the user. i.e. a list of properties associated to the role</remarks>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Success", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(RoleExample))]
		[Route("products/rpdm/roles")]
        [HttpGet]
        public HttpResponseMessage GetRoles(long editorPersonaId, long userPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var result = _manageProductRPDocumentManagement.GetRoles(editorPersonaId, userPersonaId, datafilter);

            if(result.IsError)
                Request.CreateResponse(HttpStatusCode.InternalServerError, result);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

		/// <summary>
		/// Used to get a list of additional information related to the role id requested
		/// </summary>
		/// <param name="editorPersonaId">The user who is adding/editing the user</param>
		/// <param name="userPersonaId">The user being added/edited</param>
		/// <param name="roleId">The id of the role to get information for</param>
		/// <param name="datafilter">A datafilter used to filter the roles. (Not currently implemented)</param>
		/// <returns>A list of items associated to the role given</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Success", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ClassifierExample))]
		[Route("products/rpdm/role/classifier")]
	    [HttpGet]
	    public HttpResponseMessage GetRoleClassifierDataset(long editorPersonaId, long userPersonaId, string roleId, [FromUri]RequestParameter datafilter)
	    {
		    if (editorPersonaId == 0)
			    return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

		    if (_realpageUserId == Guid.Empty)
			    return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

		    var result = _manageProductRPDocumentManagement.GetRoleClassifierDataset(editorPersonaId: editorPersonaId, userPersonaId: userPersonaId, roleId: roleId, datafilter: datafilter);

		    if(result.IsError)
		        Request.CreateResponse(HttpStatusCode.Forbidden, result);

		    return Request.CreateResponse(HttpStatusCode.OK, result);
	    }

		/// <summary>
		/// Used to get the domain for the given user id
		/// </summary>
		/// <param name="personaId">The user to get the domain for</param>
		/// <returns>The domain for Doc Management for the given persona</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
	    [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
	    [SwaggerResponse(HttpStatusCode.OK, Description = "Success", Type = typeof(HttpResponseMessage))]
	    [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
	    [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(DomainExample))]
	    [Route("products/rpdm/domain")]
	    [HttpGet]
	    public HttpResponseMessage GetDomain(long personaId)
	    {
		    if (personaId == 0)
			    return Request.CreateResponse(HttpStatusCode.BadRequest, "personaId not supplied.");

		    if (_realpageUserId == Guid.Empty)
			    return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

		    var result = _manageProductRPDocumentManagement.GetDomain(personaId);

		    if (result.IsError)
			    Request.CreateResponse(HttpStatusCode.Forbidden, result);

		    return Request.CreateResponse(HttpStatusCode.OK, result);
	    }

		#region Migration API
		/// <summary>
		/// Returns product users of an organization for given user.
		/// </summary>
		/// 
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List Document Directory users", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/rpdm/migration-users")]
		[Authorize] // Todo: Need to implement Resource Scope Based Authorization
		[HttpGet]
		public HttpResponseMessage ListRPDMigrationUsers(long editorPersonaId, [FromUri] RequestParameter datafilter)
		{
			if (editorPersonaId == 0)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

			ManagePersona managePersona = new ManagePersona();
			var persona = managePersona.GetPersona(editorPersonaId);
			if (persona == null)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

			base._userClaims.UserRealPageGuid = persona.RealPageId;
			IManageProductRPDocumentManagement manageRPDocument = new ManageProductRPDocumentManagement(_userClaims);

			return Request.CreateResponse(HttpStatusCode.OK, manageRPDocument.GetMigrationUsers(editorPersonaId, datafilter));
		}

		/// <summary>
		/// Update migration status of users.
		/// </summary>
		/// 
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Mark Document Directory users to migrated", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/rpdm/migrate-users")]
		[Authorize]
		[HttpPut]
		public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
		{
			IManageProductRPDocumentManagement manageRPDocument = new ManageProductRPDocumentManagement(_userClaims);
			return Request.CreateResponse(HttpStatusCode.OK, manageRPDocument.UpdateUsersMigrationStatus(_personaId, migrateUsers));
		}
		#endregion
		///// <summary>
		///// Used to create/update Users. Not used from API, should be called though ProductBatch
		///// </summary>
		///// <param name="editorPersonaId"></param>
		///// <param name="userPersonaId"></param>
		///// <param name="rolepropList"></param>
		///// <returns></returns>
		//[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		//[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		//[SwaggerResponse(HttpStatusCode.Created, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		//[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request (when role or property data has invalid entries / when Iiformation is out of sync with the server)")]
		//[Route("products/rpdm/user")]
		//[Authorize]
		//[HttpPost]
		//public HttpResponseMessage CreateRPDMUser(long editorPersonaId, long userPersonaId, RolePropertyList rolepropList)
		//{
		//	if (editorPersonaId == 0 || userPersonaId == 0) { throw new HttpResponseException(HttpStatusCode.BadRequest); }
		//	if (rolepropList == null)
		//	{
		//		rolepropList = new RolePropertyList();
		//	}

		//	string result = _manageProductRPDocumentManagement.ManageRPDMUser(editorPersonaId, userPersonaId, rolepropList);
		//	if (string.IsNullOrEmpty(result))
		//	{
		//		return Request.CreateResponse(HttpStatusCode.Created);
		//	}
		//	return Request.CreateResponse(HttpStatusCode.BadRequest, result);
		//}

		/// <summary>
		/// Used to document examples of the role response
		/// </summary>
		[ExcludeFromCodeCoverage]
	    public class RoleExample : IProvideExamples
	    {
		    /// <summary>
		    /// Example object data used by Swagger to document the output of the webapi method
		    /// </summary>
		    /// <returns>Response example</returns>
		    public object GetExamples()
		    {
			    IList<ProductRole> dataList = new List<ProductRole>()
			    {
				    new ProductRole() {ID = "74655", Name = "Property Manager", IsAssigned = false, Roletype = "Site Name", Alias = "/{companyalias}/roles/74655"},
				    new ProductRole() {ID = "34433", Name = "Domain Admin", IsAssigned = true, Roletype = null, Alias = "/{companyalias}/roles/34433"}
			    };

			    ListResponse example = new ListResponse()
			    {
				    Records = dataList.Cast<object>().ToList(),
				    TotalRows = dataList.Count,
				    RowsPerPage = dataList.Count,
				    TotalPages = 1,
				    ErrorReason = ""
			    };
				return example;
		    }
	    }

	    /// <summary>
	    /// Used to document examples of the classifier response
	    /// </summary>
	    [ExcludeFromCodeCoverage]
	    public class ClassifierExample : IProvideExamples
	    {
		    /// <summary>
		    /// Example object data used by Swagger to document the output of the webapi method
		    /// </summary>
		    /// <returns>Response example</returns>
		    public object GetExamples()
		    {
			    IList<ProductProperty> dataList = new List<ProductProperty>()
			    {
				    new ProductProperty() {ID = "74767", Name = "Between The Fifties", IsAssigned = false, Alias = "/{companyalias}/datasets/sitename/values/74767"},
				    new ProductProperty() {ID = "74763", Name = "PCT427 Hunters Point", IsAssigned = true, Alias = "/{companyalias}/datasets/sitename/values/74763"},
				    new ProductProperty() {ID = "74302", Name = "Heritage", IsAssigned = true, Alias = "/{companyalias}/datasets/sitename/values/74302"}
				};

			    ListResponse example = new ListResponse()
			    {
				    Records = dataList.Cast<object>().ToList(),
				    TotalRows = dataList.Count,
				    RowsPerPage = dataList.Count,
				    TotalPages = 1,
				    ErrorReason = ""
			    };
			    return example;
		    }
	    }

	    /// <summary>
	    /// Used to document examples of the domain response
	    /// </summary>
	    [ExcludeFromCodeCoverage]
		public class DomainExample : IProvideExamples
	    {
		    /// <summary>
		    /// Example object data used by Swagger to document the output of the webapi method
		    /// </summary>
		    /// <returns>Response example</returns>
			public object GetExamples()
		    {
				ListResponse example = new ListResponse()
			    {
					Additional = "somedomain",
				    Records = null,
				    TotalRows = 0,
				    RowsPerPage = 0,
				    TotalPages = 1,
				    ErrorReason = ""
			    };
			    return example;
			}
	    }
	}
}