using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Enterprise.Settings;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Controllers
{
    public class SettingsController : BaseApiController
    {
        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="messageHandler"></param>
        /// <param name="userClaims"></param>
        public SettingsController(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims)
        {
            _userClaims = userClaims;
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }
        
		[SwaggerOperation("Gets a list of all settings for an organization")]
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when Profile object have invalid entries)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Gets a list of all settings for an organization", Type = typeof(SettingResponse))]
        [Route("setting/internationalsetting")]
        [HttpGet]
        public HttpResponseMessage GetCompanyInternationalSettings(string settingType)
		{
			if (string.IsNullOrEmpty(settingType))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest	, "Invalid settingType.");
			}
            SettingsManagement settingManagement = new SettingsManagement(_userClaims, _greenBookAccessToken);

            return Request.CreateResponse(HttpStatusCode.OK, settingManagement.GetCompanyInternationalSettings(_userClaims.OrganizationRealPageGuid, settingType));
		}
	}
}
