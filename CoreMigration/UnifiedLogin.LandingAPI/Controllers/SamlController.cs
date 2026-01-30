using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Attribute;
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
    public class SamlController : ControllerBase
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SamlController()
        {
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
        public async Task<IActionResult> ListProductsByPersonaId(long PersonaId, int ProductId = 0, string ProductType = null) //TODO:rename Portfolio to Organization everywhere; change userId to enterpriseUniqueId
        {
            return await Task.Run<IActionResult>(() =>
            {
                var samlRepository = new SamlRepository();
                var result = samlRepository.ListAllProductsByPersonaId(PersonaId, ProductId, ProductType);
                return Ok(result);
            });
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
        public async Task<IActionResult> GetProductSamlDetails(long PersonaId, int ProductId) //TODO:rename Portfolio to Organization everywhere; check portfolioProductUserId for int vs Guid
        {
            return await Task.Run<IActionResult>(() =>
            {
                var samlRepository = new SamlRepository();
                var result = samlRepository.GetProductSamlDetails(PersonaId, ProductId);
                return Ok(result);
            });
        }

        /// <summary>
        /// Get Saml product attributes by  ProductId
        /// </summary>
        /// <param name="ProductId"></param>
        /// <returns>List of Saml Attributes</returns>
        [HttpGet("saml/productAttributes")]
        [ProducesResponseType(typeof(IList<SamlProductAttributes>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetSamlProductAttributes(int ProductId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var samlRepository = new SamlRepository();
                var result = samlRepository.GetSamlProductAttributes(ProductId);
                return Ok(result);
            });
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
        public async Task<IActionResult> GetPersonaProductSamlDetails(long personaId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var samlRepository = new SamlRepository();
                var productSamlDetails = samlRepository.ListPersonaProductsSamlDetails(personaId);
                var aoProducts = productSamlDetails.FirstOrDefault(p => p.ProductId == 4);
                ProductRepository productRepository = new ProductRepository();
                IList<GbProductMap> gbProductMaps = productRepository.ListProducts(null, null, null, null);
                SamlUserProductDetails samlUserProductDetails = new SamlUserProductDetails
                {
                    AOProducts = gbProductMaps.Where(p => p.UDMSourceCode == "AO").OrderBy(s => s.Name).ToList()
                };
                if (aoProducts != null)
                {
                    aoProducts.Products = new List<ProductDetails>();
                    //Add AO products with success status
                    var successStatusProducts = productSamlDetails.Where(p => p.ParentProductTypeId == 400 && p.ProductStatus.Equals("Success", StringComparison.OrdinalIgnoreCase)).ToList();
                    foreach (var prod in successStatusProducts)
                    {
                        ProductDetails successfulproductDetails = new ProductDetails
                        {
                            ProductId = prod.ProductId,
                            ProductName = prod.ProductName,
                            Status = prod.ProductStatus
                        };
                        aoProducts.Products.Add(successfulproductDetails);
                    }
                    //Add AO Products other then success status with order by status and name
                    var aoProductsOrderByStatus = productSamlDetails.Where(p => p.ParentProductTypeId == 400 && !p.ProductStatus.Equals("Success", StringComparison.OrdinalIgnoreCase)).OrderByDescending(x => x.ProductStatus).ThenBy(x1 => x1.ProductName).ToList();
                    foreach (var prod in aoProductsOrderByStatus)
                    {
                        ProductDetails productDetails = new ProductDetails
                        {
                            ProductId = prod.ProductId,
                            ProductName = prod.ProductName,
                            Status = prod.ProductStatus
                        };
                        aoProducts.Products.Add(productDetails);
                    }
                    IList<ProductSamlDetails> allAOProducts = productSamlDetails.Where(p => p.ParentProductTypeId == 400).ToList();
                    foreach (var item in allAOProducts)
                    {
                        productSamlDetails.Remove(item);
                    }
                }
                samlUserProductDetails.ProductSamlDetails = productSamlDetails;
                return Ok(samlUserProductDetails);
            });
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
        public async Task<IActionResult> GetProductSamlSettingsByProductId(int productId)
        {
            return await Task.Run<IActionResult>(() =>
            {
                var samlRepository = new SamlRepository();
                var result = samlRepository.GetProductSamlSettingsByProductId(productId);
                return Ok(result);
            });
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
        public async Task<IActionResult> UpdateSamlUserAttribute([FromBody] SamlAttributes samlAttributes)
        {
            return await Task.Run<IActionResult>(() =>
            {
                RepositoryResponse repositoryResponse = new RepositoryResponse();
                IManageSaml samlLogic = new ManageSaml();
                repositoryResponse = samlLogic.UpdateSamlUserAttribute(samlAttributes);
                return Ok(repositoryResponse);
            });
        }
    }
}
