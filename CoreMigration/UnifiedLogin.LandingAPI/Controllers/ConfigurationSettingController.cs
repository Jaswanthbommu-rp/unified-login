using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Controllers
{
    /// <summary>
    /// Configuration Setting Controller to hold all Configuration Setting management related APIs
    /// </summary>
    [Route("")]
    [ApiController]
    [Authorize]
    public class ConfigurationSettingController : ControllerBase
    {
        private readonly IManageConfigurationSetting _manageConfigurationSetting;

        /// <summary>
        /// Constructor with dependency injection
        /// </summary>
        public ConfigurationSettingController(IManageConfigurationSetting manageConfigurationSetting)
        {
            _manageConfigurationSetting = manageConfigurationSetting;
        }

        /// <summary>
        /// Get a list of user Configuration Settings
        /// </summary>
        /// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
        /// <param name="SettingName">Setting Name (DarkNavigation)</param>
        /// <returns>List of User Configuration Settings</returns>
        [HttpGet("configurationsettings")]
        [ProducesResponseType(typeof(ObjectListOutput<ConfigurationSetting, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListConfigurationSetting(long PartyId, string SettingName = null)
        {
            ObjectListOutput<ConfigurationSetting, IErrorData> output = new ObjectListOutput<ConfigurationSetting, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (PartyId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ConfigurationSetting.ListConfigurationSetting.1";
                errorStatus.ErrorMsg = "List ConfigurationSetting: Invalid parameter user party Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var configurationSettingList = await Task.Run(() =>
                _manageConfigurationSetting.ListUserLoginConfigurationSetting(PartyId, SettingName));

            if (configurationSettingList != null)
            {
                output.Status = errorStatus;
                output.list = configurationSettingList;
                return Ok(output);
            }
            else
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ConfigurationSetting.ListConfigurationSetting.2";
                errorStatus.ErrorMsg = "List ConfigurationSetting: No data";
                output.Status = errorStatus;
                return Ok(output);
            }
        }

        /// <summary>
        /// Get a list of Organization Settings
        /// </summary>
        /// <param name="PartyId">Unique partyID (Person, Organization,...)</param>
        /// <param name="SettingName">Setting Name (DarkNavigation)</param>
        /// <returns>List of User Configuration Settings</returns>
        [HttpGet("organizationsettings")]
        [ProducesResponseType(typeof(ObjectListOutput<ConfigurationSetting, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ListOrganizationSetting(long PartyId, string SettingName = null)
        {
            ObjectListOutput<ConfigurationSetting, IErrorData> output = new ObjectListOutput<ConfigurationSetting, IErrorData>();
            Status<IErrorData> errorStatus = new Status<IErrorData>();

            if (PartyId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "Organization.ListOrganizationSetting.1";
                errorStatus.ErrorMsg = "List OrganizationSetting: Invalid parameter user party Id";
                output.Status = errorStatus;
                return Ok(output);
            }

            var configurationSettingList = await Task.Run(() =>
                _manageConfigurationSetting.ListOrganizationConfigurationSetting(PartyId, SettingName));

            if (configurationSettingList != null)
            {
                output.Status = errorStatus;
                output.list = configurationSettingList;
                return Ok(output);
            }
            else
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ConfigurationSetting.ListOrganizationSetting.2";
                errorStatus.ErrorMsg = "List OrganizationSetting: No data";
                output.Status = errorStatus;
                return Ok(output);
            }
        }

        /// <summary>
        /// Update Configuration Setting
        /// </summary>
        /// <param name="configurationSetting">Master Configuration setting object</param>
        /// <returns>Response with Success Message</returns>
        [HttpPut("configurationsettings")]
        [ProducesResponseType(typeof(ObjectOutput<IConfigurationSetting, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateConfigurationSetting([FromBody] ConfigurationSetting configurationSetting)
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
                return Ok(output);
            }
            else if (configurationSetting.MasterConfigurationSettingId == 0)
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ConfigurationSetting.UpdateConfigurationSetting.2";
                errorStatus.ErrorMsg = "Update ConfigurationSetting: Invalid parameter MasterConfigurationSettingId";
                output.Status = errorStatus;
                return Ok(output);
            }
            else if (string.IsNullOrWhiteSpace(configurationSetting.Value))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ConfigurationSetting.UpdateConfigurationSetting.3";
                errorStatus.ErrorMsg = "Value is required.";
                output.Status = errorStatus;
                return Ok(output);
            }

            var repositoryResponse = await Task.Run(() =>
                _manageConfigurationSetting.UpdateConfigurationSetting(configurationSetting));

            if (repositoryResponse.Id == 0)
            {
                output.obj = configurationSetting;
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ConfigurationSetting.UpdateConfigurationSetting.4";
                errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                output.Status = errorStatus;
                return Ok(output);
            }

            output.Status = errorStatus;
            return Ok(output);
        }

        /// <summary>
        /// Create Master Configuration Setting
        /// </summary>
        /// <param name="masterConfigurationSetting">Master Configuration setting object</param>
        /// <returns>Response with Success Message</returns>
        [HttpPost("configurationsettings")]
        [ProducesResponseType(typeof(ObjectOutput<MasterConfigurationSetting, IErrorData>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostConfigurationSetting([FromBody] MasterConfigurationSetting masterConfigurationSetting)
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
                return Ok(output);
            }
            else if (string.IsNullOrWhiteSpace(masterConfigurationSetting.ConfigurationType))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "MasterConfigurationSetting.CreateMasterConfigurationSetting.2";
                errorStatus.ErrorMsg = "ConfigurationType is required.";
                output.Status = errorStatus;
                return Ok(output);
            }
            else if (string.IsNullOrWhiteSpace(masterConfigurationSetting.SettingType))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "MasterConfigurationSetting.CreateMasterConfigurationSetting.3";
                errorStatus.ErrorMsg = "SettingType is required.";
                output.Status = errorStatus;
                return Ok(output);
            }
            else if (string.IsNullOrWhiteSpace(masterConfigurationSetting.Value))
            {
                errorStatus.Success = false;
                errorStatus.ErrorCode = "MasterConfigurationSetting.CreateMasterConfigurationSetting.4";
                errorStatus.ErrorMsg = "Value is required.";
                output.Status = errorStatus;
                return Ok(output);
            }

            var repositoryResponse = await Task.Run(() =>
                _manageConfigurationSetting.CreateMasterConfigurationSetting(masterConfigurationSetting));

            if (repositoryResponse.Id == 0)
            {
                output.obj = masterConfigurationSetting;
                errorStatus.Success = false;
                errorStatus.ErrorCode = "ConfigurationSetting.CreateMasterConfigurationSetting.4";
                errorStatus.ErrorMsg = repositoryResponse.ErrorMessage;
                output.Status = errorStatus;
                return Ok(output);
            }

            output.Status = errorStatus;
            return Ok(output);
        }
    }
}
