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
using System.Net.Http;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	public class ManageUnifiedSettingsTests : ManageProductBaseTests
	{
		#region Private Variables
		IManageUnifiedSettings _manageUnifiedySettings;
		readonly Mock<IUnifiedSettingsRepository> _mockUnifiedSettingsRepository = new Mock<IUnifiedSettingsRepository>();
        Mock<HttpMessageHandler> _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        private readonly IList<Setting> _expectedUnifiedSettings;
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
            IList<Setting> securitySettings = manageSecuritySettings.GetUnifiedSettings(category, partyId);

            Assert.True(
                securitySettings.Count == _expectedUnifiedSettings.Count
                &&
                securitySettings.SequenceEqual(_expectedUnifiedSettings)
                &&
                NumberOfProperties == 1
            );
        }       

    }
}
