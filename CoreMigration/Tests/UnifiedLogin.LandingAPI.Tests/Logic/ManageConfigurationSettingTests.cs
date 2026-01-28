using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Moq;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.LandingAPI.Tests.Helpers;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;
using Xunit;

namespace UnifiedLogin.LandingAPI.Tests.Logic
{
    /// <summary>
    /// ManageConfigurationSetting business logic xUnit tests.
    /// Migrated from RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic.ManageConfigurationSetting
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ManageConfigurationSettingTests : TestBase
    {
        private readonly Mock<IConfigurationSettingRepository> _mockConfigurationSettingRepository;

        public ManageConfigurationSettingTests()
        {
            _mockConfigurationSettingRepository = new Mock<IConfigurationSettingRepository>();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithRepository_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Assert
            Assert.NotNull(manageConfigurationSetting);
        }

        [Fact]
        public void Constructor_WithoutParameters_InitializesSuccessfully()
        {
            // Arrange & Act
            var manageConfigurationSetting = new ManageConfigurationSetting();

            // Assert
            Assert.NotNull(manageConfigurationSetting);
        }

        #endregion

        #region ListUserLoginConfigurationSetting Tests

        [Fact]
        public void ListUserLoginConfigurationSetting_WithValidPartyId_ReturnsConfigurationSettings()
        {
            // Arrange
            long partyId = 12345;
            string settingName = "DarkNavigation";
            var expectedSettings = new List<ConfigurationSetting>
            {
                new ConfigurationSetting
                {
                    MasterConfigurationSettingId = 1,
                    SettingName = "DarkNavigation",
                    Value = "true"
                },
                new ConfigurationSetting
                {
                    MasterConfigurationSettingId = 2,
                    SettingName = "DarkNavigation",
                    Value = "false"
                }
            };

            _mockConfigurationSettingRepository
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, settingName))
                .Returns(expectedSettings);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.ListUserLoginConfigurationSetting(partyId, settingName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("DarkNavigation", result[0].SettingName);
            _mockConfigurationSettingRepository.Verify(
                x => x.ListUserLoginConfigurationSetting(partyId, settingName),
                Times.Once);
        }

        [Fact]
        public void ListUserLoginConfigurationSetting_WithZeroPartyId_ThrowsException()
        {
            // Arrange
            long partyId = 0;
            string settingName = "DarkNavigation";
            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageConfigurationSetting.ListUserLoginConfigurationSetting(partyId, settingName));
            
