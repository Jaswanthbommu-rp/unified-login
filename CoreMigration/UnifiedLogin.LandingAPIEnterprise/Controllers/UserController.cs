using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RealPage.DataAccess.Dapper;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Security.Claims;
using UnifiedLogin.BusinessLogic.Attributes;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User;
using UnifiedLogin.BusinessLogic.Logic.Enterprise.User.Models;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Logic.Product.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory;
using UnifiedLogin.BusinessLogic.Logic.Security;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.BusinessLogic.Repository.Security;
using UnifiedLogin.DataAccess;
using UnifiedLogin.ServiceDefaults;
using UnifiedLogin.SharedObjects.Attribute;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enterprise;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Helper;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Security;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.OneSite;
using UnifiedLogin.SharedObjects.Product.Ops;
using UnifiedLogin.SharedObjects.Product.Rum;
using UnifiedLogin.SharedObjects.ResponseObject;
using UnifiedLogin.SharedObjects.Saml;
using UnifiedLogin.SharedObjects.Swagger;
using IRepository = UnifiedLogin.DataAccess.IRepository;

namespace UnifiedLogin.LandingAPIEnterprise.Controllers
{
	/// <summary>
	/// User Controller - Refactored to modern ASP.NET Core patterns
	/// TODO: This controller should be split into:
	/// - UserManagementController (CRUD operations)
	/// - UserProductController (product assignments)
	/// - UserProfileController (profile operations)
	/// </summary>
	[Authorize]
	[ApiController]
	[ApiVersion("1.0")]
	[Route("v{version:apiVersion}/[controller]")]
	public class UserController : ControllerBase
	{
		#region Private Fields
		private readonly IManagePersona _managePersona;
		private readonly IManagePerson _managePerson;
		private readonly IManageProduct _manageProduct;
		private readonly IManageOrganization _manageOrganization;
		private readonly IManageUnifiedSettings _manageSettings;
		private readonly IProductRepository _productRepository;
		private readonly IUserRepository _userRepository;
		private readonly IManageSecurity _manageSecurityLogic;
		private readonly IManageProductPanel _manageProductPanel;
		private readonly IIntegrationTypeFactory _integrationTypeFactory;
		private readonly IManageUserLogin _userLoginLogic;
		private readonly IManageProductUser _manageProductUser;
		private readonly ISamlRepository _samlRepository;
		private readonly IUserClaimsAccessor _userClaimsAccessor;
		private readonly IManageCustomFields _manageCustomFields;
		private readonly IManageProductOps _manageProductOps;
		private readonly IManageBlueBook manageBlueBook;
		private UserManagement _userManagement;
		private IRepository _repository;
		private IOneSiteProductService _oneSiteProductService;
		private HttpMessageHandler _messageHandler;
		private ManageUser _manageUser;
		#endregion

