using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Factory;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.ProductImplementation;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Exceptions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Attributes;
using System.Web.Http.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product
{
	/// <summary>
	/// 
	/// </summary>
	public class ProductInvokerController : BaseApiController
	{
		private IntegrationTypeFactory _integrationTypeFactory;

		private IProductRepository _productRepository;

		protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);

			var manageProduct = new ManageProduct(_userClaims);
			var manageUnifiedLogin = new ManageUnifiedLogin(_userClaims);
			var manageProductOneSite = new ManageProductOneSite(_userClaims);
			var productInternalSettingRepository = new ProductInternalSettingRepository();

			_productRepository = new ProductRepository(_userClaims);

			_integrationTypeFactory = new IntegrationTypeFactory(manageProduct, manageUnifiedLogin, manageProductOneSite, _productRepository,
				productInternalSettingRepository, _userClaims);
		}

        /// <summary>
        /// Returns Roles for given product and user
        /// </summary>
        /// <param name="editorPersonaId">Editor user persona Id</param>
        /// <param name="subjectPersonaId">Subject user persona id</param>
        /// <param name="productType">Product Type</param>
        /// <param name="dataFilter">A dataFilter used to filter the roles.</param>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Operation successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")
		]
		[Route("products/roles")]
		[HttpGet]
		public HttpResponseMessage GetRoles(ProductEnum productType, long editorPersonaId, long subjectPersonaId,
			[FromUri] RequestParameter dataFilter)
		{
			ListResponse result;
			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				if (_realpageUserId == Guid.Empty)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

				int productId = (int)productType;
				var integrationType = _integrationTypeFactory.GetIntegration(productId);
				result = integrationType.GetRoles(editorPersonaId, subjectPersonaId, 0, null, dataFilter);

				if (result.IsError)
					Request.CreateResponse(HttpStatusCode.Forbidden, result);

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch (Exception ex)
			{
				if (ex.InnerException is BlueBookException)
				{
					result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
				}
				else
				{
					result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
				}
			}

			return Request.CreateResponse(HttpStatusCode.Forbidden, result);
		}

        /// <summary>
		/// Returns Rights for given roleid and user
		/// </summary>
		/// <param name="editorPersonaId">Editor user persona Id</param>
		/// <param name="subjectPersonaId">Subject user persona id</param>
		/// <param name="productType">Product Type</param>
        /// <param name="roleId">Role unique Id</param>
		/// <param name="dataFilter">A dataFilter used to filter the roles.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Operation successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description =
             "Bad request(when data filter have invalid entries / when information is out of sync with the server)")
        ]
        [Route("products/role/rights")]
		[HttpGet]
        public HttpResponseMessage GetRightsForRole(ProductEnum productType, long editorPersonaId, long subjectPersonaId, string roleId,
            [FromUri] RequestParameter dataFilter)
        {
			// TODO: This method doesn't appear to be in use
            ListResponse result;
			int productRoleId;
			try
            {
                if (editorPersonaId == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				if (String.IsNullOrWhiteSpace(roleId) || roleId == "0")
					return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");

				if (_realpageUserId == Guid.Empty)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

				int productId = (int)productType;
				var integrationType = _integrationTypeFactory.GetIntegration(productId);
				if (_integrationTypeFactory.GetIntegrationTypeForProductId(productId) == ProductIntegrationTypeEnum.StandardV1)
				{
					result = integrationType.GetRightsForRole(editorPersonaId, subjectPersonaId, roleId, 0, false, dataFilter);
				}
				else
				{
					result = integrationType.GetRightsForRole(editorPersonaId, subjectPersonaId, Int32.Parse(roleId), 0, false, dataFilter);			
				}
				
				
				if (result.IsError)
                    Request.CreateResponse(HttpStatusCode.Forbidden, result);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                if (ex.InnerException is BlueBookException)
                {
                    result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
                }
                else
                {
                    result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
                }
            }

            return Request.CreateResponse(HttpStatusCode.Forbidden, result);
        }
					   		 	  
		/// <summary>
		///  Returns Rights for given company and user
		/// </summary>
		/// <param name="editorPersonaId">Editor user persona Id</param>
		/// <param name="subjectPersonaId">Subject user persona id</param>
		/// <param name="productType">Product Type</param>
		/// <param name="dataFilter">A dataFilter used to filter the roles.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Operation successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")
		]
		[Route("products/company/rights")]
		[HttpGet]
		public HttpResponseMessage GetAllRights(ProductEnum productType, long editorPersonaId, long subjectPersonaId, [FromUri] RequestParameter dataFilter)
		{
			// TODO: This endpoint appears to not be in use
			ListResponse result;
			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				if (_realpageUserId == Guid.Empty)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

				int productId = (int)productType;
				var integrationType = _integrationTypeFactory.GetIntegration(productId);
				result = integrationType.GetAllRights(editorPersonaId, subjectPersonaId, dataFilter);

				if (result.IsError)
					Request.CreateResponse(HttpStatusCode.Forbidden, result);

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch (Exception ex)
			{
				if (ex.InnerException is BlueBookException)
				{
					result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
				}
				else
				{
					result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
				}
			}

			return Request.CreateResponse(HttpStatusCode.Forbidden, result);
		}
			   		 	  	  	   
		/// <summary>
		/// Returns properties
		/// </summary>
		/// <param name="editorPersonaId">Editor user persona Id</param>
		/// <param name="subjectPersonaId">Subject user persona id</param>
		/// <param name="productType">Product Type</param>
		/// <param name="dataFilter">A dataFilter used to filter the roles.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Operation successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/properties")]
		[HttpGet]
		public HttpResponseMessage GetProperties(ProductEnum productType, long editorPersonaId, long subjectPersonaId,
			[FromUri] RequestParameter dataFilter)
		{
			ListResponse result;
			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				if (_realpageUserId == Guid.Empty)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

				int productId = (int)productType;
				var integrationType = _integrationTypeFactory.GetIntegration(productId);
				result = integrationType.GetProperties(editorPersonaId, subjectPersonaId, dataFilter);

				if (result.IsError)
					Request.CreateResponse(HttpStatusCode.Forbidden, result);

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch (Exception ex)
			{
				if (ex.InnerException is BlueBookException)
				{
					result = new ListResponse
					{
						IsError = true,
						ErrorReason = ex.InnerException.Message
					};
				}
				else
				{
					result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
				}
			}

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns product property groups / regions
		/// </summary>
		/// <param name="editorPersonaId">Editor user persona Id</param>
		/// <param name="subjectPersonaId">Subject user persona id</param>
		/// <param name="productType">Product Type</param>
		/// <param name="dataFilter">A dataFilter used to filter the roles.</param>
		/// <param name="tabName">Tab Name</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Operation successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/propertygroups")]
		[HttpGet]
		public HttpResponseMessage GetPropertyGroups(ProductEnum productType, long editorPersonaId, long subjectPersonaId,
			[FromUri] RequestParameter dataFilter , string tabName = null)
		{
			ListResponse result;
			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				if (_realpageUserId == Guid.Empty)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

				int productId = (int)productType;
				var integrationType = _integrationTypeFactory.GetIntegration(productId);
				result = integrationType.GetPropertyGroups(editorPersonaId, subjectPersonaId, dataFilter);

				if (result.IsError)
                {
					if (!string.IsNullOrEmpty(tabName))
					{
						if (tabName == TabEnum.Area.ToString())
						{
							result.ErrorReason = CommonMessageConstants.AreaErrorMessage;
						}

						if (tabName == TabEnum.Region.ToString())
						{
							result.ErrorReason = CommonMessageConstants.RegionErrorMessage;
						}
					}

					Request.CreateResponse(HttpStatusCode.Forbidden, result);
				}

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch (Exception ex)
			{
				if (ex.InnerException is BlueBookException)
				{
					result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
				}
				else
				{
					result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
				}
			}

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Returns Properties for given property or region group
		/// </summary>
		/// <param name="editorPersonaId">Editor user persona Id</param>
		/// <param name="subjectPersonaId">Subject user persona id</param>
		/// <param name="productType">Product Type</param>
		/// <param name="groupId">Group or region Id</param>
		/// <param name="dataFilter">A dataFilter used to filter the roles.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Operation successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[ProductAllowed(ProductEnum.LeadAnalytics)]
		[Route("products/properties/{groupId}")]
		[HttpGet]
		public HttpResponseMessage GetPropertiesByGroup(ProductEnum productType, long editorPersonaId, long subjectPersonaId, string groupId,
			[FromUri] RequestParameter dataFilter)
		{
			ListResponse result;
			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				if (_realpageUserId == Guid.Empty)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

				if (string.IsNullOrEmpty(groupId))
					return Request.CreateResponse(HttpStatusCode.BadRequest, "Group Id is required.");

				int productId = (int)productType;
				var integrationType = _integrationTypeFactory.GetIntegration(productId);
				result = integrationType.GetPropertiesByGroup(editorPersonaId, subjectPersonaId, groupId, dataFilter);

				if (result.IsError)
					Request.CreateResponse(HttpStatusCode.Forbidden, result);

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch (Exception ex)
			{
				if (ex.InnerException is BlueBookException)
				{
					result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
				}
				else
				{
					result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
				}
			}

			return Request.CreateResponse(HttpStatusCode.Forbidden, result);
		}

		/// <summary>
		/// For a product, returns all organizations or by given organizationId (Used in ClickPay)
		/// </summary>
		/// <param name="editorPersonaId">Editor user persona Id</param>
		/// <param name="subjectPersonaId">Subject user persona id</param>
		/// <param name="organizationType">Organization type- site, owner, company etc</param>
		/// <param name="productType">Product Type</param> 
		/// <param name="dataFilter">A dataFilter used to filter the roles.</param>
		/// <param name="organizationRoleId">Role id for organization</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Operation successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/organizations/")]
		[HttpGet]
		public HttpResponseMessage GetProductOrganizations(string organizationRoleId, string organizationType, ProductEnum productType, long editorPersonaId, long subjectPersonaId)
		{
			ListResponse result;

			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				if (_realpageUserId == Guid.Empty)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

				int productId = (int)productType;
				var integrationType = _integrationTypeFactory.GetIntegration(productId);
				result = integrationType.GetOrganizations(editorPersonaId, subjectPersonaId, organizationRoleId, organizationType);

				if (result.IsError)
					Request.CreateResponse(HttpStatusCode.Forbidden, result);

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch (Exception ex)
			{
				if (ex.InnerException is BlueBookException)
				{
					result = new ListResponse { IsError = true, ErrorReason = ex.InnerException.Message };
				}
				else
				{
					result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
				}
			}

			return Request.CreateResponse(HttpStatusCode.Forbidden, result);
		}

		/// <summary>
		/// Returns Properties for given property or region group
		/// </summary>
		/// <param name="editorPersonaId">Editor user persona Id</param>
		/// <param name="productType">Product Books Code</param>
		/// <param name="dataFilter">A datafilter used to filter the roles.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List users", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/users/migration")]
		[HttpGet]
		public HttpResponseMessage ListMigrationUsers(string productType, long editorPersonaId, [FromUri] RequestParameter dataFilter)
		{
			ListResponse result;
			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				var productList = _productRepository.GetAllProducts();
				int productId = ProductEnumHelper.GetProductIdByProductCode(productType, productList);

				var integrationType = _integrationTypeFactory.GetIntegration(productId);
				result = integrationType.GetMigrationUsers(editorPersonaId, dataFilter); 

                if (result.IsError)
					return Request.CreateResponse(HttpStatusCode.Forbidden, result);

				return Request.CreateResponse(HttpStatusCode.OK, result);
			}
			catch (Exception ex)
			{
				if (ex.InnerException is BlueBookException)
				{
					result = new ListResponse
					{
						IsError = true,
						ErrorReason = ex.InnerException.Message
					};
				}
				else
				{
					result = new ListResponse { IsError = true, ErrorReason = "Internal server error." };
				}
			}

			return Request.CreateResponse(HttpStatusCode.Forbidden, result);
		}

		/// <summary>
		/// Returns Properties for given property or region group
		/// </summary>
		/// <param name="productType">Product Type</param>
		/// <param name="migrateUsers">A datafilter used to filter the roles.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Migrated successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/users/migrate")]
		[HttpPatch]
		public HttpResponseMessage UpdateUsersMigrationStatus([FromUri]string productType, [FromBody]IList<MigrateUser> migrateUsers)
		{
			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var productList = _productRepository.GetAllProducts();
			int productId = ProductEnumHelper.GetProductIdByProductCode(productType, productList);

			var integrationType = _integrationTypeFactory.GetIntegration(productId);
			var result = integrationType.UpdateUsersMigrationStatus(_personaId, migrateUsers);

			if (!result.Status)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Used to change product user profile without requiring green-book.
		/// Direct call to product to change profile including isActive (mainly used to
		/// activate-deactivate from Migration tool)
		/// </summary>
		/// <param name="productType">Product Code</param>
		/// <param name="productUserProfile">Product user profile.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "User profile changed successfully", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[Route("products/users/externalprofilechange")]
		[HttpPatch]
		public HttpResponseMessage ExternalProductUserProfileChange([FromUri]string productType, [FromBody]ProductUserProfile productUserProfile)
		{
			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var productList = _productRepository.GetAllProducts();
			int productId = ProductEnumHelper.GetProductIdByProductCode(productType, productList);

			var integrationType = _integrationTypeFactory.GetIntegration(productId);
			var result = integrationType.ExternalUserProfileChange(_personaId, productUserProfile);

			if (result)
				return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");

			return Request.CreateResponse(HttpStatusCode.Forbidden, "Failed to disabled product user.");
		}
	}
}

