using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Attributes;
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
        ///
        /// </summary>
        public SettingsController()
        {
           
        }

        /// <summary>
        /// Used to initialize DI classes with userclaim
        /// </summary>
        /// <param name="controllerContext"></param>
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
        }

        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.NotFound, Description = "Not Found")]
        [SwaggerResponse(200, type: typeof(SettingResponse))]
        [SwaggerResponse(500, type: typeof(ApiError))]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get internal settings")]
        [Route("setting/internationalsetting")]
        [HttpGet]
        [AuthorizeScope("enterpriseapi")]
        public HttpResponseMessage GetCompanyInternationalSettings(string settingType)
		{
			if (string.IsNullOrEmpty(settingType))
			{
				return Request.CreateResponse(HttpStatusCode.BadRequest	, "Invalid settingType.");
			}
            try
            {
                SettingsManagement settingManagement = new SettingsManagement(_userClaims);
                var settings = settingManagement.GetCompanyInternationalSettings(_userClaims.OrganizationRealPageGuid, settingType);
                
                return Request.CreateResponse(HttpStatusCode.OK, settingManagement.GetCompanyInternationalSettings(_userClaims.OrganizationRealPageGuid, settingType));
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
		}
	}
}
