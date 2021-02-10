using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Saml;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	public class SamlController : BaseApiController
    {
        /// <summary>
        /// Get a list of products information by a user persona
        /// </summary>
        /// <param name="PersonaId">A Persona for a logged in user</param>
        /// <param name="ProductId">A Product Id</param>
        /// <param name="ProductType">NULL, ProductWithFavorites, IsResource, IsFavorite</param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the user", Type = typeof(PersonaProductUserDetails))]
        [SwaggerResponseExamples(typeof(PersonaProductUserDetails), typeof(PortfolioProductUserDetailsExample))]
        [Authorize]
        [Route("saml/persona/product")]
        [HttpGet]
        //[Route("saml/getproductsbypersonaid")]
        public IList<PersonaProductUserDetails> ListProductsByPersonaId(long PersonaId, int ProductId = 0, string ProductType = null) //TODO:rename Portfolio to Organization everywhere; change userId to enterpriseUniqueId
        {
            var samlRepository = new SamlRepository();
            return samlRepository.ListAllProductsByPersonaId(PersonaId, ProductId, ProductType);
        }

		/// <summary>
		/// Get Saml attributes by PersonaId and ProductId
		/// </summary>
		/// <param name="PersonaId"></param>
		/// <param name="ProductId"></param>
		/// <returns>List of Saml Attributes</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the user", Type = typeof(SamlAttributes))]
        [SwaggerResponseExamples(typeof(SamlAttributes), typeof(SamlAttributesExample))]
        [Authorize]
        [Route("saml/persona/product/attributes")]
        [HttpGet]
        //[Route("saml/getproductsamldetails")]
        public IList<SamlAttributes> GetProductSamlDetails(long PersonaId, int ProductId) //TODO:rename Portfolio to Organization everywhere; check portfolioProductUserId for int vs Guid
        {
            var samlRepository = new SamlRepository();
            return samlRepository.GetProductSamlDetails(PersonaId, ProductId);
        }

        /// <summary>
		/// Get Persona Products Saml attributes  
		/// </summary>
		/// <param name="personaId"></param>
		/// <returns>List of Saml Attributes</returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the user", Type = typeof(ProductSamlDetails))]
        [SwaggerResponseExamples(typeof(ProductSamlDetails), typeof(PersonaProductsSamlAttributesExample))]
        [Authorize]
        [Route("saml/persona/{personaId}/attributes")]
        [HttpGet]
        public IList<ProductSamlDetails> GetPersonaProductSamlDetails([FromUri] long personaId) 
        {
            var samlRepository = new SamlRepository();
            return samlRepository.ListPersonaProductsSamlDetails(personaId);
        }

        /// <summary>
        /// Get Product Saml settings by product id
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the user", Type = typeof(ProductSamlSettings))]
        [SwaggerResponseExamples(typeof(ProductSamlSettings), typeof(ProductSamlSettingsExample))]
        [Authorize]
        [Route("saml/product/setting")]
        [HttpGet]
        //[Route("saml/getproductsamlsettingsbyproductid")]
        public ProductSamlSettings GetProductSamlSettingsByProductId(int productId)
        {
            var samlRepository = new SamlRepository();
            return samlRepository.GetProductSamlSettingsByProductId(productId);
        }

		/// <summary>
		/// Used to update the Value of a SAML attribute for the given SamlUserAttributeId
		/// </summary>
		/// <param name="samlAttributes">SamlAttributes object of the parameter values</param>
		/// <returns>RepositoryResponse object</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "SamlUserAttribute Updated")]
		[Route("saml/persona/product/attributes")]
		[HttpPut]
		public RepositoryResponse UpdateSamlUserAttribute(SamlAttributes samlAttributes)
		{
			RepositoryResponse repositoryResponse = new RepositoryResponse();
			IManageSaml samlLogic = new ManageSaml();
			repositoryResponse = samlLogic.UpdateSamlUserAttribute(samlAttributes);
			return repositoryResponse;
		}

		#region Get Examples
		/// <summary>
		/// Used to document examples of the PortfolioProductUserDetails webapi result
		/// </summary>
		public class PortfolioProductUserDetailsExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>User List example</returns>
            public object GetExamples()
            {
                PersonaProductUserDetails example = new PersonaProductUserDetails()
                {
					PersonaId = 6,
					OrganizationPartyId = 1,
					OrganizationName = "Product",
					ProductId = 1,
					ProductName = "Product name",
					ProductDescription = "Description",
					PersonPartyId = 2,
					TotalAccounts = 4,
					ClientId = "101",
					ClassName = "product",
					SettingsUrl = "/settings",
					ProductUrl = "https://producturl",
					TitleId = "OneSite",
					TitleUniqueId = new Guid("CCFE2F2B-BE0B-4075-B673-110F683E51C4"),
					IsNewTab = false,
                    MetatagUniqueId = "Product",
					IsResource = false,
					IsFavorite = true,
					Subsolution = ""
                };

                return example;
            }
        }

        /// <summary>
        /// Used to document examples of the Product Saml setting webapi result
        /// </summary>
        public class SamlAttributesExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>User List example</returns>
            public object GetExamples()
            {
                List<SamlAttributes> example = new List<SamlAttributes>();
                example.Add(new SamlAttributes()
                {
                    Name = "userid",
                    SamlAttributeId = 1,
                    Type = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic",
                    Value = "6"
                });
                example.Add(new SamlAttributes()
                {
                    Name = "RealPageIDProductsRaw",
                    SamlAttributeId = 5,
                    Type = "&lt;SSOServiceProviders&gt;&lt;/SSOServiceProviders&gt;",
                    Value = "6"
                });

                return example;
            }
        }

        /// <summary>
        /// Used to document examples of the Product Saml setting webapi result
        /// </summary>
        public class PersonaProductsSamlAttributesExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>User List example</returns>
            public object GetExamples()
            {
                List<ProductSamlDetails> example = new List<ProductSamlDetails>();
                example.Add(new ProductSamlDetails()
                {
                    ProductId = 1,
                    ProductName = "OneSite",
                    ProductDescription = "One Site",
                    ProductStatus = "Success",
                    UserID = "6",
                    ProductUserName = "test",
                    PMCID ="12344",
                    RoleType = "",
                    PortalId = "",
                    OrganizationId="",
                    NWPUserType=""
                });
                example.Add(new ProductSamlDetails()
                {
                    ProductId = 18,
                    ProductName = "Utility Management",
                    ProductDescription = "RUM",
                    ProductStatus = "Success",
                    UserID = "1116",
                    ProductUserName = "test1",
                    PMCID = "",
                    RoleType = "",
                    PortalId = "",
                    OrganizationId = "",
                    NWPUserType = "PR"
                });

                return example;
            }
        }

        /// <summary>
        /// Used to document examples of the Product Saml setting webapi result
        /// </summary>
        public class ProductSamlSettingsExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>User List example</returns>
            public object GetExamples()
            {
                ProductSamlSettings example = new ProductSamlSettings()
                {
                     ProductId = 1,
                     LoginUri = "https://someproducturl/loginsaml",
                     ProductSamlSettingsId = 4,
                     SigningCertificateThumbprint = "12KLJ12LJK12J12LADAD002923923090A9DS09121L2K",
                     SubjectIdSamlAttribute = "Userid"
                };
                
                return example;
            }
        }
        #endregion
    }
}

