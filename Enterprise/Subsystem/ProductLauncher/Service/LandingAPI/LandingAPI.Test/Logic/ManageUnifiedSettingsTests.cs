using Moq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using Xunit;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Net.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.LandingAPI.Test.Logic
{
	public class ManageUnifiedSettingsTests : ManageProductBaseTests
	{
		#region Private Variables
		IManageUnifiedSettings _manageUnifiedySettings;
		readonly Mock<IUnifiedSettingsRepository> _mockUnifiedSettingsRepository = new Mock<IUnifiedSettingsRepository>();
        Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        private readonly IList<Setting> _expectedUnifiedSettings;
        private readonly IList<CustomField> _expectedCustomFields;
        Mock<IRepository> _mockRepository = new Mock<IRepository>();

        #endregion

        public ManageUnifiedSettingsTests() : base((int)ProductEnum.UnifiedPlatform)
        {
            _expectedUnifiedSettings = new List<Setting>()
            {
                new Setting()
                {
                    Name = "ForcedLock",
                    Value = "288",
                    Editable = true,
                    Hidden = false
                },
                new Setting()
                {
                    Name = "ForgotPassword",
                    Value = "4",
                    Right = 0,
                     Editable = true,
                    Hidden = false
                }
            };

            _expectedCustomFields = new List<CustomField>()
            {
                new CustomField()
                {
                    Name = "test1",
                    FieldId=1,
                    Sequence = 1,
                    Enabled = true,
                    ReadOnly = false,
                    Required = true,
                    FieldTypeId = 1,
                    MinCharLength = 1,
                    MaxCharLength = 10
                },
                new CustomField()
                {
                    Name = "test2",
                    FieldId=1,
                    Sequence = 1,
                    Enabled = true,
                    ReadOnly = false,
                    Required = true,
                    FieldTypeId = 1,
                    MinCharLength = 1,
                    MaxCharLength = 10
                }
            };
        }

        [Fact]
        public void GetUnifiedSettings_InvalidSourceId_ExceptionThrown()
        {
            //Arrange
            string category = "security";
            long partyId = 0;

            _manageUnifiedySettings = new ManageUnifiedSettings(_mockRepository.Object, _userUserClaim, _mockHttpMessageHandler.Object);

            //Act
            Exception exception = Record.Exception(() => _manageUnifiedySettings.GetUnifiedSettings(category, partyId));

            //Assert
            Assert.IsType<Exception>(exception);
        }

        [Fact]
        public void GetUnifiedSettings_Mock_ReturnValidConfigurationUnifiedSettings()
        {
            //Arrange
            string category = "security";
            long partyId = 123456;
            Type type = typeof(IList<Setting>);

            var mockRepository = new Mock<IRepository>();

            mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, It.IsAny<object>()))
                .Returns(_expectedUnifiedSettings);

            IUnifiedSettingsRepository settingsRepository = new UnifiedSettingsRepository(mockRepository.Object);

            //Act
            int NumberOfProperties = type.GetProperties().Length;
            IManageUnifiedSettings manageSecuritySettings = new ManageUnifiedSettings(mockRepository.Object, _userUserClaim, _mockHttpMessageHandler.Object);
            ISettingResponse securitySettings = manageSecuritySettings.GetUnifiedSettings(category, partyId);

            Assert.True(
                securitySettings.Keys.Count == _expectedUnifiedSettings.Count
                &&
                securitySettings.Keys.SequenceEqual(_expectedUnifiedSettings)
                &&
                NumberOfProperties == 1
            );
        }

        [Fact]
        public void GetUnifiedSettings_Mock_ReturnValidCustomFields()
        {
            //Arrange
            string category = "customfields";
            long partyId = 123456;
            Type type = typeof(IList<Setting>);

            var mockRepository = new Mock<IRepository>();

            mockRepository.Setup(m => m.GetMany<CustomField>(StoredProcNameConstants.SP_GetFieldsByPartyId, It.IsAny<object>()))
                .Returns(_expectedCustomFields);

            IUnifiedSettingsRepository settingsRepository = new UnifiedSettingsRepository(mockRepository.Object);

            //Act
            int NumberOfProperties = type.GetProperties().Length;
            IManageUnifiedSettings manageSecuritySettings = new ManageUnifiedSettings(mockRepository.Object, _userUserClaim, _mockHttpMessageHandler.Object);
            ISettingResponse securitySettings = manageSecuritySettings.GetUnifiedSettings(category, partyId);

            var values = securitySettings.Tables.Select(s => s.Value).FirstOrDefault();
            Assert.True(
                values.Count() == _expectedCustomFields.Count                
            );
        }

        [Fact]
        public void UpdateSecuritySettings_Mock_Success()
        {
            //Arrange
            string category = "security";
            long partyId = 123456;
            Type type = typeof(IList<Setting>);

            var mockRepository = new Mock<IRepository>();
            var mockUnitofWork = new Mock<IUnitOfWork>();

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUnifiedSetting, It.IsAny<object>()))
                .Returns(new RepositoryResponse() { Id = 1234, ErrorMessage = "" });

            mockRepository.Setup(m => m.UnitOfWork).Returns(mockUnitofWork.Object);

            IUnifiedSettingsRepository securitySettingsRepository = new UnifiedSettingsRepository(mockRepository.Object);

            //Act
            IManageUnifiedSettings manageSecuritySettings = new ManageUnifiedSettings(mockRepository.Object, _userUserClaim, _mockHttpMessageHandler.Object);
            var response = manageSecuritySettings.UpdateUnifiedSettings(_expectedUnifiedSettings, category,partyId,null);

            Assert.True(string.IsNullOrEmpty(response.ErrorMessage));
        }

        [Fact]
        public void UpdateUnifiedySettings_Mock_Failure()
        {
            //Arrange
            string category = "security";
            long partyId = 123456;
            Type type = typeof(IList<Setting>);

            var mockRepository = new Mock<IRepository>();
            var mockUnitofWork = new Mock<IUnitOfWork>();

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateUnifiedSetting, It.IsAny<object>()))
                .Returns(new RepositoryResponse() { Id = 0, ErrorMessage = "There was a problem with the sql" });

            mockRepository.Setup(m => m.UnitOfWork).Returns(mockUnitofWork.Object);

            IUnifiedSettingsRepository securitySettingsRepository = new UnifiedSettingsRepository(mockRepository.Object);

            //Act
            IManageUnifiedSettings manageSecuritySettings = new ManageUnifiedSettings(mockRepository.Object, _userUserClaim, _mockHttpMessageHandler.Object);
            var response = manageSecuritySettings.UpdateUnifiedSettings(_expectedUnifiedSettings, category, partyId, null);

            Assert.Equal("Update security settings Error: There was a problem with the sql.", response.ErrorMessage);


            Exception exception = Record.Exception(() => manageSecuritySettings.UpdateUnifiedSettings(null, null,0,null));

            Assert.IsType<ArgumentNullException>(exception);

            Assert.Equal("Null  Settings \r\nParameter name: settings", exception.Message);
        }
    }
}
