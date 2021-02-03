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
		/// <param name="companyId">Organization Id</param>
		/// <param name="includes">filter</param>
		/// <returns>A Settings Details based on category</returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when  object have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the Settings based on category", Type = typeof(IList<Setting>))]
        [SwaggerResponseExamples(typeof(IList<Setting>), typeof(UnifiedSettingsExample))]
        [Route("companines/{companyId}/settings")]
        [HttpGet]
        public HttpResponseMessage GetSettings(string category, Guid companyId, [FromUri]string[] includes)
        {
            Organization organization = new Organization();
            IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
            IApiError apiError;

            if (companyId != null)
            {
                Organization org = manageOrganization.GetOrganization(companyId);
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

                bool IsValid = manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, org.RealPageId);
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

                IList<Setting> settingList = new List<Setting>();
                UnifiedSetting unfiedSetting = new UnifiedSetting();
                IManageUnifiedSettings manageSettings = new ManageUnifiedSettings(_userClaims);
                settingList = manageSettings.GetUnifiedSettings(category, org.PartyId, includes);

                if (settingList == null)
                {
                    //When trying to get a Security Settings that don't exists
                    apiError = new ApiError()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = (short)HttpStatusCode.NotFound,
                        Title = "Unified settings not found.",
                        Detail = $"Unified settings not found for {organization.Name}",
                        Links = string.Empty,
                        Code = "Settings.GetSettings.4",
                        Source = new ApiErrorSource()
                        {
                            JsonPointer = string.Empty,
                            Parameter = string.Empty
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
                }
                unfiedSetting.keys = (List<Setting>)settingList;
                return Request.CreateResponse(HttpStatusCode.OK, unfiedSetting);
            } 
            else
            {
                apiError = new ApiError()
                {
                    Id = Guid.NewGuid().ToString(),
                    Status = (short)HttpStatusCode.BadRequest,
                    Title = "Null Companyd.",
                    Detail = $"Empty Company parameter passed",
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

        }

        /// <summary>
		/// Update Settings by category
		/// </summary>
		/// <param name="settings">Settings list of object (Key value pairs) of the parameter values</param>
		/// <param name="category">Setting category (e.g. Security, CustomFields)</param>
		/// <param name="companyId">Organization Id</param>
		/// <param name="includes">Filter</param>
		/// <returns></returns>
		[SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when SecuritySettings object have invalid entries / when Information is out of sync with the server)")]
        [SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Settings updated")]
        [Route("companines/{companyId}/settings")]
        [HttpPatch]
        public HttpResponseMessage UpdateUnifiedSettings([FromBody] IList<Setting> settings, string category, Guid companyId, [FromUri] string[] includes)
        {
            Organization organization = new Organization();
            IManageOrganization manageOrganization = new ManageOrganization(_userClaims);
            IApiError apiError;

            if (companyId != null)
            {
                Organization org = manageOrganization.GetOrganization(companyId);
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

                bool IsValid = manageOrganization.ValidateOrganization(_userClaims.OrganizationMasterId, _userClaims.UserRealPageGuid, org.RealPageId);
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

                if (settings == null)
                {
                    apiError = new ApiError()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = (short)HttpStatusCode.BadRequest,
                        Title = "Invalid parameter.",
                        Detail = $"Null parameter: {category} Settings",
                        Links = string.Empty,
                        Code = "Settings.UpdateSettings.4",
                        Source = new ApiErrorSource()
                        {
                            JsonPointer = string.Empty,
                            Parameter = string.Empty
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
                }

                if (category == null)
                {
                    apiError = new ApiError()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = (short)HttpStatusCode.BadRequest,
                        Title = "Invalid parameter.",
                        Detail = $"Null parameter:  Category",
                        Links = string.Empty,
                        Code = "Settings.UpdateSettings.5",
                        Source = new ApiErrorSource()
                        {
                            JsonPointer = string.Empty,
                            Parameter = string.Empty
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
                }

                IList<Setting> settingList = new List<Setting>();
                UnifiedSetting unfiedSetting = new UnifiedSetting();
                IManageUnifiedSettings manageSettings = new ManageUnifiedSettings(_userClaims);

                _repositoryResponse = manageSettings.UpdateUnifiedSettings(settings, category, org.PartyId, includes);
                if (_repositoryResponse.Id == 0)
                {
                    apiError = new ApiError()
                    {
                        Id = Guid.NewGuid().ToString(),
                        Status = (short)HttpStatusCode.BadRequest,
                        Title = "Update unified settings Failed.",
                        Detail = _repositoryResponse.ErrorMessage,
                        Links = string.Empty,
                        Code = "Settings.UpdateSettings.11",
                        Source = new ApiErrorSource()
                        {
                            JsonPointer = string.Empty,
                            Parameter = string.Empty
                        }
                    };
                    return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
                }
                else
                {
                    settingList = manageSettings.GetUnifiedSettings(category, org.PartyId, includes);
                    if (settingList == null)
                    {
                        //When trying to get a Security Settings that don't exists
                        apiError = new ApiError()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Status = (short)HttpStatusCode.NotFound,
                            Title = "Unified settings not found.",
                            Detail = $"Unified settings not found for {organization.Name}",
                            Links = string.Empty,
                            Code = "Settings.GetSettings.4",
                            Source = new ApiErrorSource()
                            {
                                JsonPointer = string.Empty,
                                Parameter = string.Empty
                            }
                        };
                        return Request.CreateResponse(HttpStatusCode.BadRequest, apiError);
                    }
                    unfiedSetting.keys = (List<Setting>)settingList;
                    return Request.CreateResponse(HttpStatusCode.OK, unfiedSetting);
                }
                              
            }
            else
            {
                apiError = new ApiError()
                {
                    Id = Guid.NewGuid().ToString(),
                    Status = (short)HttpStatusCode.BadRequest,
                    Title = "Null Companyd.",
                    Detail = $"Empty Company parameter passed",
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


        }
        #endregion
        #region Get Examples
        /// <summary>
        /// Used to document examples of the User List Model webapi result
        /// </summary>
        public class UnifiedSettingsExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>SecuritySettings example</returns>
            public object GetExamples()
            {
                IList<Setting> unifiedSettingList = new List<Setting>()
                {
                    new Setting()
                    {
                        Name = "Settings OR CustomFields",
                        Value = "Value",
                        Right = 0,
                        Editable = true,
                        Hidden = false
                    }
                };
                return unifiedSettingList;
            }
        }
        #endregion
    }
}