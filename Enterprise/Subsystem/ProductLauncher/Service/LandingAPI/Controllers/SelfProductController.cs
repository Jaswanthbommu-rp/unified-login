using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog.Events;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    [AllowAnonymous]
    public class SelfProductController : ApiController
    {
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "List of roles by partyid", Type = typeof(HttpResponseMessage))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when data filter have invalid entries / when information is out of sync with the server)")]
        [Route("SelfProduct/ValidateProductUser")]
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage ValidateProductUser(string username, string password, string productCode)
        {

            var result = GetAllProducts();
            int productId = result.Where(a=>a.BooksProductCode == productCode).Select(a=>a.ProductId).FirstOrDefault();
            IProductRepository productRepository = new ProductRepository();
            return Request.CreateResponse(HttpStatusCode.OK, productRepository.GetSelfProductUserInfo(username, password, productId));
            
        }

        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the books products", Type = typeof(GbProductMap))]
        [Route("SelfProduct/products")]
        [HttpGet]
        public HttpResponseMessage GetProducts()
        {
            var result = GetAllProducts();
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        private IList<GbProductMap> GetAllProducts()
        {
            IProductRepository productRepository = new ProductRepository();

            var result = productRepository.GetAllProducts().Where(p => p.ProductId != 2 && p.ProductId != 3 && p.ProductId != 42);
            IList<GbProductMap> productlist = result.ToList();
            return productlist;
        }
    }
}

