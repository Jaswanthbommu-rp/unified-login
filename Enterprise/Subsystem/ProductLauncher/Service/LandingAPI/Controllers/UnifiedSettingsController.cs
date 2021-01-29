using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UnifiedLogin;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
    public class UnifiedSettingsController : BaseApiController
    {
        #region Private variables
        IRepositoryResponse _repositoryResponse = new RepositoryResponse();
        private IManageOrganization _manageOrganization;
        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public UnifiedSettingsController() : base() { }
        #endregion

        #region Public Methods
        /// <summary>
		/// Get Settings Details
		/// </summary>
		/// <param name="category">Setting category (e.g. Security, CustomFields)</param>
		/// <param name="booksCustomerMasterId">Books Customer MasterId</param>
		/// <param name="bookMasterTypeId">Type of Book MasterId (e.g. 1 = Black, 2 = Blue)</param>
		/// <returns>A Settings Details based on category</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when  object have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the Settings based on category", Type = typeof(IList<Setting>))]
        [SwaggerResponseExamples(typeof(IList<Setting>), typeof(SettingsExample))]
        [Route("companines/{companyId}/settings")]
        [HttpGet]
        public HttpResponseMessage GetSettings(string category, Guid companyId, string[] includes)
        {
            Organization organization = new Organization();
            IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
            IApiError apiError;

            if (companyId != null)
            {
                Organization org = _manageOrganization.GetOrganization(companyId);
                if (org == null)
                {
                    apiError = new ApiError()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = (short)HttpStatusCode.BadRequest,
                        Title = "Company not found.",
                        Detail = $"Company not found for Id: {companyId}",
                        Links = string.Empty,
                        Code = "Settings.GetSettings.2",
                        Source = new ApiErrorSource()
                        {
                            JsonPointer = string.Empty,
                            Parameter = string.Empty
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
                }

                bool IsValid = _manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, org.RealPageId);
                if (!IsValid)
                {
                    apiError = new ApiError()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = (short)HttpStatusCode.BadRequest,
                        Title = "User is not authorized.",
                        Detail = $"Logged in user is not authorized to view security settings for {org.Name}.",
                        Links = string.Empty,
                        Code = "Settings.GetSettings.3",
                        Source = new ApiErrorSource()
                        {
                            JsonPointer = string.Empty,
                            Parameter = string.Empty
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
                }
            }

            

        }
        #endregion

    }
}