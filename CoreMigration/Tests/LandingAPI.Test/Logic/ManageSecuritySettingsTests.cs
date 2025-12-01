using Moq;
using UnifiedLogin.BusinessLogic;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;
using UnifiedLogin.SharedObjects;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	public class ManageSecuritySettingsTests : ManageProductBaseTests
	{
		#region Private Variables
		IManageSecuritySettings _manageSecuritySettings;
        readonly Mock<ISecuritySettingsRepository> _mockSecuritySettingsRepository = new Mock<ISecuritySettingsRepository>();

        private readonly IList<Setting> _expectedSecuritySettings;

        #endregion

        public ManageSecuritySettingsTests() : base((int)ProductEnum.UnifiedPlatform)
		{
            _expectedSecuritySettings = new List<Setting>()
            {
                new Setting()
                {
                    Name = "ForcedLock",
                    Value = "288",
                    Right = 0
                },
                new Setting()
                {
                    Name = "ForgotPassword",
                    Value = "4",
                    Right = 0
                },
                new Setting()
                {
                    Name = "MinimumLength",
                    Value = "8",
                    Right = 0
                },
                new Setting()
                {
                    Name = "NewUserRegistration",
                    Value = "10",
                    Right = 0
                },
                new Setting()
                {
                    Name = "NumberOfPasswordsToRemember",
                    Value = "5",
                    Right = 0
                },
                new Setting()
                {
                    Name = "PasswordExpirationPeriodInDays",
                    Value = "100",
                    Right = 0
                }
            };
        }

		[Fact]
		public void GetSecuritySettings_InvalidSourceId_ExceptionThrown()
		{
			//Arrange
			long sourceId = 0;
			int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;

			_manageSecuritySettings = new ManageSecuritySettings(_mockSecuritySettingsRepository.Object, _userUserClaim);

			//Act
			Exception exception = Record.Exception(() => _manageSecuritySettings.GetSecuritySettings(sourceId, bookMasterTypeId));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void GetSecuritySettings_Mock_ReturnValidConfigurationSecuritySettings()
		{
			//Arrange
			int sourceId = 2116;
			int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;
			Type type = typeof(IList<Setting>);
            
            var mockRepository = new Mock<IRepository>();

            mockRepository.Setup(m => m.GetMany<Setting>(StoredProcNameConstants.SP_GetSecuritySetting, It.IsAny<object>()))
                .Returns(_expectedSecuritySettings);

            ISecuritySettingsRepository securitySettingsRepository = new SecuritySettingsRepository(mockRepository.Object);
            
            //Act
            int NumberOfProperties = type.GetProperties().Length;
			IManageSecuritySettings manageSecuritySettings = new ManageSecuritySettings(securitySettingsRepository, _userUserClaim);
			IList<Setting> securitySettings = manageSecuritySettings.GetSecuritySettings(sourceId, bookMasterTypeId);

			Assert.True(
				securitySettings.Count == _expectedSecuritySettings.Count
				&&
				securitySettings.SequenceEqual(_expectedSecuritySettings)
				&&
				NumberOfProperties == 1
			);
		}

        [Fact]
        public void UpdateSecuritySettings_Mock_Success()
        {
            //Arrange
            int sourceId = 2116;
            int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;
            Type type = typeof(IList<Setting>);

            var mockRepository = new Mock<IRepository>();
            var mockUnitofWork = new Mock<IUnitOfWork>();

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateSecuritySetting, It.IsAny<object>()))
                .Returns(new RepositoryResponse() { Id = 1234, ErrorMessage = ""});

            mockRepository.Setup(m => m.UnitOfWork).Returns(mockUnitofWork.Object);

            ISecuritySettingsRepository securitySettingsRepository = new SecuritySettingsRepository(mockRepository.Object);

            //Act
            IManageSecuritySettings manageSecuritySettings = new ManageSecuritySettings(securitySettingsRepository, _userUserClaim);
            var response = manageSecuritySettings.UpdateSecuritySettings(_expectedSecuritySettings, bookMasterTypeId);

            Assert.True(string.IsNullOrEmpty(response.ErrorMessage));
        }

        [Fact]
        public void UpdateSecuritySettings_Mock_Failure()
        {
            //Arrange
            int sourceId = 2116;
            int bookMasterTypeId = (int)BookMasterType.CustomerMasterId;
            Type type = typeof(IList<Setting>);

            var mockRepository = new Mock<IRepository>();
            var mockUnitofWork = new Mock<IUnitOfWork>();

            mockRepository.Setup(m => m.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateSecuritySetting, It.IsAny<object>()))
                .Returns(new RepositoryResponse() { Id = 0, ErrorMessage = "There was a problem with the sql" });

            mockRepository.Setup(m => m.UnitOfWork).Returns(mockUnitofWork.Object);

            ISecuritySettingsRepository securitySettingsRepository = new SecuritySettingsRepository(mockRepository.Object);

            //Act
            IManageSecuritySettings manageSecuritySettings = new ManageSecuritySettings(securitySettingsRepository, _userUserClaim);
            var response = manageSecuritySettings.UpdateSecuritySettings(_expectedSecuritySettings, bookMasterTypeId);

            Assert.Equal("Update security settings Error: There was a problem with the sql.", response.ErrorMessage);

Exception exception = Record.Exception(() => manageSecuritySettings.UpdateSecuritySettings(null, 0));

            Assert.IsType<ArgumentNullException>(exception);

            Assert.Equal("Null Security Settings (Password and Activity Configuration Security Settings).\r\nParameter name: settings", exception.Message);
        }
    }
}
