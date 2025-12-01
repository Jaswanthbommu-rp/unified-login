using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.LandingAPI.Test.Logic;
using UnifiedLogin.LandingAPI;
using UnifiedLogin.LandingAPI.Controllers;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;

using Xunit;

namespace UnifiedLogin.LandingAPI.Test.ControllerTest
{
	/// <summary>
	/// Product Renters Insurance xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ConfigurationSettingTests
	{
		#region Private Variables
		ConfigurationSetting _configurationSetting;
		ConfigurationSettingController _configurationSettingController = new ConfigurationSettingController();
		#endregion

		#region Controller Unit Tests
		[Fact]
		public void ListConfigurationSetting_InvalidPartyId_ExceptionThrown()
		{
			//Arrange
			long PartyId = 0;

			//Act
			Exception exception = Record.Exception(() => _configurationSettingController.ListConfigurationSetting(PartyId));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void ListConfigurationSetting_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("ListConfigurationSetting" == baseTest.VerifyRouteToAction(
				HttpMethod.Get,
				"http://localhost/api/configurationsettings?partyId=1"
				)
			);
		}

		[Fact]
		public void UpdateConfigurationSetting_InvalidConfigurationSetting_ExceptionThrown()
		{
			//Arrange
			_configurationSetting = new ConfigurationSetting();

			//Act
			Exception exception = Record.Exception(() => _configurationSettingController.UpdateConfigurationSetting(_configurationSetting));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void UpdateConfigurationSetting_InvalidMasterConfigurationSettingId_ExceptionThrown()
		{
			//Arrange
			_configurationSetting = new ConfigurationSetting()
			{
				MasterConfigurationSettingId = 0,
				SettingName = "",
				Value = ""
			};

			//Act
			Exception exception = Record.Exception(() => _configurationSettingController.UpdateConfigurationSetting(_configurationSetting));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void UpdateConfigurationSetting_InvalidValue_ExceptionThrown()
		{
			//Arrange
			_configurationSetting = new ConfigurationSetting()
			{
				MasterConfigurationSettingId = 1,
				SettingName = "",
				Value = ""
			};

			//Act
			Exception exception = Record.Exception(() => _configurationSettingController.UpdateConfigurationSetting(_configurationSetting));

			//Assert
			Assert.IsType<ArgumentNullException>(exception);
		}

		[Fact]
		public void UpdateConfigurationSetting_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			//Assert
			Assert.True("UpdateConfigurationSetting" == baseTest.VerifyRouteToAction(
				HttpMethod.Put,
				"http://localhost/api/configurationsettings"
				)
			);
		}
		#endregion
	}
}
