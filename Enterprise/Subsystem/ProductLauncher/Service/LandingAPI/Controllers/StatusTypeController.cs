using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// StatusType Controller
	/// </summary>
	public class StatusTypeController : BaseApiController
	{
		#region Private variables
		IRepositoryResponse _repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public StatusTypeController() : base() { }
		#endregion

		#region Public Methods
		/// <summary>
		/// List of StatusTypes
		/// </summary>
		/// <param name="CategoryTypeName">Category TypeName (e.g. Status)</param>
		/// <param name="CategoryName">Category Name (e.g. User Status)</param>
		/// <returns>List of StatusType Details</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when  object have invalid entries / when Information is out of sync with the server)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the Status Types", Type = typeof(IStatusType))]
		[SwaggerResponseExamples(typeof(IStatusType), typeof(StatusTypeCommonExample))]
		[Route("statustype/categorytype/{CategoryTypeName}/categoryname/{CategoryName}")]
		[HttpGet]
		public HttpResponseMessage GetStatusType(string CategoryTypeName, string CategoryName)
		{
			IApiError apiError;
			if ((string.IsNullOrWhiteSpace(CategoryTypeName)) || (string.IsNullOrWhiteSpace(CategoryName)))
			{
				apiError = new ApiError()
				{
					Id = Guid.NewGuid().ToString(),
					Status = (short)HttpStatusCode.BadRequest,
					Title = "Invalid parameter.",
					Detail = "Invalid parameter(s):"
						+ (string.IsNullOrWhiteSpace(CategoryTypeName) ? $" Category TypeName: {CategoryTypeName}" : "")
						+ (string.IsNullOrWhiteSpace(CategoryName) ? $" Category Name: {CategoryName}" : ""),
					Links = string.Empty,
					Code = "StatusType.GetStatusType.1",
					Source = new ApiErrorSource()
					{
						JsonPointer = string.Empty,
						Parameter = string.Empty
					}
				};
				return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			}

			ISecuritySettings securitySettings = new SecuritySettings();

			IManageStatusType manageStatusType = new ManageStatusType();
			IList<StatusType> statusTypeList= manageStatusType.GetStatusType(CategoryTypeName, CategoryName);

			if (statusTypeList.Count == 0)
			{
				apiError = new ApiError()
				{
					Id = Guid.NewGuid().ToString(),
					Status = (short)HttpStatusCode.NotFound,
					Title = "Status Types not found.",
					Detail = $"Status Types not found for Category TypeName: {CategoryTypeName}, Category name: {CategoryName}",
					Links = string.Empty,
					Code = "StatusType.GetStatusType.2",
					Source = new ApiErrorSource()
					{
						JsonPointer = string.Empty,
						Parameter = string.Empty
					}
				};
				return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
			}
			return Request.CreateResponse(HttpStatusCode.OK, statusTypeList);
		}
		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the User List Model webapi result
		/// </summary>
		public class StatusTypeCommonExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>StatusType example</returns>
			public object GetExamples()
			{
				IList<StatusType> statusTypeList = new List<StatusType>()
				{
					new StatusType()
					{
						StatusTypeId = 1,
						Name = "Active"
					},
					new StatusType()
					{
						StatusTypeId = 24,
						Name = "Disabled"
					},
					new StatusType()
					{
						StatusTypeId = 23,
						Name = "Expired"
					},
					new StatusType()
					{
						StatusTypeId = 12,
						Name = "ForceResetPassword"
					},
					new StatusType()
					{
						StatusTypeId = 3,
						Name = "Locked"
					},
					new StatusType()
					{
						StatusTypeId = 2,
						Name = "Pending"
					}
				};
				return statusTypeList;
			}
		}
		#endregion
	}
}
