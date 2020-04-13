using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using System.Diagnostics.CodeAnalysis;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	public class BlueBookController : BaseApiController
    {
        public BlueBookController() : base() { }

		/// <summary>
		/// List of Top level properties from BlueBook
		/// </summary>
		/// <param name="booksCompanyMasterId">BlueBook CustomerMasterId</param>
		/// <param name="include">List of fields names (comma delimited) to return in the response: customerPropertyId,customerCompanyId,propertyName,address</param>
		/// <param name="filter">Optional (default = {ampersand}filter[isActive]=true{ampersand}page[size]=9999)</param>
		/// <returns>List of Top level properties from BlueBook</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of Top level properties from BlueBook", Type = typeof(CustomerProperty))]
		[SwaggerResponseExamples(typeof(CustomerProperty), typeof(CustomerPropertyExample))]
		[Authorize]
		[HttpGet]
		[Route("CustomerProperty/{booksCompanyMasterId}")]
		public HttpResponseMessage GetCustomerProperty(long booksCompanyMasterId, string include = null, string filter = null)
		{
			ObjectListOutput<CustomerProperty, IErrorData> output = new ObjectListOutput<CustomerProperty, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();

			IManageBlueBook manageBlueBook = new ManageBlueBook(_userClaims);

			IList<CustomerProperty> customerPropertyList = manageBlueBook.GetCustomerProperty(booksCompanyMasterId, include, filter);

			output = new ObjectListOutput<CustomerProperty, IErrorData>() { list = customerPropertyList, Status = errorStatus };
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		[ExcludeFromCodeCoverage]
		public class CustomerPropertyExample : IProvideExamples
		{
			public object GetExamples()
			{
				List<CustomerProperty> customerPropertyList = new List<CustomerProperty>()
				{
					new CustomerProperty()
					{
						id = "9533",
						companyId = "826",
						propertyId = "9533",
						name = "LYONS COURT SENIOR",
						street = "510 SELBY AVE",
						city = "SAINT PAUL",
						state = "MN",
						postalCode = "55102-1729"
					},
					new CustomerProperty()
					{
						id = "9538",
						companyId = "826",
						propertyId = "9538",
						name = "HARRISON LOFTS",
						street = "1420 N HARRISON ST",
						city = "DAVENPORT",
						state = "IA",
						postalCode = "52803-4801"
					}
				};

				ObjectListOutput<CustomerProperty, IErrorData> output = new ObjectListOutput<CustomerProperty, IErrorData>() { list = customerPropertyList };

				return output;
			}
		}
	}
}
