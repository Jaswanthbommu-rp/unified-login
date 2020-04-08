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

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers.Product
{
	/// <summary>
	/// 
	/// </summary>
	public class ProductInvokerController : BaseApiController
	{
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
		[ProductAllowed(ProductEnum.LeadManagement, ProductEnum.LeadAnalytics, ProductEnum.PortfolioManagement,
			ProductEnum.ClickPay, ProductEnum.DepositAlternative, ProductEnum.SeniorLeadManagement)]
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

				var productLogic =
					ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, _userClaims);
				result = productLogic.GetProductRoles(dataFilter);

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
        [ProductAllowed(ProductEnum.LeadManagement, ProductEnum.LeadAnalytics, ProductEnum.PortfolioManagement,
            ProductEnum.ClickPay, ProductEnum.DepositAlternative, ProductEnum.SeniorLeadManagement)]
        [Route("products/role/rights")]
        [HttpGet]
        public HttpResponseMessage GetRightsForRole(ProductEnum productType, long editorPersonaId, long subjectPersonaId, long roleId,
            [FromUri] RequestParameter dataFilter)
        {
            ListResponse result;
            try
            {
                if (editorPersonaId == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

                if (roleId == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "roleId not supplied.");

                if (_realpageUserId == Guid.Empty)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

                var productLogic =
                    ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, _userClaims);
                result = productLogic.GetProductRightsForRole(dataFilter, roleId);

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
		[ProductAllowed(ProductEnum.LeadAnalytics, ProductEnum.LeadManagement, ProductEnum.PortfolioManagement, ProductEnum.DepositAlternative
            , ProductEnum.SeniorLeadManagement)]
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

				var productLogic = ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, _userClaims);
				result = productLogic.GetProductProperties(dataFilter);

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
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Operation successful", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[ProductAllowed(ProductEnum.LeadAnalytics, ProductEnum.DepositAlternative)]
		[Route("products/propertygroups")]
		[HttpGet]
		public HttpResponseMessage GetPropertyGroups(ProductEnum productType, long editorPersonaId, long subjectPersonaId,
			[FromUri] RequestParameter dataFilter)
		{
			ListResponse result;
			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				if (_realpageUserId == Guid.Empty)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

				var productLogic = ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, _userClaims);
				result = productLogic.GetProductPropertyGroups(dataFilter);

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

				var productLogic = ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, _userClaims);
				result = productLogic.GetProductPropertiesByGroup(groupId, dataFilter);

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
		[ProductAllowed(ProductEnum.ClickPay)]
		[Route("products/organizations/")]
		[HttpGet]
		public HttpResponseMessage GetProductOrganizations(string organizationRoleId, string organizationType, ProductEnum productType, long editorPersonaId, long subjectPersonaId,
			[FromUri] RequestParameter dataFilter)
		{
			ListResponse result;

			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				if (_realpageUserId == Guid.Empty)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

				var productLogic = ManageProductFactory.GetProductLogic(productType, editorPersonaId, subjectPersonaId, _userClaims);
				result = productLogic.GetProductOrganizations(organizationRoleId, organizationType, dataFilter);

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
		/// <param name="productType">Product Type</param>
		/// <param name="datafilter">A datafilter used to filter the roles.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "List users", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description =
			 "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
		[Route("products/users/migration")]
		[HttpGet]
		public HttpResponseMessage ListMigrationUsers(ProductEnum productType, long editorPersonaId, [FromUri] RequestParameter datafilter)
		{
			ListResponse result;
			try
			{
				if (editorPersonaId == 0)
					return Request.CreateResponse(HttpStatusCode.BadRequest, "editorPersonaId not supplied.");

				var productLogic = ManageProductFactory.GetProductLogic(productType, editorPersonaId, editorPersonaId, _userClaims);
				result = productLogic.GetMigrationUsers(datafilter);

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
		public HttpResponseMessage UpdateUsersMigrationStatus([FromUri]ProductEnum productType, [FromBody]IList<MigrateUser> migrateUsers)
		{
			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var productLogic = ManageProductFactory.GetProductLogic(productType, _personaId, _personaId, _userClaims);
			var result = productLogic.UpdateUsersMigrationStatus(migrateUsers);

			if (!result.Status)
				Request.CreateResponse(HttpStatusCode.Forbidden, result);

			return Request.CreateResponse(HttpStatusCode.OK, result);
		}

		/// <summary>
		/// Used to change product user profile without requiring green-book.
		/// Direct call to product to change profile including isActive (mainly used to
		/// activate-deactivate from Migration tool)
		/// </summary>
		/// <param name="productType">Product Type</param>
		/// <param name="productUserProfile">Product user profile.</param>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "User profile changed successfully", Type = typeof(HttpResponseMessage))]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
		[Route("products/users/externalprofilechange")]
		[HttpPatch]
		public HttpResponseMessage ExternalProductUserProfileChange([FromUri]ProductEnum productType, [FromBody]ProductUserProfile productUserProfile)
		{
			if (_realpageUserId == Guid.Empty)
				return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

			var productLogic = ManageProductFactory.GetProductLogic(productType, _personaId, _personaId, _userClaims);
			var result = productLogic.ExternalProductUserProfileChange(productUserProfile);

			if (result)
				return Request.CreateResponse(HttpStatusCode.OK, "Successfully disabled product user.");

			return Request.CreateResponse(HttpStatusCode.Forbidden, "Failed to disabled product user.");
		}
	}
}

