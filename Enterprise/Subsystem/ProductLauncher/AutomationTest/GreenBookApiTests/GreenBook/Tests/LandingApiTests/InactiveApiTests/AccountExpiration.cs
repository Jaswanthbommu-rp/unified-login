using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using System;

namespace GreenBook.Tests
{
	public class AccountExpiration : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DatabaseController dbManager;

		public AccountExpiration(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// AccountExpiration=/api/user/accountexpiration

		/*Disabling this test because GET AccountExpiration API has already been deprecated
		 and replaced by GET UserLogins API.*/
		//[Fact, Trait("", "Happy Path")]
		public void GetAccountExpiration()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["AccountExpiration"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			AccountExpirationResponse accountExpirationResponse = JsonConvert.DeserializeObject<AccountExpirationResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(accountExpirationResponse.SeverityLevel);
			Assert.True(accountExpirationResponse.SeverityLevel == (SeverityLevelType) Enum.Parse(typeof(SeverityLevelType), "None")
				, "accountExpirationResponse.SeverityLevel == (SeverityLevelType) Enum.Parse(typeof(SeverityLevelType), \"None\")");
			Assert.NotNull(accountExpirationResponse.DaysToExpire);
			Assert.True(accountExpirationResponse.DaysToExpire >= 0 || accountExpirationResponse.DaysToExpire <= 0
				, "accountExpirationResponse.DaysToExpire >= 0 || accountExpirationResponse.DaysToExpire <= 0");
			Assert.Null(accountExpirationResponse.Remaining);
			Assert.NotNull(accountExpirationResponse.IsError);
			Assert.False(accountExpirationResponse.IsError, "getSecurityQuestions.isError");
			Assert.Null(accountExpirationResponse.ErrorReason);
		}
	}
}