            Assert.Equal("Invalid parameter PartyId.", exception.Message);
            _mockConfigurationSettingRepository.Verify(
                x => x.ListUserLoginConfigurationSetting(It.IsAny<long>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ListUserLoginConfigurationSetting_WithNullSettingName_ReturnsAllSettings()
        {
            // Arrange
            long partyId = 12345;
            string settingName = null;
            var expectedSettings = new List<ConfigurationSetting>
            {
                new ConfigurationSetting
                {
                    MasterConfigurationSettingId = 1,
                    SettingName = "DarkNavigation",
                    Value = "true"
                },
                new ConfigurationSetting
                {
                    MasterConfigurationSettingId = 2,
                    SettingName = "Theme",
                    Value = "Blue"
                }
            };

            _mockConfigurationSettingRepository
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, settingName))
                .Returns(expectedSettings);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.ListUserLoginConfigurationSetting(partyId, settingName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ListUserLoginConfigurationSetting_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            long partyId = 12345;
            string settingName = "NonExistentSetting";
            var expectedSettings = new List<ConfigurationSetting>();

            _mockConfigurationSettingRepository
                .Setup(x => x.ListUserLoginConfigurationSetting(partyId, settingName))
                .Returns(expectedSettings);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.ListUserLoginConfigurationSetting(partyId, settingName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region ListOrganizationConfigurationSetting Tests

        [Fact]
        public void ListOrganizationConfigurationSetting_WithValidPartyId_ReturnsConfigurationSettings()
        {
            // Arrange
            long partyId = 98765;
            string settingName = "OrganizationTheme";
            var expectedSettings = new List<ConfigurationSetting>
            {
                new ConfigurationSetting
                {
                    MasterConfigurationSettingId = 10,
                    SettingName = "OrganizationTheme",
                    Value = "Corporate"
                }
            };

            _mockConfigurationSettingRepository
                .Setup(x => x.ListOrganizationConfigurationSetting(partyId, settingName))
                .Returns(expectedSettings);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.ListOrganizationConfigurationSetting(partyId, settingName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("OrganizationTheme", result[0].SettingName);
            Assert.Equal("Corporate", result[0].Value);
            _mockConfigurationSettingRepository.Verify(
                x => x.ListOrganizationConfigurationSetting(partyId, settingName),
                Times.Once);
        }

        [Fact]
        public void ListOrganizationConfigurationSetting_WithZeroPartyId_ThrowsException()
        {
            // Arrange
            long partyId = 0;
            string settingName = "OrganizationTheme";
            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<Exception>(() =>
                manageConfigurationSetting.ListOrganizationConfigurationSetting(partyId, settingName));
            
            Assert.Equal("Invalid parameter PartyId.", exception.Message);
            _mockConfigurationSettingRepository.Verify(
                x => x.ListOrganizationConfigurationSetting(It.IsAny<long>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void ListOrganizationConfigurationSetting_WithNullSettingName_ReturnsAllSettings()
        {
            // Arrange
            long partyId = 98765;
            string settingName = null;
            var expectedSettings = new List<ConfigurationSetting>
            {
                new ConfigurationSetting
                {
                    MasterConfigurationSettingId = 10,
                    SettingName = "OrganizationTheme",
                    Value = "Corporate"
                },
                new ConfigurationSetting
                {
                    MasterConfigurationSettingId = 11,
                    SettingName = "OrganizationLogo",
                    Value = "logo.png"
                }
            };

            _mockConfigurationSettingRepository
                .Setup(x => x.ListOrganizationConfigurationSetting(partyId, settingName))
                .Returns(expectedSettings);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.ListOrganizationConfigurationSetting(partyId, settingName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void ListOrganizationConfigurationSetting_WithEmptyResult_ReturnsEmptyList()
        {
            // Arrange
            long partyId = 98765;
            string settingName = "NonExistentSetting";
            var expectedSettings = new List<ConfigurationSetting>();

            _mockConfigurationSettingRepository
                .Setup(x => x.ListOrganizationConfigurationSetting(partyId, settingName))
                .Returns(expectedSettings);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.ListOrganizationConfigurationSetting(partyId, settingName);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region UpdateConfigurationSetting Tests

        [Fact]
        public void UpdateConfigurationSetting_WithValidConfigurationSetting_ReturnsSuccessResponse()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = 100,
                SettingName = "DarkNavigation",
                Value = "true"
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 100,
                ErrorMessage = ""
            };

            _mockConfigurationSettingRepository
                .Setup(x => x.UpdateConfigurationSetting(configurationSetting))
                .Returns(expectedResponse);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.UpdateConfigurationSetting(configurationSetting);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockConfigurationSettingRepository.Verify(
                x => x.UpdateConfigurationSetting(configurationSetting),
                Times.Once);
        }

        [Fact]
        public void UpdateConfigurationSetting_WithNullConfigurationSetting_ThrowsArgumentNullException()
        {
            // Arrange
            ConfigurationSetting configurationSetting = null;
            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageConfigurationSetting.UpdateConfigurationSetting(configurationSetting));
            
            Assert.Equal("configurationSetting", exception.ParamName);
            Assert.Contains("Null ConfigurationSetting.", exception.Message);
            _mockConfigurationSettingRepository.Verify(
                x => x.UpdateConfigurationSetting(It.IsAny<ConfigurationSetting>()),
                Times.Never);
        }

        [Fact]
        public void UpdateConfigurationSetting_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var configurationSetting = new ConfigurationSetting
            {
                MasterConfigurationSettingId = 100,
                SettingName = "DarkNavigation",
                Value = "true"
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Database error occurred"
            };

            _mockConfigurationSettingRepository
                .Setup(x => x.UpdateConfigurationSetting(configurationSetting))
                .Returns(expectedResponse);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.UpdateConfigurationSetting(configurationSetting);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Database error occurred", result.ErrorMessage);
        }

        #endregion

        #region CreateMasterConfigurationSetting Tests

        [Fact]
        public void CreateMasterConfigurationSetting_WithValidMasterConfigurationSetting_ReturnsSuccessResponse()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "IdentityServerCorsAllowedOrigins",
                Value = "https://example.com",
                PartyId = null,
                CreatedBy = 1,
                MappingName = "CorsOrigins"
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 200,
                ErrorMessage = ""
            };

            _mockConfigurationSettingRepository
                .Setup(x => x.CreateMasterConfigurationSetting(masterConfigurationSetting))
                .Returns(expectedResponse);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.CreateMasterConfigurationSetting(masterConfigurationSetting);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.Id);
            Assert.Equal("", result.ErrorMessage);
            _mockConfigurationSettingRepository.Verify(
                x => x.CreateMasterConfigurationSetting(masterConfigurationSetting),
                Times.Once);
        }

        [Fact]
        public void CreateMasterConfigurationSetting_WithNullMasterConfigurationSetting_ThrowsArgumentNullException()
        {
            // Arrange
            MasterConfigurationSetting masterConfigurationSetting = null;
            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                manageConfigurationSetting.CreateMasterConfigurationSetting(masterConfigurationSetting));
            
            Assert.Equal("masterConfigurationSetting", exception.ParamName);
            Assert.Contains("Null MasterConfigurationSetting.", exception.Message);
            _mockConfigurationSettingRepository.Verify(
                x => x.CreateMasterConfigurationSetting(It.IsAny<MasterConfigurationSetting>()),
                Times.Never);
        }

        [Fact]
        public void CreateMasterConfigurationSetting_WithCompanySpecificSetting_ReturnsSuccessResponse()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Company",
                SettingType = "LandingApiCorsAllowedOrigins",
                Value = "https://company.example.com",
                PartyId = "12345",
                CreatedBy = 1,
                MappingName = "CompanyCors"
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 300,
                ErrorMessage = ""
            };

            _mockConfigurationSettingRepository
                .Setup(x => x.CreateMasterConfigurationSetting(masterConfigurationSetting))
                .Returns(expectedResponse);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.CreateMasterConfigurationSetting(masterConfigurationSetting);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(300, result.Id);
            Assert.Equal("", result.ErrorMessage);
        }

        [Fact]
        public void CreateMasterConfigurationSetting_WithRepositoryError_ReturnsErrorResponse()
        {
            // Arrange
            var masterConfigurationSetting = new MasterConfigurationSetting
            {
                ConfigurationType = "Global",
                SettingType = "IdentityServerCorsAllowedOrigins",
                Value = "https://example.com",
                CreatedBy = 1
            };

            var expectedResponse = new RepositoryResponse
            {
                Id = 0,
                ErrorMessage = "Failed to create configuration setting"
            };

            _mockConfigurationSettingRepository
                .Setup(x => x.CreateMasterConfigurationSetting(masterConfigurationSetting))
                .Returns(expectedResponse);

            var manageConfigurationSetting = new ManageConfigurationSetting(_mockConfigurationSettingRepository.Object);

            // Act
            var result = manageConfigurationSetting.CreateMasterConfigurationSetting(masterConfigurationSetting);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id);
            Assert.Equal("Failed to create configuration setting", result.ErrorMessage);
        }

        #endregion
    }
}
