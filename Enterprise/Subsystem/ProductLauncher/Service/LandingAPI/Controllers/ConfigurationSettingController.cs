using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Swashbuckle.Swagger.Annotations;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI.Controllers
{
	/// <summary>
	/// Configuration Setting Controller to hold all Contact Mechanism UsageType management related APIs
	/// </summary>
	public class ConfigurationSettingController : BaseApiController
	{
		#region Private variables
		IRepositoryResponse repositoryResponse = new RepositoryResponse();
		#endregion

		#region Constructor
		/// <summary>
		/// Default constructor
		/// </summary>
		public ConfigurationSettingController() : base() { }
		#endregion

		#region Public Methods
		/// <summary>
		/// Get a list of user Configuration Settings
		/// </summary>
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of User Configuration Settings</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(IConfigurationSetting))]
		[SwaggerResponseExamples(typeof(IConfigurationSetting), typeof(ConfigurationSettingExample))]
		[Route("configurationsettings")]
		[Authorize]
		[HttpGet]
		public HttpResponseMessage ListConfigurationSetting(long PartyId, string SettingName = null)
		{
			ObjectListOutput<ConfigurationSetting, IErrorData> output = new ObjectListOutput<ConfigurationSetting, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();

			if (PartyId == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ConfigurationSetting.ListConfigurationSetting.1";
				errorStatus.ErrorMsg = "List ConfigurationSetting: Invalid parameter user party Id";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IList<ConfigurationSetting> configurationSettingList = new List<ConfigurationSetting>();

			IManageConfigurationSetting configurationSettingLogic = new ManageConfigurationSetting();
			configurationSettingList = configurationSettingLogic.ListUserLoginConfigurationSetting(PartyId, SettingName);

			if (configurationSettingList != null)
			{
				output.Status = errorStatus;
				output.list = configurationSettingList;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ConfigurationSetting.ListConfigurationSetting.2";
				errorStatus.ErrorMsg = "List ConfigurationSetting: No data";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
		}

        /// <summary>
		/// Get a list of Organization Settings
		/// </summary>
		/// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
		/// <param name="SettingName">Setting Name (DarkNavigation)</param>
		/// <returns>List of User Configuration Settings</returns>
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
        [SwaggerResponse(HttpStatusCode.OK, Description = "Get information about the password policy", Type = typeof(IConfigurationSetting))]
        [SwaggerResponseExamples(typeof(IConfigurationSetting), typeof(ConfigurationSettingExample))]
        [Route("organizationsettings")]
        [Authorize]
        [HttpGet]
        public HttpResponseMessage ListOrganizationSetting(long PartyId, string SettingName = null)
        {
            ObjectListOutput<ConfigurationSetting, IErrorData> output = new ObjectListOutput<ConfigurationSetting, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (PartyId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Organization.ListOrganizationSetting.1";
                errorStatus.ErrorMsg = "List OrganizationSetting: Invalid parameter user party Id";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }

            IList<ConfigurationSetting> configurationSettingList = new List<ConfigurationSetting>();

            IManageConfigurationSetting configurationSettingLogic = new ManageConfigurationSetting();
            configurationSettingList = configurationSettingLogic.ListOrganizationConfigurationSetting(PartyId, SettingName);

            if (configurationSettingList != null)
            {
                output.Status = errorStatus;
                output.list = configurationSettingList;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
            else
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ConfigurationSetting.ListOrganizationSetting.2";
                errorStatus.ErrorMsg = "List OrganizationSetting: No data";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
        }

        /// <summary>
        /// Update Configuration Setting
        /// </summary>
        /// <param name="configurationSetting">Master Configuration setting object</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when ConfigurationSetting object have invalid entries / when Information is out of sync with the server)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "Configuration Setting Updated")]
		[Route("configurationsettings")]
		[Authorize]
		[HttpPut]
		public HttpResponseMessage UpdateConfigurationSetting([FromBody] ConfigurationSetting configurationSetting)
		{
			ObjectOutput<IConfigurationSetting, IErrorData> output = new ObjectOutput<IConfigurationSetting, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.obj = configurationSetting;

			if (configurationSetting == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ConfigurationSetting.UpdateConfigurationSetting.1";
				errorStatus.ErrorMsg = "Update ConfigurationSetting: Invalid parameter configurationSetting";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (configurationSetting.MasterConfigurationSettingId == 0)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ConfigurationSetting.UpdateConfigurationSetting.2";
				errorStatus.ErrorMsg = "Update ConfigurationSetting: Invalid parameter MasterConfigurationSettingId";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
			else if (string.IsNullOrWhiteSpace(configurationSetting.Value))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ConfigurationSetting.UpdateConfigurationSetting.3";
				errorStatus.ErrorMsg = "Value is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IManageConfigurationSetting configurationSettingLogic = new ManageConfigurationSetting();
			repositoryResponse = configurationSettingLogic.UpdateConfigurationSetting(configurationSetting);
			if (repositoryResponse.Id == 0)
			{
				output.obj = configurationSetting;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ConfigurationSetting.UpdateConfigurationSetting.4";
				errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}

		/// <summary>
        /// Update Configuration Setting
        /// </summary>
        /// <param name="masterConfigurationSetting">Master Configuration setting object</param>
        /// <returns>Response with Success Message</returns>
        [SwaggerResponse(HttpStatusCode.BadRequest, Description = "Bad request(when ConfigurationSetting object have invalid entries / when Information is out of sync with the server)")]
		[SwaggerResponse(HttpStatusCode.Unauthorized, Description = "Unauthorized")]
		[SwaggerResponse(HttpStatusCode.InternalServerError, Description = "Internal Server Error")]
		[SwaggerResponse(HttpStatusCode.OK, Description = "MasterConfigurationSetting Updated")]
		[Route("configurationsettings")]
		[Authorize]
		[HttpPost]
		public HttpResponseMessage PostConfigurationSetting([FromBody] MasterConfigurationSetting masterConfigurationSetting)
		{
			ObjectOutput<MasterConfigurationSetting, IErrorData> output = new ObjectOutput<MasterConfigurationSetting, IErrorData>();
			Status<IErrorData> errorStatus = new Status<IErrorData>();
			output.obj = masterConfigurationSetting;

			if (masterConfigurationSetting == null)
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "MasterConfigurationSetting.CreateMasterConfigurationSetting.1";
				errorStatus.ErrorMsg = "Create MasterConfigurationSetting: Invalid parameter masterConfigurationSetting";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
            else if (string.IsNullOrWhiteSpace(masterConfigurationSetting.ConfigurationType))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "MasterConfigurationSetting.CreateMasterConfigurationSetting.2";
                errorStatus.ErrorMsg = "ConfigurationType is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}
            else if (string.IsNullOrWhiteSpace(masterConfigurationSetting.SettingType))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "MasterConfigurationSetting.CreateMasterConfigurationSetting.3";
                errorStatus.ErrorMsg = "SettingType is required.";
                output.Status = errorStatus;
                return Request.CreateResponse(HttpStatusCode.OK, output);
            }
			else if (string.IsNullOrWhiteSpace(masterConfigurationSetting.Value))
			{
				errorStatus.Success = false;
				errorStatus.ErrorCode = "MasterConfigurationSetting.CreateMasterConfigurationSetting.4";
				errorStatus.ErrorMsg = "Value is required.";
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			IManageConfigurationSetting configurationSettingLogic = new ManageConfigurationSetting();
			repositoryResponse = configurationSettingLogic.CreateMasterConfigurationSetting(masterConfigurationSetting);
			if (repositoryResponse.Id == 0)
			{
				output.obj = masterConfigurationSetting;
				errorStatus.Success = false;
				errorStatus.ErrorCode = "ConfigurationSetting.CreateMasterConfigurationSetting.4";
				errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
				output.Status = errorStatus;
				return Request.CreateResponse(HttpStatusCode.OK, output);
			}

			output.Status = errorStatus;
			return Request.CreateResponse(HttpStatusCode.OK, output);
		}
		#endregion

		#region Get Examples
		/// <summary>
		/// Used to document examples of the Configuration Setting Model webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
		public class ConfigurationSettingExample : IProvideExamples
		{
			/// <summary>
			/// Example object data used by Swagger to document the output of the webapi method
			/// </summary>
			/// <returns>Configuration Setting example</returns>
			public object GetExamples()
			{
				IConfigurationSetting example = new ConfigurationSetting()
				{
					MasterConfigurationSettingId = 1,
					SettingName = "DarkNavigation",
					Value = "Dark"
				};

				ObjectOutput<IConfigurationSetting, IErrorData> output = new ObjectOutput<IConfigurationSetting, IErrorData>() { obj = example };

				return output;
			}
		}
		#endregion
	}
}