		#region Constructor
		/// <summary>
		/// Constructor with dependency injection for user controller.
		/// Follows modern ASP.NET Core patterns for testable, maintainable code.
		/// All dependencies are injected as interfaces for loose coupling and testability.
		/// </summary>
		public UserController(
			IManagePersona managePersona,
			IManagePerson managePerson,
			IManageProduct manageProduct,
			IManageOrganization manageOrganization,
			IManageUnifiedSettings manageSettings,
			IProductRepository productRepository,
			IUserRepository userRepository,
			IManageSecurity manageSecurity,
			IManageProductPanel manageProductPanel,
			IIntegrationTypeFactory integrationTypeFactory,
			IManageUserLogin userLoginLogic,
			IManageProductUser manageProductUser,
			ISamlRepository samlRepository,
			IUserClaimsAccessor userClaimsAccessor,
			IManageCustomFields manageCustomFields,
			IManageProductOps manageProductOps,UnifiedLogin.DataAccess.IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims, IOneSiteProductService oneSiteProductService)
		{
			//_managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
			//_managePerson = managePerson ?? throw new ArgumentNullException(nameof(managePerson));
			//_manageProduct = manageProduct ?? throw new ArgumentNullException(nameof(manageProduct));
			//_manageOrganization = manageOrganization ?? throw new ArgumentNullException(nameof(manageOrganization));
			//_manageSettings = manageSettings ?? throw new ArgumentNullException(nameof(manageSettings));
			//_productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
			//_userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
			//_manageSecurityLogic = manageSecurity ?? throw new ArgumentNullException(nameof(manageSecurity));
			//_manageProductPanel = manageProductPanel ?? throw new ArgumentNullException(nameof(manageProductPanel));
			//_integrationTypeFactory = integrationTypeFactory ?? throw new ArgumentNullException(nameof(integrationTypeFactory));
			//_userLoginLogic = userLoginLogic ?? throw new ArgumentNullException(nameof(userLoginLogic));
			//_manageProductUser = manageProductUser ?? throw new ArgumentNullException(nameof(manageProductUser));
			//_samlRepository = samlRepository ?? throw new ArgumentNullException(nameof(samlRepository));
			//_userClaimsAccessor = userClaimsAccessor ?? throw new ArgumentNullException(nameof(userClaimsAccessor));
			//_manageCustomFields = manageCustomFields ?? throw new ArgumentNullException(nameof(manageCustomFields));
			//_manageProductOps = manageProductOps ?? throw new ArgumentNullException(nameof(manageProductOps));
			_repository = repository;
			_messageHandler = messageHandler;
			_oneSiteProductService = oneSiteProductService;
			var productInternalSettingRepository = new ProductInternalSettingRepository(repository);
			var manageBlueBook = new ManageBlueBook(userClaims, repository, productInternalSettingRepository, messageHandler);
			var personaRightRepository = new PersonaRightRepository(null);//TOD):pass Sqlconnection.
			var manageUnifiedLogin = new ManageUnifiedLogin(repository, userClaims, messageHandler);
			var manageProductOneSite = new ManageProductOneSite(repository, userClaims, messageHandler, oneSiteProductService);
			_managePersona = new ManagePersona(repository, userClaims, messageHandler);
			managePerson = new ManagePerson(repository);
			_manageOrganization = new ManageOrganization(repository, userClaims, messageHandler);
			_manageSettings = new ManageUnifiedSettings(repository, userClaims, messageHandler);
			_manageProduct = new ManageProduct(repository, userClaims, messageHandler);
			_manageProductPanel = new ManageProductPanel(userClaims, repository, manageBlueBook, messageHandler, manageProductOneSite);
			_productRepository = new ProductRepository(repository, userClaims);
			_userClaimsAccessor = userClaimsAccessor;
			_userRepository = new UserRepository(repository, userClaims, messageHandler);
			_manageSecurityLogic = new ManageSecurity(personaRightRepository,userClaimsAccessor);
			_integrationTypeFactory = new IntegrationTypeFactory(_manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository, productInternalSettingRepository, userClaims);
			_userManagement = new UserManagement(userClaims);
			_manageUser = new ManageUser(repository, userClaims, messageHandler);
			_userLoginLogic = new ManageUserLogin(repository, userClaims, messageHandler);

			_manageProductUser = new ManageProductUser(repository, userClaims, messageHandler, oneSiteProductService);
			_samlRepository = new SamlRepository(repository);
		}
		#endregion

		#region API Endpoints - Example refactored methods

