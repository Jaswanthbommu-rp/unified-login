using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Attributes
{
	[AttributeUsage(AttributeTargets.Method)]
	public class ProductAllowedAttribute : ActionFilterAttribute
	{
		private readonly ProductEnum[] _productsAllowed;

		public ProductAllowedAttribute(params ProductEnum[] productList)
		{
			this._productsAllowed = productList;
		}

		public override void OnActionExecuting(HttpActionContext filterContext)
		{
			var product = filterContext.ActionArguments["productType"];
			if (string.IsNullOrEmpty(product.ToString()))
			{
				throw new HttpResponseException(filterContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest,"productType argument missing in the request."));
			}

			if (!_productsAllowed.Contains((ProductEnum)product))
			{
				throw new HttpResponseException(filterContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest,
												$"{product} is not supporting this API. " +
												$"Supported products {string.Join(", ", _productsAllowed)}"));
			}
		}
	}
}