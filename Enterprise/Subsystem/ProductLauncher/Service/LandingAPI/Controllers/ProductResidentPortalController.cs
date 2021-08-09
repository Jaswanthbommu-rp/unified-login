using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Controller for all product management related APIs
	/// </summary>
	public class ProductResidentPortalController : BaseApiController
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
			//	//_userPersona = mp.GetActivePersona(_realpageUserId);
			//}
		}

		#region Public Methods
		/// <summary>
		/// Returns Properties  
		/// </summary>
		/// <param name="editorPersonaId">Assign user Id</param>
		/// <param name="userPersonaId">Author user persona id who is creating or editing user</param> 
		/// <param name="datafilter">A datafilter used to filter the properties.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List properties", Type = typeof(ListResponse))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/residentportal/properties")]
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
			ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);
			listResponse = manageProductResidentPortal.ListProperties(editorPersonaId, userPersonaId, datafilter);

			return Request.CreateResponse(HttpStatusCode.OK, listResponse);
		}

		/// <summary>
		/// Get Notification Settings 
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get resident portal notification settings", Type = typeof(INotifications))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/residentportal/notifications")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage GetNotificationSettings(long editorPersonaId, long userPersonaId = 0)
		{
			ObjectOutput<INotifications, IErrorData> output = new ObjectOutput<INotifications, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.Status = errorStatus;

			if (editorPersonaId == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalNotificationSettings.1";
				errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if ((_realpageUserId == Guid.Empty) || (_realpageUserId == null))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalNotificationSettings.2";
				errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			INotifications notifications = new Notifications();
			ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);

			notifications = manageProductResidentPortal.GetNotificationSettings(editorPersonaId, userPersonaId);
			if (notifications == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalNotificationSettings.3";
				errorStatus.ErrorMsg = $"Product Resident Portal - Get notification settings: Invalid User Id for PersonaId- {userPersonaId}";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			output.obj = notifications;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// Used to Resident Portal enterprise user details
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get resident portal enterprise OR manager user details", Type = typeof(IResidentPortalUser))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/residentportal/user")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage GetResidentPortalUser(long editorPersonaId, long userPersonaId)
		{
			ObjectOutput<IResidentPortalUser, IErrorData> output = new ObjectOutput<IResidentPortalUser, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.Status = errorStatus;

			if (editorPersonaId == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.1";
				errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if (userPersonaId == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.2";
				errorStatus.ErrorMsg = "Invalid parameter - userPersonaId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if ((_realpageUserId == Guid.Empty) || (_realpageUserId == null))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.3";
				errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IResidentPortalUser residentPortalUser = new ResidentPortalUser();
			ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);

			residentPortalUser = manageProductResidentPortal.GetUser(editorPersonaId, userPersonaId, string.Empty);
			if (residentPortalUser == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.4";
				errorStatus.ErrorMsg = $"Product Resident Portal - Get user: Invalid User Id for PersonaId- {userPersonaId}";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			residentPortalUser = manageProductResidentPortal.SetLevelAndGroupObjects(editorPersonaId, userPersonaId, residentPortalUser);
			output.obj = residentPortalUser;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// List Levels (Resident Poratl roles)
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List resident portal levels", Type = typeof(ILevel))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/residentportal/levels")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage ListLevels(long editorPersonaId, long userPersonaId = 0)
		{
			ObjectListOutput<ILevel, IErrorData> output = new ObjectListOutput<ILevel, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.Status = errorStatus;

			if (editorPersonaId == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.ListLevel.1";
				errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if ((_realpageUserId == Guid.Empty) || (_realpageUserId == null))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.ListLevel.2";
				errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			List<ILevel> levelList = new List<ILevel>();
			ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);

			levelList = manageProductResidentPortal.ListLevels(editorPersonaId, userPersonaId);
			if (levelList == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.ListLevel.3";
				errorStatus.ErrorMsg = "Product Resident Portal - List levels: No data";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			output.list = levelList;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// List Messaging groups
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List resident portal messaging groups", Type = typeof(IMessagingGroups))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/residentportal/messagegroups")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage ListMessagingGroups(long editorPersonaId, long userPersonaId = 0)
		{
			ObjectListOutput<IMessagingGroups, IErrorData> output = new ObjectListOutput<IMessagingGroups, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.Status = errorStatus;

			if (editorPersonaId == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.ListMessagingGroup.1";
				errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if ((_realpageUserId == Guid.Empty) || (_realpageUserId == null))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.ListMessagingGroup.2";
				errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			List<IMessagingGroups> messageGroupsList = new List<IMessagingGroups>();
			ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);

			messageGroupsList = manageProductResidentPortal.ListMessageGroups(editorPersonaId, userPersonaId);
			if (messageGroupsList == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.ListMessagingGroup.3";
				errorStatus.ErrorMsg = "Product Resident Portal - List messaging groups: No data";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			output.list = messageGroupsList;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// List Messaging groups
		/// </summary>
		/// <param name="editorPersonaId">new user editorPersonaId</param>
		/// <param name="userPersonaId">new user userPersonaId</param>
		/// <param name="loginName">new user loginName</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List resident portal messaging groups", Type = typeof(IMessagingGroups))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/residentportal/checkisuserexists")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage checkisuserexists(long editorPersonaId, long userPersonaId,string loginName)
		{
			ObjectOutput<IResidentPortalUser, IErrorData> output = new ObjectOutput<IResidentPortalUser, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.Status = errorStatus;
			ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);

			IResidentPortalUser residentPortalUser = new ResidentPortalUser();
			residentPortalUser = manageProductResidentPortal.GetUser(editorPersonaId, userPersonaId, loginName);
			if (residentPortalUser == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.GetResidentPortalUser.4";
				errorStatus.ErrorMsg = $"Product Resident Portal - Get user: Invalid User Id for PersonaId- {userPersonaId}";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			residentPortalUser = manageProductResidentPortal.SetLevelAndGroupObjects(editorPersonaId, userPersonaId, residentPortalUser);
			output.obj = residentPortalUser;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
		/// List Titles
		/// </summary>
		/// <param name="editorPersonaId">Logged-in user PersonaId</param>
		/// <param name="userPersonaId">new user PersonaId</param>
		/// <returns>Response with Success Message</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List resident portal titles", Type = typeof(ITitle))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/residentportal/titles")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage ListTitles(long editorPersonaId, long userPersonaId = 0)
		{
			ObjectListOutput<ITitle, IErrorData> output = new ObjectListOutput<ITitle, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.Status = errorStatus;

			if (editorPersonaId == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.ListTitles.1";
				errorStatus.ErrorMsg = "Invalid parameter - editorPersonaId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			if ((_realpageUserId == Guid.Empty) || (_realpageUserId == null))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.ListTitles.2";
				errorStatus.ErrorMsg = "Invalid - Enterprise User Id";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			List<ITitle> titleList = new List<ITitle>();
			ManageProductResidentPortal manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);

			titleList = manageProductResidentPortal.ListTitles(editorPersonaId, userPersonaId);
			if (titleList == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ProductResidentPortal.ListTitles.3";
				errorStatus.ErrorMsg = "Product Resident Portal - List titles: No data";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			output.list = titleList;
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
        [SwaggerResponse(HttpStatusCode.OK, Description = "List Resident Portal users", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/residentportal/migration-users")]
        [Authorize] // Todo: Need to implement Resource Scope Based Authorization
        [HttpGet]
        public HttpResponseMessage ListResidentPortalMigrationUsers(long editorPersonaId, [FromUri]RequestParameter datafilter)
        {
            if (editorPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

            ManagePersona managePersona = new ManagePersona();
            var persona = managePersona.GetPersona(editorPersonaId);
            if (persona == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not found.");

	        base._userClaims.UserRealPageGuid = persona.RealPageId;
            var manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, manageProductResidentPortal.GetMigrationUsers(editorPersonaId, datafilter));
        }

        /// <summary>
        /// Update migration status of users.
        /// </summary>
        /// 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Mark Resident Portal users to migrated", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("products/residentportal/migrate-users")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers)
        {
            var manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);
            return Request.CreateResponse(HttpStatusCode.OK, manageProductResidentPortal.UpdateUsersMigrationStatus(_personaId, migrateUsers));
        }

        #endregion

        #region User-Status

        /// <summary>
        /// Deletes the resident portal user.
        /// </summary>
        /// <param name="produtUser">The produt user.</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "User Deleted Successfully", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad Request")]
        [SwaggerResponseExamples(typeof(HttpResponseMessage), typeof(ResponseExample))]
        [Route("products/residentportal/user/MT/status")]
        [Authorize]
        [HttpPut]
        public HttpResponseMessage UpdateResidentPortalUserStatus(ProductUser produtUser)
        {
            var manageProductResidentPortal = new ManageProductResidentPortal(base._userClaims);
            if (!manageProductResidentPortal.DeleteUser(_personaId, produtUser.UserId, produtUser.UserName))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Deleteing ResidentPortal user failed.");
            }
            return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");
        }

        #endregion

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
				IList<string> list = new List<string>
				{
					"Center 1",
					"Center 2",
					"Center 3"
				};

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
		#endregion
	}
}