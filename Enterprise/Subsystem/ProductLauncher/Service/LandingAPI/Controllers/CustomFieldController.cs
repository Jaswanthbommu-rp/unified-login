using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	public class CustomFieldController : BaseApiController
	{
		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public CustomFieldController()
		{
			_manageCustomFields = new ManageCustomFields(_userClaims);
		}

		/// <summary>
		/// Used for dependency injection
		/// </summary>
		/// <param name="manageCustomFields"></param>
		public CustomFieldController(IManageCustomFields manageCustomFields)
		{
			_manageCustomFields = manageCustomFields;
		}
		#endregion

		#region Private variables
		IManageCustomFields _manageCustomFields;
		#endregion

		/// <summary>
		/// List CustomField Types
		/// </summary>
		/// <param name="fieldTypeId">Optinal CustomField TypeId</param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Organization customfield type object has invalid entries / when Information is out of sync with the server)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "CustomField Type result", Type = typeof(ICustomFieldType))]
		[SwaggerResponseExamples(typeof(ICustomFieldType), typeof(CustomFieldTypeExample))]
		[SwaggerOperation("CustomFieldType")]
		[Route("customfield/type")]
		[HttpGet]
		[Obsolete("Deprecated")]
		public HttpResponseMessage CustomFieldType(byte? fieldTypeId = null)
		{
			IList<CustomFieldType> customFieldTypeList = _manageCustomFields.GetCustomFieldType(fieldTypeId);

			ListResponse response = new ListResponse()
			{
				Records = customFieldTypeList.Cast<object>().ToList(),
				TotalRows = customFieldTypeList.Count(),
				RowsPerPage = customFieldTypeList.Count(),
				ErrorReason = string.Empty,
				TotalPages = 1
			};
			return Request.CreateResponse(HttpStatusCode.OK, response);
		}

		#region Output results for documentation
		/// <summary>
		/// Used to document examples of the customfields Model webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class CustomFieldTypeExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>List of User CustomFields example</returns>
			public object GetExamples()
			{
				IList<CustomFieldType> customFieldTypeList = new List<CustomFieldType>()
				{
					new CustomFieldType()
					{
					  FieldTypeId = 1,
					  Name = "Alphanumeric",
					  Description = "Alphanumeric"
					},
					new CustomFieldType()
					{
					  FieldTypeId = 2,
					  Name = "Numeric",
					  Description = "Numeric"
					}
				};

				ListResponse response = new ListResponse()
				{
					Records = customFieldTypeList.Cast<object>().ToList(),
					TotalRows = customFieldTypeList.Count(),
					RowsPerPage = customFieldTypeList.Count(),
					ErrorReason = string.Empty,
					TotalPages = 1
				};
				return response;
			}
		}
		#endregion
	}
}