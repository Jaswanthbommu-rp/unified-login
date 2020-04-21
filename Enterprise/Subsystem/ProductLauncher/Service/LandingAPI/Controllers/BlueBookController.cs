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
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	public class BlueBookController : BaseApiController
    {
        public BlueBookController() : base() { }

		/// <summary>
		/// List of Top level properties from BlueBook
		/// </summary>
		/// <param name="booksCompanyMasterId">Optional BlueBook CustomerMasterId (default to value from Token))</param>
		/// <param name="include">Optional List of fields names (comma delimited) to return in the response: customerPropertyId,customerCompanyId,propertyName,address</param>
		/// <param name="filter">Optional (default = {ampersand}filter[isActive]=true{ampersand}page[size]=9999)</param>
		/// <returns>List of Top level properties from BlueBook</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List of Top level properties from BlueBook", Type = typeof(ProductProperty))]
		[SwaggerResponseExamples(typeof(ProductProperty), typeof(ProductPropertyExample))]
		[Authorize]
		[HttpGet]
		[Route("CustomerProperty")]
		public HttpResponseMessage GetCustomerProperty(long booksCompanyMasterId = 0, string include = null, string filter = null)
		{
			ObjectListOutput<ProductProperty, IErrorData> output = new ObjectListOutput<ProductProperty, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();

			IManageBlueBook manageBlueBook = new ManageBlueBook(_userClaims);

			IList<ProductProperty> productPropertyList = manageBlueBook.GetCustomerProperty(booksCompanyMasterId, include, filter);

			output = new ObjectListOutput<ProductProperty, IErrorData>() { list = productPropertyList, Status = errorStatus };
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		[ExcludeFromCodeCoverage]
		public class ProductPropertyExample : IProvideExamples
		{
			public object GetExamples()
			{
				List<ProductProperty> productPropertyList = new List<ProductProperty>()
				{
					new ProductProperty()
					{
						ID = "9533",
						Name = "LYONS COURT SENIOR",
						Street1 = "510 SELBY AVE",
						City = "SAINT PAUL",
						State = "MN",
						Zip = "55102-1729"
					},
					new ProductProperty()
					{
						ID = "9538",
						Name = "HARRISON LOFTS",
						Street1 = "1420 N HARRISON ST",
						City = "DAVENPORT",
						State = "IA",
						Zip = "52803-4801"
					}
				};

				ObjectListOutput<ProductProperty, IErrorData> output = new ObjectListOutput<ProductProperty, IErrorData>() { list = productPropertyList };

				return output;
			}
		}
	}
}
