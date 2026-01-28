using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.LandingAPI.Controllers;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Controllers
{
    /// <summary>
    /// Comprehensive unit tests for ConfigurationSettingController.
    /// Tests all endpoints, error cases, and edge cases for 100% code coverage.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ConfigurationSettingControllerTests : ControllerTestBase
    {
        #region Private Fields

        private readonly Mock<IManageConfigurationSetting> _mockManageConfigurationSetting;
        private ConfigurationSettingController _configurationSettingController;

        #endregion

        #region Constructor

        public ConfigurationSettingControllerTests()
        {
            _mockManageConfigurationSetting = new Mock<IManageConfigurationSetting>();

            _configurationSettingController = new ConfigurationSettingController(
                _mockManageConfigurationSetting.Object
            )
            {
                ControllerContext = CreateControllerContext()
            };
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidDependency_CreatesInstance()
        {
            // Act
            var controller = new ConfigurationSettingController(_mockManageConfigurationSetting.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullDependency_CreatesInstance()
        {
            // Note: Controller doesn't have null checks, so this documents current behavior
            // Act
            var controller = new ConfigurationSettingController(null!);

            // Assert
            Assert.NotNull(controller);
        }

        #endregion

        #region ListConfigurationSetting Tests

        [Fact]
        public async Task ListConfigurationSetting_WithZeroPartyId_ReturnsErrorStatus()
        {
            // Arrange
            const long partyId = 0;

            // Act
            var result = await _configurationSettingController.ListConfigurationSetting(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.ListConfigurationSetting.1", output.Status.ErrorCode);
            Assert.Equal("List ConfigurationSetting: Invalid parameter user party Id", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task ListConfigurationSetting_WithValidPartyId_ReturnsSettings()
        {
            // Arrange
            const long partyId = 12345;
            var expectedSettings = new List<ConfigurationSetting>
            {
                new ConfigurationSetting { MasterConfigurationSettingId = 1, SettingName = "DarkMode", Value = "true" },
                new ConfigurationSetting { MasterConfigurationSettingId = 2, SettingName = "Language", Value = "en-US" }
            };

            _mockManageConfigurationSetting
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, null))
                .Returns(expectedSettings);

            // Act
            var result = await _configurationSettingController.ListConfigurationSetting(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.Equal(expectedSettings, output.list);
            Assert.NotNull(output.Status);
        }

        [Fact]
        public async Task ListConfigurationSetting_WithValidPartyIdAndSettingName_ReturnsFilteredSettings()
        {
            // Arrange
            const long partyId = 12345;
            const string settingName = "DarkMode";
            var expectedSettings = new List<ConfigurationSetting>
            {
                new ConfigurationSetting { MasterConfigurationSettingId = 1, SettingName = "DarkMode", Value = "true" }
            };

            _mockManageConfigurationSetting
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, settingName))
                .Returns(expectedSettings);

            // Act
            var result = await _configurationSettingController.ListConfigurationSetting(partyId, settingName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
            Assert.Equal("DarkMode", output.list[0].SettingName);
        }

        [Fact]
        public async Task ListConfigurationSetting_WhenRepositoryReturnsNull_ReturnsErrorStatus()
        {
            // Arrange
            const long partyId = 12345;

            _mockManageConfigurationSetting
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, null))
                .Returns((IList<ConfigurationSetting>)null!);

            // Act
            var result = await _configurationSettingController.ListConfigurationSetting(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.ListConfigurationSetting.2", output.Status.ErrorCode);
            Assert.Equal("List ConfigurationSetting: No data", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task ListConfigurationSetting_WithEmptySettingName_ReturnsSettings()
        {
            // Arrange
            const long partyId = 12345;
            const string settingName = "";
            var expectedSettings = new List<ConfigurationSetting>
            {
                new ConfigurationSetting { MasterConfigurationSettingId = 1, SettingName = "Setting1", Value = "Value1" }
            };

            _mockManageConfigurationSetting
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, settingName))
                .Returns(expectedSettings);

            // Act
            var result = await _configurationSettingController.ListConfigurationSetting(partyId, settingName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.NotNull(output.list);
        }

        [Fact]
        public async Task ListConfigurationSetting_WithLargePartyId_ReturnsSettings()
        {
            // Arrange
            const long partyId = long.MaxValue;
            var expectedSettings = new List<ConfigurationSetting>();

            _mockManageConfigurationSetting
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, null))
                .Returns(expectedSettings);

            // Act
            var result = await _configurationSettingController.ListConfigurationSetting(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.Empty(output.list);
        }

        [Fact]
        public async Task ListConfigurationSetting_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            const long partyId = 999;
            const string settingName = "TestSetting";

            _mockManageConfigurationSetting
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, settingName))
                .Returns(new List<ConfigurationSetting>());

            // Act
            await _configurationSettingController.ListConfigurationSetting(partyId, settingName);

            // Assert
            _mockManageConfigurationSetting.Verify(
                x => x.ListUserLoginConfigurationSetting(partyId, settingName),
                Times.Once);
        }

        #endregion

        #region ListOrganizationSetting Tests

        [Fact]
        public async Task ListOrganizationSetting_WithZeroPartyId_ReturnsErrorStatus()
        {
            // Arrange
            const long partyId = 0;

            // Act
            var result = await _configurationSettingController.ListOrganizationSetting(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("Organization.ListOrganizationSetting.1", output.Status.ErrorCode);
            Assert.Equal("List OrganizationSetting: Invalid parameter user party Id", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task ListOrganizationSetting_WithValidPartyId_ReturnsSettings()
        {
            // Arrange
            const long partyId = 54321;
            var expectedSettings = new List<ConfigurationSetting>
            {
                new ConfigurationSetting { MasterConfigurationSettingId = 1, SettingName = "OrgSetting1", Value = "OrgValue1" },
                new ConfigurationSetting { MasterConfigurationSettingId = 2, SettingName = "OrgSetting2", Value = "OrgValue2" }
            };

            _mockManageConfigurationSetting
                .Setup(x => x.ListOrganizationConfigurationSetting(partyId, null))
                .Returns(expectedSettings);

            // Act
            var result = await _configurationSettingController.ListOrganizationSetting(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.Equal(2, output.list.Count);
            Assert.NotNull(output.Status);
        }

        [Fact]
        public async Task ListOrganizationSetting_WithValidPartyIdAndSettingName_ReturnsFilteredSettings()
        {
            // Arrange
            const long partyId = 54321;
            const string settingName = "OrgSetting1";
            var expectedSettings = new List<ConfigurationSetting>
            {
                new ConfigurationSetting { MasterConfigurationSettingId = 1, SettingName = "OrgSetting1", Value = "OrgValue1" }
            };

            _mockManageConfigurationSetting
                .Setup(x => x.ListOrganizationConfigurationSetting(partyId, settingName))
                .Returns(expectedSettings);

            // Act
            var result = await _configurationSettingController.ListOrganizationSetting(partyId, settingName);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.Single(output.list);
        }

        [Fact]
        public async Task ListOrganizationSetting_WhenRepositoryReturnsNull_ReturnsErrorStatus()
        {
            // Arrange
            const long partyId = 54321;

            _mockManageConfigurationSetting
                .Setup(x => x.ListOrganizationConfigurationSetting(partyId, null))
                .Returns((IList<ConfigurationSetting>)null!);

            // Act
            var result = await _configurationSettingController.ListOrganizationSetting(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.ListOrganizationSetting.2", output.Status.ErrorCode);
            Assert.Equal("List OrganizationSetting: No data", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task ListOrganizationSetting_CallsRepositoryWithCorrectParameters()
        {
            // Arrange
            const long partyId = 777;
            const string settingName = "OrgTestSetting";

            _mockManageConfigurationSetting
                .Setup(x => x.ListOrganizationConfigurationSetting(partyId, settingName))
                .Returns(new List<ConfigurationSetting>());

            // Act
            await _configurationSettingController.ListOrganizationSetting(partyId, settingName);

            // Assert
            _mockManageConfigurationSetting.Verify(
                x => x.ListOrganizationConfigurationSetting(partyId, settingName),
                Times.Once);
        }

        [Fact]
        public async Task ListOrganizationSetting_WithNegativePartyId_ReturnsSettings()
        {
            // Arrange - negative party ID is not zero, so it passes validation
            const long partyId = -1;
            var expectedSettings = new List<ConfigurationSetting>();

            _mockManageConfigurationSetting
                .Setup(x => x.ListOrganizationConfigurationSetting(partyId, null))
                .Returns(expectedSettings);

            // Act
            var result = await _configurationSettingController.ListOrganizationSetting(partyId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectListOutput<ConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.NotNull(output.list);
        }

        #endregion

        #region UpdateConfigurationSetting Tests

        [Fact]
        public async Task UpdateConfigurationSetting_WithNullConfigurationSetting_ReturnsErrorStatus()
        {
            // Arrange
            ConfigurationSetting configurationSetting = null!;

            // Act
            var result = await _configurationSettingController.UpdateConfigurationSetting(configurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.UpdateConfigurationSetting.1", output.Status.ErrorCode);
            Assert.Equal("Update ConfigurationSetting: Invalid parameter configurationSetting", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task UpdateConfigurationSetting_WithZeroMasterConfigurationSettingId_ReturnsErrorStatus()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = 0,
                SettingName = "TestSetting",
                Value = "TestValue"
            };

            // Act
            var result = await _configurationSettingController.UpdateConfigurationSetting(configurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.UpdateConfigurationSetting.2", output.Status.ErrorCode);
            Assert.Equal("Update ConfigurationSetting: Invalid parameter MasterConfigurationSettingId", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task UpdateConfigurationSetting_WithNullValue_ReturnsErrorStatus()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = 1,
                SettingName = "TestSetting",
                Value = null!
            };

            // Act
            var result = await _configurationSettingController.UpdateConfigurationSetting(configurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.UpdateConfigurationSetting.3", output.Status.ErrorCode);
            Assert.Equal("Value is required.", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task UpdateConfigurationSetting_WithEmptyValue_ReturnsErrorStatus()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = 1,
                SettingName = "TestSetting",
                Value = ""
            };

            // Act
            var result = await _configurationSettingController.UpdateConfigurationSetting(configurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.UpdateConfigurationSetting.3", output.Status.ErrorCode);
        }

        [Fact]
        public async Task UpdateConfigurationSetting_WithWhitespaceValue_ReturnsErrorStatus()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = 1,
                SettingName = "TestSetting",
                Value = "   "
            };

            // Act
            var result = await _configurationSettingController.UpdateConfigurationSetting(configurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.UpdateConfigurationSetting.3", output.Status.ErrorCode);
        }

        [Fact]
        public async Task UpdateConfigurationSetting_WhenRepositoryReturnsZeroId_ReturnsErrorStatus()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = 1,
                SettingName = "TestSetting",
                Value = "TestValue"
            };

            var repositoryResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Update failed"
            };

            _mockManageConfigurationSetting
                .Setup(x => x.UpdateConfigurationSetting(configurationSetting))
                .Returns(repositoryResponse);

            // Act
            var result = await _configurationSettingController.UpdateConfigurationSetting(configurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.UpdateConfigurationSetting.4", output.Status.ErrorCode);
            Assert.Equal("Update failed", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task UpdateConfigurationSetting_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = 1,
                SettingName = "TestSetting",
                Value = "TestValue"
            };

            var repositoryResponse = new RepositoryResponse
            {
                Id = 1,
                ErrorMessage = ""
            };

            _mockManageConfigurationSetting
                .Setup(x => x.UpdateConfigurationSetting(configurationSetting))
                .Returns(repositoryResponse);

            // Act
            var result = await _configurationSettingController.UpdateConfigurationSetting(configurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<IConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.NotNull(output.Status);
        }

        [Fact]
        public async Task UpdateConfigurationSetting_CallsRepositoryWithCorrectParameter()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = 99,
                SettingName = "VerifySetting",
                Value = "VerifyValue"
            };

            _mockManageConfigurationSetting
                .Setup(x => x.UpdateConfigurationSetting(It.IsAny<ConfigurationSetting>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            await _configurationSettingController.UpdateConfigurationSetting(configurationSetting);

            // Assert
            _mockManageConfigurationSetting.Verify(
                x => x.UpdateConfigurationSetting(configurationSetting),
                Times.Once);
        }

        [Fact]
        public async Task UpdateConfigurationSetting_WithLargeMasterConfigurationSettingId_ReturnsSuccess()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = long.MaxValue,
                SettingName = "LargeSetting",
                Value = "LargeValue"
            };

            _mockManageConfigurationSetting
                .Setup(x => x.UpdateConfigurationSetting(configurationSetting))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _configurationSettingController.UpdateConfigurationSetting(configurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        #endregion

        #region PostConfigurationSetting Tests

        [Fact]
        public async Task PostConfigurationSetting_WithNullMasterConfigurationSetting_ReturnsErrorStatus()
        {
            // Arrange
            MasterConfigurationSetting masterConfigurationSetting = null!;

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.1", output.Status.ErrorCode);
            Assert.Equal("Create MasterConfigurationSetting: Invalid parameter masterConfigurationSetting", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithNullConfigurationType_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = null!,
                SettingType = "TestSettingType",
                Value = "TestValue"
            };

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.2", output.Status.ErrorCode);
            Assert.Equal("ConfigurationType is required.", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithEmptyConfigurationType_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "",
                SettingType = "TestSettingType",
                Value = "TestValue"
            };

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithWhitespaceConfigurationType_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "   ",
                SettingType = "TestSettingType",
                Value = "TestValue"
            };

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.2", output.Status.ErrorCode);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithNullSettingType_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = null!,
                Value = "TestValue"
            };

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.3", output.Status.ErrorCode);
            Assert.Equal("SettingType is required.", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithEmptySettingType_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "",
                Value = "TestValue"
            };

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.3", output.Status.ErrorCode);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithWhitespaceSettingType_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "   ",
                Value = "TestValue"
            };

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.3", output.Status.ErrorCode);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithNullValue_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "CorsAllowedOrigins",
                Value = null!
            };

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.4", output.Status.ErrorCode);
            Assert.Equal("Value is required.", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithEmptyValue_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "CorsAllowedOrigins",
                Value = ""
            };

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.4", output.Status.ErrorCode);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithWhitespaceValue_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "CorsAllowedOrigins",
                Value = "   "
            };

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("MasterConfigurationSetting.CreateMasterConfigurationSetting.4", output.Status.ErrorCode);
        }

        [Fact]
        public async Task PostConfigurationSetting_WhenRepositoryReturnsZeroId_ReturnsErrorStatus()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "CorsAllowedOrigins",
                Value = "https://example.com"
            };

            var repositoryResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Creation failed"
            };

            _mockManageConfigurationSetting
                .Setup(x => x.CreateMasterConfigurationSetting(masterConfigurationSetting))
                .Returns(repositoryResponse);

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.False(output.Status.Success);
            Assert.Equal("ConfigurationSetting.CreateMasterConfigurationSetting.4", output.Status.ErrorCode);
            Assert.Equal("Creation failed", output.Status.ErrorMsg);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "CorsAllowedOrigins",
                Value = "https://example.com",
                PartyId = null,
                CreatedBy = 123
            };

            var repositoryResponse = new RepositoryResponse
            {
                Id = 1,
                ErrorMessage = ""
            };

            _mockManageConfigurationSetting
                .Setup(x => x.CreateMasterConfigurationSetting(masterConfigurationSetting))
                .Returns(repositoryResponse);

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.NotNull(output.Status);
        }

        [Fact]
        public async Task PostConfigurationSetting_CallsRepositoryWithCorrectParameter()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "VerifyType",
                SettingType = "VerifySettingType",
                Value = "VerifyValue"
            };

            _mockManageConfigurationSetting
                .Setup(x => x.CreateMasterConfigurationSetting(It.IsAny<MasterConfigurationSetting>()))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            _mockManageConfigurationSetting.Verify(
                x => x.CreateMasterConfigurationSetting(masterConfigurationSetting),
                Times.Once);
        }

        [Fact]
        public async Task PostConfigurationSetting_WithAllOptionalFields_ReturnsSuccess()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "CorsAllowedOrigins",
                Value = "https://example.com",
                PartyId = "12345",
                CreatedBy = 99,
                MappingName = "TestMapping"
            };

            _mockManageConfigurationSetting
                .Setup(x => x.CreateMasterConfigurationSetting(masterConfigurationSetting))
                .Returns(new RepositoryResponse { Id = 1 });

            // Act
            var result = await _configurationSettingController.PostConfigurationSetting(masterConfigurationSetting);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var output = Assert.IsType<ObjectOutput<MasterConfigurationSetting, IErrorData>>(okResult.Value);
            Assert.Equal(masterConfigurationSetting, output.obj);
        }

        #endregion

        #region Parameter Combination Tests

        [Theory]
        [InlineData(1, null)]
        [InlineData(1, "")]
        [InlineData(1, "DarkMode")]
        [InlineData(100, "Setting")]
        [InlineData(999999, "LongSettingName")]
        public async Task ListConfigurationSetting_WithVariousParameterCombinations_ReturnsOkResult(
            long partyId, string settingName)
        {
            // Arrange
            _mockManageConfigurationSetting
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, settingName))
                .Returns(new List<ConfigurationSetting>());

            // Act
            var result = await _configurationSettingController.ListConfigurationSetting(partyId, settingName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Theory]
        [InlineData(1, null)]
        [InlineData(1, "")]
        [InlineData(1, "OrgSetting")]
        [InlineData(100, "Setting")]
        [InlineData(999999, "LongOrgSettingName")]
        public async Task ListOrganizationSetting_WithVariousParameterCombinations_ReturnsOkResult(
            long partyId, string settingName)
        {
            // Arrange
            _mockManageConfigurationSetting
                .Setup(x => x.ListOrganizationConfigurationSetting(partyId, settingName))
                .Returns(new List<ConfigurationSetting>());

            // Act
            var result = await _configurationSettingController.ListOrganizationSetting(partyId, settingName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Concurrent Access Tests

        [Fact]
        public async Task ListConfigurationSetting_MultipleConcurrentCalls_AllReturnOk()
        {
            // Arrange
            _mockManageConfigurationSetting
                .Setup(x => x.ListUserLoginConfigurationSetting(It.IsAny<long>(), It.IsAny<string>()))
                .Returns(new List<ConfigurationSetting>());

            var tasks = new List<Task<IActionResult>>();

            // Act
            for (int i = 1; i <= 10; i++)
            {
                tasks.Add(_configurationSettingController.ListConfigurationSetting(i));
            }

            var results = await Task.WhenAll(tasks);

            // Assert
            foreach (var result in results)
            {
                Assert.IsType<OkObjectResult>(result);
            }
        }

        #endregion

        #region Dispose

        public override void Dispose()
        {
            _configurationSettingController = null!;
            base.Dispose();
        }

        #endregion
    }
}





