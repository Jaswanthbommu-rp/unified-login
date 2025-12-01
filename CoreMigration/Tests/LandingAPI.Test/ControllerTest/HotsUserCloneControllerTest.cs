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
	/// User xUnit tests
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class HotsUserCloneControllerTest
	{

public void CloneUser_VerifyRouteToAction_ReturnAction()
		{
			//Arrange
			HttpClient Config = new HttpClient();

			//Act
			WebApiConfig.Register(Config);
			Config.EnsureInitialized();
			DefaultHttpControllerSelector ControllerSelector = new DefaultHttpControllerSelector(Config);
			RouteTestBase baseTest = new RouteTestBase(Config, ControllerSelector);

			Assert.True("HOTCloneUsers" == baseTest.VerifyRouteToAction(
				HttpMethod.Post,
				"http://localhost/apienterprise/userclone"
				)
			);
		}
		
	}
}
