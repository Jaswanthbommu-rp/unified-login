using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Product;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Controller for product related APIs
	/// </summary>
	public class ProductUserController : BaseApiController
    {
        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public ProductUserController() : base() { }

        #endregion


        #region Public methods

        /// <summary>
        /// Used to create a new Realpage product (One site, AO etc) user for the given GreenBook user
        /// </summary>
        /// <param name="productUser">Details to send to Realpage product for a user</param> 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("productuser/user")]
        [AllowAnonymous]//TODO: Make it authorize by having client id for Windows Servive in ID server
        [HttpPost]
        public HttpResponseMessage CreateProductUser(ProductUserProperitiesRoles productUser)
        {
            if (productUser == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "productUser null.");

            if (productUser.RealPageId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            ManageProductUser manageProduct = new ManageProductUser(_userClaims);
            string result = manageProduct.CreateProductUser(productUser);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Request.CreateResponse(HttpStatusCode.Created, result);
        }

        /// <summary>
        /// Used to update details for a Realpage product (OneSite, Accounting, VendorServices) user for the given GreenBook user
        /// </summary>
        /// <param name="productUser">Details to save for a user</param> 
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("productuser/details")]
        [HttpPut]
        public HttpResponseMessage UpdateProductUserAccountDetails([FromBody] ProductUserAccountDetails productUser)
        {
            if (productUser == null)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "productUser null.");

            if (productUser.ProductId <= 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "ProductName empty.");

            ManageProductUser manageProduct = new ManageProductUser(_userClaims);
            string result = manageProduct.UpdateProductUserAccountDetails(productUser, true);

            if (string.IsNullOrEmpty(result))
                result = "Success";

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

		/// <summary>
		/// Used to delete all SAML product information and status for a user
		/// </summary>
		/// <param name="productUser">Details to save for a user</param> 
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
	    [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
	    [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
	    [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
	    [Route("productuser/details")]
	    [HttpDelete]
	    public HttpResponseMessage DeleteSamlUserProductInfoAndStatus([FromBody] ProductUserAccountDetails productUser)
	    {
		    if (productUser == null)
			    return Request.CreateResponse(HttpStatusCode.BadRequest, "productUser null.");

            if (productUser.ProductId <= 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "ProductName empty.");

		    ManageProductUser manageProduct = new ManageProductUser(_userClaims);
		    string result = manageProduct.DeleteSamlUserProductInfoAndStatus(productUser, true);

		    if (string.IsNullOrEmpty(result))
			    result = "Success";

		    return Request.CreateResponse(HttpStatusCode.OK, result);
	    }

		/// <summary>
		/// Returns product statues for the given user
		/// </summary>
		/// <param name="assignUserPersonaId">Assign user Id</param> 
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Update successful", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("productuser/status")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetProductStatuses(long assignUserPersonaId)
        {
            if (assignUserPersonaId == 0)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "assignUserPersonaId not supplied.");


            if (_realpageUserId == Guid.Empty)
                return Request.CreateResponse(HttpStatusCode.BadRequest, "RealPageId empty.");

            var manageProduct = new ManageProductUser(_userClaims);
            var result = manageProduct.GetProductStatuses(_realpageUserId, assignUserPersonaId);
            ListResponse output = null;

            if (result?.Count > 0)
            {
                output = new ListResponse()
                {
                    Records = result.Cast<object>().ToList()
                };
            }

            return Request.CreateResponse(HttpStatusCode.OK, output);
        }
        #endregion


        #region Get Examples


        /// <summary>
        /// Used to document examples of the webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class ProductUserAccountDetailsExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>ProductUserAccountDetails example</returns>
            public object GetExamples()
            {
                var output = new ProductUserAccountDetails()
                {
                    PersonaId = 1234,
                    ProductId = (int)Component.SharedObjects.Enum.ProductEnum.OneSite,
                    ProductStatus = Component.SharedObjects.Enum.ProductBatchStatusType.Success,
                    ProductSettings = new Dictionary<SamlAttributeEnum, string>()
                };
                output.ProductSettings.Add(SamlAttributeEnum.UserId, "41323");
                output.ProductSettings.Add(SamlAttributeEnum.PMCID, "1234567");
                output.ProductSettings.Add(SamlAttributeEnum.productUsername, "user name");
                output.ProductSettings.Add(SamlAttributeEnum.eidPersonLogin, "user login");
                output.ProductSettings.Add(SamlAttributeEnum.User_FirstName, "first name");
                output.ProductSettings.Add(SamlAttributeEnum.User_Username, "user name");
                output.ProductSettings.Add(SamlAttributeEnum.User_email, "emailaddress");
                output.ProductSettings.Add(SamlAttributeEnum.portal_id, "portal id");
                output.ProductSettings.Add(SamlAttributeEnum.organization_id, "organization id");

                return output;
            }
        }
        #endregion
    }
}