using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.Attribute;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.ResponseObject;
using UnifiedLogin.SharedObjects.Swagger;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
	/// <summary>
	/// Product Controller for managing product-related operations.
	/// Refactored to use modern ASP.NET Core dependency injection patterns.
	/// </summary>
	[Authorize]
	[ApiController]
	[ApiVersion("1.0")]
	[Route("v{version:apiVersion}/[controller]")]
	public class ProductController : ControllerBase
	{
		private readonly IProductRepository _productRepository;
		private readonly IUserClaimsAccessor _userClaimsAccessor;

		/// <summary>
		/// Constructor with dependency injection for product controller.
		/// Follows modern ASP.NET Core patterns for testable, maintainable code.
		/// All dependencies are injected as interfaces for loose coupling and testability.
		/// </summary>
		/// <param name="productRepository">Repository for product-related operations</param>
		/// <param name="userClaimsAccessor">Accessor for current authenticated user's claims</param>
		public ProductController(
			IProductRepository productRepository,
			IUserClaimsAccessor userClaimsAccessor)
		{
			_productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
			_userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
		}

		/// <summary>
		/// Get list of products
		/// </summary>
		/// <returns>List of all products</returns>
		[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse((int)HttpStatusCode.OK, Description = "Get information about the books products", Type = typeof(GbProductMap))]
		[Route("products")]
		[AuthorizeScope("userinfoapi")]
		[HttpPost]
		public ActionResult GetProducts()
		{
			WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "GetProducts", "Started" });

			var result = GetAllProducts();

			var logData = new Dictionary<string, object> { { "result", result } };
			WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", logData, messageProperties: new object[] { "GetProducts", "Data returned" });
			return Ok(result);
		}

		#region Unified notifications endpoint
		/// <summary>
		/// Get list of users by companyid or productids
		/// </summary>
		/// <param name="companyId">Company ID to filter users</param>
		/// <param name="upfmId">UPFM ID to filter users</param>
		/// <param name="products">List of product IDs</param>
		/// <param name="userType">User type filter</param>
		/// <param name="userStatus">User status filter</param>
		/// <returns>List of users filtered by company and products</returns>
		[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse((int)HttpStatusCode.OK, Description = "Get list of users by company and products", Type = typeof(ProductUsers))]
		[Route("usersbycompanyproducts")]
		[AuthorizeScope("userinfoapi")]
		[HttpGet]
		public ActionResult GetUsersByCompanyorProducts(
			string companyId = null,
			string upfmId = null,
			[FromQuery] IList<int?> products = null,
			[FromQuery] string userType = null,
			[FromQuery] string userStatus = null)
		{
			WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "GetUsersByCompanyorProducts", "Started" });

			if (!ValidateCompanyProductsDetailsData(companyId, products))
			{
				return BadRequest();
			}

			var result = _productRepository.GetUsersByCompanyorProducts(companyId, products, upfmId, userType, userStatus);

			var logData = new Dictionary<string, object> { { "result", result } };
			WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", logData, messageProperties: new object[] { "GetUsersByCompanyorProducts", "Data returned" });

			return Ok(result);
		}

		/// <summary>
		/// Get Unified Login User Mapping id for given Product user Id's by Blue Book Company ID or upfmId and ProductId.
		/// </summary>
		/// <param name="productUserIDMappingRequest">Request containing company ID, product code, and product user IDs</param>
		/// <returns>Mapped unified login user details</returns>
		[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse((int)HttpStatusCode.OK, Description = "Get list of UL mapping users id by company and products", Type = typeof(MappedUnifiedLoginUserDetails))]
		[Route("ulusermappingidbycompanyproductUserId")]
		[AuthorizeScope("userinfoapi")]
		[HttpPost]
		public ActionResult GetULUserIdMappedToProductUserIdByCompanyAndProducts([FromBody] ProductUserIDMappingRequest productUserIDMappingRequest)
		{
			WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "GetULUserIdMappedToProductUserIdByCompanyAndProducts", "Started" });

			var mappedUnifiedLoginUserDetails = new MappedUnifiedLoginUserDetails
			{
				CompanyId = productUserIDMappingRequest?.CompanyId ?? 0,
				ProductCode = productUserIDMappingRequest?.ProductCode,
				upfmId = productUserIDMappingRequest?.upfmId,
				ULMappedPersonaId = new List<ULMappedPersonaIds>()
			};

			// Validate request
			if (!ValidateProductUserMappingRequest(productUserIDMappingRequest, out var productId))
			{
				return BadRequest(mappedUnifiedLoginUserDetails);
			}

			mappedUnifiedLoginUserDetails.ULMappedPersonaId = _productRepository.GetULMappingPersonaIDsByCompanyAndProducts(
				productUserIDMappingRequest.CompanyId,
				productUserIDMappingRequest.upfmId,
				productId,
				productUserIDMappingRequest.ProductUserId);

			var logData = new Dictionary<string, object> { { "result", mappedUnifiedLoginUserDetails } };
			WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", logData, messageProperties: new object[] { "GetULUserIdMappedToProductUserIdByCompanyAndProducts", "Data returned" });

			return Ok(mappedUnifiedLoginUserDetails);
		}

		/// <summary>
		/// Get list of users by companyid or productcodes
		/// </summary>
		/// <param name="productcode">List of product codes</param>
		/// <param name="companyid">Company ID to filter users</param>
		/// <param name="upfmId">UPFM ID to filter users</param>
		/// <param name="rowsPerPage">Number of rows per page</param>
		/// <param name="pageNumber">Current page number</param>
		/// <param name="roles">List of roles to filter</param>
		/// <param name="rights">List of rights to filter</param>
		/// <param name="propertyIds">List of property IDs to filter</param>
		/// <param name="companyDomain">Company domain to filter</param>
		/// <returns>Paginated list of users by company and product codes</returns>
		[SwaggerResponse((int)HttpStatusCode.BadRequest, Description = "Bad request")]
		[SwaggerResponse((int)HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse((int)HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse((int)HttpStatusCode.OK, Description = "Get list of users by company and product codes", Type = typeof(ProductUsers))]
		[SwaggerResponseExamples(typeof(ProductUsers), typeof(UserCompanyProductCodeExample))]
		[Route("users")]
		[AuthorizeScope("userinfoapi")]
		[HttpGet]
		public ActionResult GetUsersByCompanyorProductCodes(
			[FromQuery] List<string> productcode,
			string companyid = null,
			string upfmId = null,
			int? rowsPerPage = 5000,
			int? pageNumber = 1,
			[FromQuery] List<string> roles = null,
			[FromQuery] List<string> rights = null,
			[FromQuery] List<string> propertyIds = null,
			string companyDomain = null)
		{
			WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", messageProperties: new object[] { "GetUsersByCompanyorProductCodes", "Started" });

			// Validate input parameters
			if ((string.IsNullOrEmpty(companyid) && string.IsNullOrEmpty(upfmId)) || productcode == null || !productcode.Any())
			{
				return CreateEmptyProductUsersResponse(isError: true, errorReason: "BadRequest");
			}

			// Clean and deduplicate property IDs
			var cleanedPropertyIds = CleanPropertyIdList(propertyIds);

			// Convert product codes to product IDs
			var productIds = ConvertProductCodesToIds(productcode);
			var sharedProductIds = _productRepository.GetProductSharedwithOtherProductIdList(productIds);

			// Get users
			var result = _productRepository.GetUsersByCompanyorProducts(
				companyid,
				upfmId,
				sharedProductIds,
				rowsPerPage.Value,
				pageNumber.Value,
				roles,
				rights,
				cleanedPropertyIds,
				companyDomain);

			if (result == null)
			{
				return CreateEmptyProductUsersResponse(isError: true, errorReason: "BadRequest");
			}

			var response = CreatePagedResponse(result, pageNumber.Value, rowsPerPage.Value);

			var logData = new Dictionary<string, object> { { "result", response } };
			WriteToLog(LogEventLevel.Information, "{ActionName} - {state}", logData, messageProperties: new object[] { "GetUsersByCompanyorProductCodes", "Data returned" });

			return Ok(response);
		}
		#endregion

		#region Private Helper Methods

		/// <summary>
		/// Gets all products excluding certain system products
		/// </summary>
		/// <returns>List of all available products</returns>
		private IList<GbProductMap> GetAllProducts()
		{
			var result = _productRepository.GetAllProducts();

			var excludeProducts = new List<string>
			{
				ProductEnum.SalesForce.ToEnumDescription(),
				ProductEnum.UnifiedPlatform.ToEnumDescription(),
				ProductEnum.UnifiedUI.ToEnumDescription()
			};

			return result.Where(x => !excludeProducts.Contains(x.BooksProductCode)).ToList();
		}

		/// <summary>
		/// Validates product user mapping request
		/// </summary>
		/// <param name="request">Product user mapping request</param>
		/// <param name="productId">Output parameter for product ID</param>
		/// <returns>True if validation passes, false otherwise</returns>
		private bool ValidateProductUserMappingRequest(ProductUserIDMappingRequest request, out int productId)
		{
			productId = 0;

			if (request == null)
			{
				return false;
			}

			if (request.CompanyId < 1 && string.IsNullOrEmpty(request.upfmId))
			{
				return false;
			}

			if (string.IsNullOrEmpty(request.ProductCode) || request.ProductUserId?.Count == 0)
			{
				return false;
			}

			var allProducts = GetAllProducts();
			productId = allProducts.FirstOrDefault(x => x.BooksProductCode == request.ProductCode)?.ProductId ?? 0;

			return productId > 0;
		}

		/// <summary>
		/// Converts product codes to product IDs
		/// </summary>
		/// <param name="productCodes">List of product codes</param>
		/// <returns>List of product IDs</returns>
		private IList<int> ConvertProductCodesToIds(List<string> productCodes)
		{
			var products = new List<int>();
			var productList = _productRepository.GetAllProducts();

			foreach (var code in productCodes)
			{
				var productId = ProductEnumHelper.GetProductIdByProductCode(code, productList);
				products.Add(productId);
			}

			return products;
		}

		/// <summary>
		/// Cleans and deduplicates property ID list
		/// </summary>
		/// <param name="propertyIds">List of property IDs</param>
		/// <returns>Cleaned list of property IDs</returns>
		private List<string> CleanPropertyIdList(List<string> propertyIds)
		{
			if (propertyIds == null || !propertyIds.Any())
			{
				return null;
			}

			return propertyIds
				.Where(x => !string.IsNullOrEmpty(x))
				.Distinct()
				.ToList();
		}

		/// <summary>
		/// Creates an empty product users response
		/// </summary>
		/// <param name="isError">Whether this is an error response</param>
		/// <param name="errorReason">Error reason if applicable</param>
		/// <returns>BadRequest ActionResult with empty response</returns>
		private ActionResult CreateEmptyProductUsersResponse(bool isError, string errorReason = null)
		{
			var response = new PagedResponse
			{
				Meta = new Meta(),
				Data = new List<ProductUsers>().Cast<object>().ToList(),
				IsError = isError,
				ErrorReason = errorReason
			};

			response.Meta.CurrentPage = 1;
			response.Meta.TotalRows = 0;
			response.Meta.RowsPerPage = 0;

			return BadRequest(response);
		}

		/// <summary>
		/// Creates a paged response from product users list
		/// </summary>
		/// <param name="users">List of product users</param>
		/// <param name="pageNumber">Current page number</param>
		/// <param name="rowsPerPage">Rows per page</param>
		/// <returns>Paged response object</returns>
		private PagedResponse CreatePagedResponse(IList<ProductUsers> users, int pageNumber, int rowsPerPage)
		{
			var response = new PagedResponse { Meta = new Meta() };

			response.Data = users.Cast<object>().ToList();
			response.Meta.CurrentPage = pageNumber;
			response.Meta.TotalRows = users.Any() ? users[0].TotalRecords : 0;
			response.Meta.RowsPerPage = rowsPerPage;

			return response;
		}

		/// <summary>
		/// Validates company and product details data
		/// </summary>
		/// <param name="companyId">Company ID</param>
		/// <param name="products">List of product IDs</param>
		/// <returns>True if validation passes, false otherwise</returns>
		private bool ValidateCompanyProductsDetailsData(string companyId, IList<int?> products)
		{
			if (string.IsNullOrEmpty(companyId) && products == null)
			{
				return false;
			}

			if (!int.TryParse(companyId, out int compId) && products == null)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Used to write to the central log
		/// </summary>
		/// <param name="logType">Log Type</param>
		/// <param name="message">Message template</param>
		/// <param name="logData">Dictionary of additional properties to log</param>
		/// <param name="exception">Exception details</param>
		/// <param name="messageProperties">Message properties</param>
		private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
		{
			try
			{
				var logger = Log.Logger;
				if (logData?.Keys != null)
				{
					logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
				}
				logger = logger.ForContext("ProductModule", this.GetType());
				logger = logger.ForContext("CorrelationId", _userClaimsAccessor.CorrelationId.ToString());

				logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValue0: messageProperties?[0], propertyValue1: messageProperties?[1]);
			}
			catch
			{
				/*ignored*/
			}
		}

		#endregion

		#region Examples

		/// <summary>
		/// Used to document examples of the UserCompanyProductCode result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class UserCompanyProductCodeExample : IProvideExamples
		{
			/// <summary>
			/// Gets example data for Swagger documentation
			/// </summary>
			/// <returns>Example PagedResponse with ProductUsers</returns>
			public object GetExamples()
			{
				var productUsers = new List<ProductUsers>
				{
					new ProductUsers
					{
						UserId = 2659,
						LoginName = "notificationsuser@test.com",
						FirstName = "Notifications",
						LastName = "User",
						PersonaId = 2649,
						PreferredPhoneNumber = "5555555555",
						Email = "notificationemail@test.com"
					},
					new ProductUsers
					{
						UserId = 2660,
						LoginName = "multiuser1@test.com",
						FirstName = "multi",
						LastName = "user1",
						PersonaId = 2657,
						PreferredPhoneNumber = "8888888888",
						Email = "notificationemail@test.com"
					}
				};

				var response = new PagedResponse
				{
					Meta = new Meta(),
					Data = productUsers.Cast<object>().ToList(),
					IsError = false,
					ErrorReason = null
				};

				response.Meta.CurrentPage = 1;
				response.Meta.TotalRows = 2;
				response.Meta.RowsPerPage = 5000;

				return response;
			}
		}
		#endregion
	}
}