		/// <summary>
		/// Get User role and asset details
		/// </summary>
		[SwaggerResponse(400, Description = "Bad request")]
		[SwaggerResponse(401, Description = "Unauthorized")]
		[SwaggerResponse(404, Description = "Not Found")]
		[SwaggerResponse(500, Description = "Internal Server Error")]
		[SwaggerResponse(200, Description = "A list of User(s)", Type = typeof(UserRoleAssetDto))]
		[SwaggerResponseExamples(typeof(UserRoleAssetDto), typeof(EnterpriseGetUserRoleAssetExample))]
		[Route("user/{realPageId}/product/{productCode}")]
		[AuthorizeScope("enterpriseapi")]
		[HttpGet]
		public ActionResult GetUserRoleAsset(Guid realPageId, string productCode)
		{
			var userRoleAssetDto = new UserRoleAssetDto();
			var userRoleAssetDtoList = new List<UserRoleAssetDto>();
			var response = new PagedResponse { Meta = new Meta() };
			var error = new ErrorResponse { Errors = new List<Error>() };

			// Get person information
			var person = _managePerson.GetPerson(realPageId);
			if (person == null)
			{
				return NotFound();
			}

			// Get persona for the user in the current organization
			var persona = _managePersona.GetFirstAvailablePersonaByCompany(
				realPageId,
				_userClaimsAccessor.OrganizationPartyId);

			// Verify user belongs to same company
			if (persona == null || persona.OrganizationPartyId != _userClaimsAccessor.OrganizationPartyId)
			{
				return NotFound();
			}

			// Get product information
			var products = _productRepository.GetAllProducts();
			var productId = ProductEnumHelper.GetProductIdByProductCode(productCode, products);

			ListResponse listResponse = null;

			switch (productId)
			{
				case (int)ProductEnum.OpsBuyer:
					var productList = _samlRepository.ListActiveProductsByPersonaId(
						persona.PersonaId,
						(int)ProductEnum.OpsBuyer,
						null);

					if (productList.Any(p => p.ProductStatus == (int)ProductBatchStatusType.Success))
					{
						listResponse = _manageProductOps.GetRoles(
							_userClaimsAccessor.PersonaId,
							persona.PersonaId,
							"",
							null);

						userRoleAssetDto.ProductRole = listResponse.Records
							.Cast<ProductRole>()
							.Where(p => p.IsAssigned)
							.ToList();

						listResponse = _manageProductOps.GetCompanyAssets(
							_userClaimsAccessor.PersonaId,
							persona.PersonaId,
							false,
							null);

						userRoleAssetDto.AssetGroups = listResponse.Records
							.Cast<AssetGroup>()
							.Where(p => p.IsAssigned)
							.ToList();

						userRoleAssetDtoList.Add(userRoleAssetDto);
					}
					break;

				default:
					error.Errors.Add(new Error
					{
						Title = "Bad request",
						Detail = "No valid product code could be found",
						Source = "/user",
						StatusCode = ""
					});
					return BadRequest(error);
			}

			if (listResponse != null && !listResponse.IsError)
			{
				response.Data = userRoleAssetDtoList.Cast<object>().ToList();
				response.Meta.CurrentPage = 1;
				response.Meta.TotalRows = userRoleAssetDtoList.Count;
				response.Meta.RowsPerPage = userRoleAssetDtoList.Count;
				return Ok(response);
			}

			error.Errors.Add(new Error
			{
				Title = "Error",
				Detail = listResponse?.ErrorReason,
				Source = "/user",
				StatusCode = ""
			});
			return BadRequest(error);
		}

		/// <summary>
		/// List User Custom Fields
		/// </summary>
		[SwaggerResponse(401, Description = "Unauthorized")]
		[SwaggerResponse(500, Description = "Internal Server Error")]
		[SwaggerResponse(200, Description = "Gets the User Custom Fields", Type = typeof(ICustomFieldValue))]
		[SwaggerResponseExamples(typeof(ICustomFieldValue), typeof(UserCustomFieldsExample))]
		[Route("customfieldsmaster")]
		[AuthorizeScope("enterpriseapi")]
		[HttpGet]
		public ActionResult UserCustomFields(long? userLoginPersonaId = null)
		{
			var customFieldValueList = _manageCustomFields.GetCustomFieldsValues(
				organizationPartyId: _userClaimsAccessor.OrganizationPartyId,
				userLoginPersonaId: userLoginPersonaId,
				enabled: true);

			var response = new ListResponse
			{
				Records = customFieldValueList.Cast<object>().ToList(),
				TotalRows = customFieldValueList.Count,
				RowsPerPage = customFieldValueList.Count,
				ErrorReason = string.Empty,
				TotalPages = 1
			};

			return Ok(response);
		}

		/// <summary>
		/// Get Saml product attributes by ProductId
		/// </summary>
		[SwaggerResponse(401, Description = "Unauthorized")]
		[SwaggerResponse(500, Description = "Internal Server Error")]
		[SwaggerResponse(200, Description = "Get information about the user", Type = typeof(SamlProductAttributes))]
		[SwaggerResponseExamples(typeof(SamlProductAttributes), typeof(SamlProductAttributesExample))]
		[Authorize]
		[Route("user/productuser/attributes")]
		[HttpGet]
		public IList<SamlProductAttributes> GetSamlProductAttributes(int ProductId)
		{
			return _samlRepository.GetSamlProductAttributes(ProductId);
		}

