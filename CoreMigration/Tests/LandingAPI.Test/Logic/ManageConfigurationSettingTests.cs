using System.Diagnostics.CodeAnalysis;
using UnifiedLogin.BusinessLogic.Logic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.LandingAPI.Test.Logic
{
	[ExcludeFromCodeCoverage]
	public class ManageConfigurationSettingTests
	{
		#region Private Variables
		IManageConfigurationSetting _configurationSettingLogic;
		IManageConfigurationSetting _manageConfigurationSetting = new ManageConfigurationSetting();
		//Mock<IConfigurationSettingRepository> _mockConfigurationSettingRepository = new Mock<IConfigurationSettingRepository>();
		#endregion

		#region Manager Unit Tests
		[Fact]
		public void ListUserLoginConfigurationSetting_InvalidPartyId_ExceptionThrown()
		{
			//Arrange
			long PartyId = 0;

			//Act
			Exception exception = Record.Exception(() => _manageConfigurationSetting.ListUserLoginConfigurationSetting(PartyId, null));

			//Assert
			Assert.IsType<Exception>(exception);
		}

		[Fact]
		public void ListUserLoginConfigurationSetting_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			long PartyId = 1;
			IList<ConfigurationSetting> configurationSettingList = new List<ConfigurationSetting>();
			IList<ConfigurationSetting> expectedConfigurationSettingList = new List<ConfigurationSetting>()
			{
				new ConfigurationSetting()
				{
					MasterConfigurationSettingId = 1,
					SettingName = "DarkNavigation",
					Value = "Dark"
				}
			};

            //_mockConfigurationSettingRepository
            //	.Setup(m => m.ListUserLoginConfigurationSetting(PartyId, null))
            //	.Returns(expectedConfigurationSettingList);

            //IList<ConfigurationSetting> result = repository.GetMany<ConfigurationSetting>(StoredProcNameConstants.SP_ListUserLoginSettings, param);

            Mock<IRepository> mockRepository = new Mock<IRepository>();

            mockRepository.Setup(m => m.GetMany<ConfigurationSetting>(StoredProcNameConstants.SP_ListUserLoginSettings, It.IsAny<object>()))
                .Returns(expectedConfigurationSettingList);

            ConfigurationSettingRepository configurationSettingRepository = new ConfigurationSettingRepository(mockRepository.Object);
            _configurationSettingLogic = new ManageConfigurationSetting(configurationSettingRepository);

			//Act
			configurationSettingList = _configurationSettingLogic.ListUserLoginConfigurationSetting(PartyId, null);

			//Assert
			Assert.True(
				configurationSettingList != null
				&& configurationSettingList.Count() == 1
				&& configurationSettingList[0].MasterConfigurationSettingId == 1
				&& configurationSettingList[0].SettingName == "DarkNavigation"
				&& configurationSettingList[0]. Value == "Dark"
		   );
		}

		[Fact]
		public void UpdateConfigurationSetting_InvalidConfigurationSettingObject_ExceptionThrown()
		{
			//Arrange

			//Act
			ConfigurationSetting configurationSetting = null;

			//Assert
			Assert.Throws<ArgumentNullException>(() => _manageConfigurationSetting.UpdateConfigurationSetting(configurationSetting));
		}

		[Fact]
		public void UpdateConfigurationSetting_InvalidMasterConfigurationSettingId_ExceptionThrown()
		{
			//Arrange

			//Act
			ConfigurationSetting configurationSetting = new ConfigurationSetting()
			{
				MasterConfigurationSettingId = 0,
				SettingName = "DarkNavigation",
				Value = "Dark"
			};

			//Assert
			Assert.Throws<Exception>(() => _manageConfigurationSetting.UpdateConfigurationSetting(configurationSetting));
		}

		[Fact]
		public void UpdateConfigurationSetting_InvalidValue_ExceptionThrown()
		{
			//Arrange

			//Act
			ConfigurationSetting configurationSetting = new ConfigurationSetting()
			{
				MasterConfigurationSettingId = 1,
				SettingName = "DarkNavigation",
				Value = ""
			};

			//Assert
			Assert.Throws<Exception>(() => _manageConfigurationSetting.UpdateConfigurationSetting(configurationSetting));
		}

		[Fact]
		public void UpdateConfigurationSetting_MockInputData_ReturnValidRepositoryResponseObject()
		{
			//Arrange
			ConfigurationSetting configurationSetting = new ConfigurationSetting()
			{
				MasterConfigurationSettingId = 1,
				SettingName = "DarkNavigation",
				Value = "Dark"
			};

			Mock<IConfigurationSettingRepository> mockConfigurationSettingObject = new Mock<IConfigurationSettingRepository>();
			mockConfigurationSettingObject
				.Setup(m => m.UpdateConfigurationSetting(configurationSetting))
				.Returns(new RepositoryResponse { Id = 1, ErrorMessage = "", RealPageId = Guid.Empty });

			//Act
			_manageConfigurationSetting = new ManageConfigurationSetting(mockConfigurationSettingObject.Object);
			IRepositoryResponse repositoryResponse = _manageConfigurationSetting.UpdateConfigurationSetting(configurationSetting);

			//Assert
			Assert.True(
				repositoryResponse.Id == 1
				&& repositoryResponse.ErrorMessage == ""
				&& repositoryResponse.RealPageId == Guid.Empty
			);
		}
		#endregion
	}
}
