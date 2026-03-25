using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.Core;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Saml;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// SAML Controller for managing SAML authentication and attributes
    /// Migrated to .NET Core 8.0
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class SamlController : BaseController
    {
        private readonly ISamlRepositoryAsync _samlRepositoryAsync;

        /// <summary>
        /// Constructor
        /// </summary>
        public SamlController(
            IUserClaimsAccessor userClaimsAccessor,
            ISamlRepositoryAsync samlRepositoryAsync) : base(userClaimsAccessor)
        {
            _samlRepositoryAsync = samlRepositoryAsync;
        }

        /// <summary>
        /// Get a list of products information by a user persona
        /// </summary>
        /// <param name="PersonaId">A Persona for a logged in user</param>
        /// <param name="ProductId">A Product Id</param>
        /// <param name="ProductType">NULL, ProductWithFavorites, IsResource, IsFavorite</param>
        /// <returns></returns>
        [HttpGet("saml/persona/product")]
        [ProducesResponseType(typeof(IList<PersonaProductUserDetails>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ListProductsByPersonaId(long PersonaId, int ProductId = 0, string ProductType = null, CancellationToken cancellationToken = default) //TODO:rename Portfolio to Organization everywhere; change userId to enterpriseUniqueId
        {
            var result = await _samlRepositoryAsync.ListAllProductsByPersonaIdAsync(PersonaId, ProductId, ProductType, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get Saml attributes by PersonaId and ProductId
        /// </summary>
        /// <param name="PersonaId"></param>
        /// <param name="ProductId"></param>
        /// <returns>List of Saml Attributes</returns>
        [HttpGet("saml/persona/product/attributes")]
        [ProducesResponseType(typeof(IList<SamlAttributes>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductSamlDetails(long PersonaId, int ProductId, CancellationToken cancellationToken = default) //TODO:rename Portfolio to Organization everywhere; check portfolioProductUserId for int vs Guid
        {
            var result = await _samlRepositoryAsync.GetProductSamlDetailsAsync(PersonaId, ProductId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get Saml product attributes by ProductId
        /// </summary>
        /// <param name="ProductId"></param>
        /// <returns>List of Saml Attributes</returns>
        [HttpGet("saml/productAttributes")]
        [ProducesResponseType(typeof(IList<SamlProductAttributes>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSamlProductAttributes(int ProductId, CancellationToken cancellationToken = default)
        {
            var result = await _samlRepositoryAsync.GetSamlProductAttributesAsync(ProductId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get Persona Products Saml attributes
        /// </summary>
        /// <param name="personaId"></param>
        /// <returns>List of Saml Attributes</returns>
        [HttpGet("saml/persona/{personaId}/attributes")]
        [ProducesResponseType(typeof(SamlUserProductDetails), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetPersonaProductSamlDetails(long personaId, CancellationToken cancellationToken = default)
        {
            var productSamlDetails = await _samlRepositoryAsync.ListPersonaProductsSamlDetailsAsync(personaId, cancellationToken);
            var aoProducts = productSamlDetails.FirstOrDefault(p => p.ProductId == 4);

            // ProductRepository.ListProducts has no async equivalent — kept as direct sync call
            IList<GbProductMap> gbProductMaps = new ProductRepository().ListProducts(null, null, null, null);

            SamlUserProductDetails samlUserProductDetails = new SamlUserProductDetails
            {
                AOProducts = gbProductMaps.Where(p => p.UDMSourceCode == "AO").OrderBy(s => s.Name).ToList()
            };

            if (aoProducts != null)
            {
                aoProducts.Products = new List<ProductDetails>();

                var successStatusProducts = productSamlDetails.Where(p => p.ParentProductTypeId == 400 && p.ProductStatus.Equals("Success", StringComparison.OrdinalIgnoreCase)).ToList();
                foreach (var prod in successStatusProducts)
                {
                    aoProducts.Products.Add(new ProductDetails
                    {
                        ProductId = prod.ProductId,
                        ProductName = prod.ProductName,
                        Status = prod.ProductStatus
                    });
                }

                var aoProductsOrderByStatus = productSamlDetails.Where(p => p.ParentProductTypeId == 400 && !p.ProductStatus.Equals("Success", StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.ProductStatus).ThenBy(x1 => x1.ProductName).ToList();
                foreach (var prod in aoProductsOrderByStatus)
                {
                    aoProducts.Products.Add(new ProductDetails
                    {
                        ProductId = prod.ProductId,
                        ProductName = prod.ProductName,
                        Status = prod.ProductStatus
                    });
                }

                IList<ProductSamlDetails> allAOProducts = productSamlDetails.Where(p => p.ParentProductTypeId == 400).ToList();
                foreach (var item in allAOProducts)
                {
                    productSamlDetails.Remove(item);
                }
            }

            samlUserProductDetails.ProductSamlDetails = productSamlDetails;
            return Ok(samlUserProductDetails);
        }

        /// <summary>
        /// Get Product Saml settings by product id
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpGet("saml/product/setting")]
        [ProducesResponseType(typeof(ProductSamlSettings), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetProductSamlSettingsByProductId(int productId, CancellationToken cancellationToken = default)
        {
            var result = await _samlRepositoryAsync.GetProductSamlSettingsByProductIdAsync(productId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Used to update the Value of a SAML attribute for the given SamlUserAttributeId
        /// </summary>
        /// <param name="samlAttributes">SamlAttributes object of the parameter values</param>
        /// <returns>RepositoryResponse object</returns>
        [HttpPut("saml/persona/product/attributes")]
        [ProducesResponseType(typeof(RepositoryResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> UpdateSamlUserAttribute([FromBody] SamlAttributes samlAttributes, CancellationToken cancellationToken = default)
        {
            var result = await _samlRepositoryAsync.UpdateSamlUserAttributeAsync(samlAttributes, cancellationToken);
            return Ok(result);
        }
    }
}