		/// <summary>
		/// Update details for a Realpage product user
		/// </summary>
		[SwaggerResponse(401, Description = "Unauthorized")]
		[SwaggerResponse(500, Description = "Internal Server Error")]
		[SwaggerResponse(200, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(400, Description = "Bad request")]
		[Route("user/productuser/details")]
		[AuthorizeScope("internalapi")]
		[HttpPut]
		public ActionResult UpdateProductUserAccountDetails([FromBody] ProductUserAccountDetails productUser)
		{
			if (productUser == null)
			{
				return BadRequest("productUser null.");
			}

			if (productUser.ProductId <= 0)
			{
				return BadRequest("ProductName empty.");
			}

			var result = _manageProductUser.UpdateProductUserAccountDetails(productUser, true);
			return Ok(string.IsNullOrEmpty(result) ? "Success" : result);
		}

		/// <summary>
		/// Delete details for a Realpage product user
		/// </summary>
		[SwaggerResponse(401, Description = "Unauthorized")]
		[SwaggerResponse(500, Description = "Internal Server Error")]
		[SwaggerResponse(200, Description = "Update successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(400, Description = "Bad request")]
		[Route("user/productuser/details")]
		[AuthorizeScope("internalapi")]
		[HttpDelete]
		public ActionResult DeleteSamlUserProductInfoAndStatus([FromBody] ProductUserAccountDetails productUser)
		{
			if (productUser == null)
			{
				return BadRequest("productUser null.");
			}

			if (productUser.ProductId <= 0)
			{
				return BadRequest("ProductName empty.");
			}

			var result = _manageProductUser.DeleteSamlUserProductInfoAndStatus(productUser, true);
			return Ok(string.IsNullOrEmpty(result) ? "Success" : result);
		}

		/// <summary>
		/// Gets the list of rights for the current authenticated user
		/// </summary>
		[SwaggerResponse(401, Description = "Unauthorized")]
		[SwaggerResponse(500, Description = "Internal Server Error")]
		[SwaggerResponse(200, Description = "Get the users UnifiedLogin rights")]
		[Route("user/rights/current")]
		[HttpGet]
		[AuthorizeScope("userinfoapi", "landingapi")]
		public ActionResult GetCurrentUserRights()
		{
			var userClaim = _userClaimsAccessor.GetUserClaim();
			return Ok(userClaim.Rights);
		}

		#endregion

		#region Private Helper Methods

		/// <summary>
		/// Used to write to the central log
		/// </summary>
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

		#region Swagger Examples

		[ExcludeFromCodeCoverage]
		public class SamlProductAttributesExample : IProvideExamples
		{
			public object GetExamples()
			{
				return new List<SamlProductAttributes>
				{
					new SamlProductAttributes
					{
						DisplayName = "Username",
						ProductID = 1,
						SamlAttributeName = "productUsername"
					},
					new SamlProductAttributes
					{
						DisplayName = "PMC ID",
						ProductID = 1,
						SamlAttributeName = "PMCID"
					}
				};
			}
		}

		[ExcludeFromCodeCoverage]
		public class UserCustomFieldsExample : IProvideExamples
		{
			public object GetExamples()
			{
				var customFieldValueList = new List<CustomFieldValue>
				{
					new CustomFieldValue
					{
						FieldValueId = 1,
						UserLoginPersonaId = 1,
						Value = "12345",
						FieldId = 15,
						OrganizationId = 350,
						Enabled = true,
						Name = "Employee ID",
						FieldTypeId = 1,
						FieldTypeName = "Alphanumeric",
						Required = false,
						ReadOnly = false,
						Sequence = 1,
						MinCharLength = 1,
						MaxCharLength = 10
					}
				};

				return new ListResponse
				{
					Records = customFieldValueList.Cast<object>().ToList(),
					TotalRows = customFieldValueList.Count,
					RowsPerPage = customFieldValueList.Count,
					ErrorReason = string.Empty,
					TotalPages = 1
				};
			}
		}
		/// <summary>
		/// 
		/// </summary>

		[ExcludeFromCodeCoverage]
		public class EnterpriseGetUserRoleAssetExample : IProvideExamples
		{
			public object GetExamples()
			{
				var userRoleAssetDtoList = new List<UserRoleAssetDto>
				{
					new UserRoleAssetDto
					{
						ProductRole = new List<ProductRole>
						{
							new ProductRole
							{
								ID = "15088",
								Name = "Marketplace Administrator",
								IsAssigned = true,
								isEditorHasRight = false,
								Roletype = "1"
							}
						},
						AssetGroups = new List<AssetGroup>
						{
							new AssetGroup
							{
								ID = "1125",
								Name = "[G] CF Real Estate Services",
								Status = "active",
								GroupType = "company",
								AssetID = "204955",
								IsAssigned = true
							}
						}
					}
				};

				return new PagedResponse
				{
					Meta = new Meta { TotalRows = 1, CurrentPage = 1, RowsPerPage = 1 },
					Data = userRoleAssetDtoList.Cast<object>().ToList()
				};
			}
		}

		#endregion
	}
}
