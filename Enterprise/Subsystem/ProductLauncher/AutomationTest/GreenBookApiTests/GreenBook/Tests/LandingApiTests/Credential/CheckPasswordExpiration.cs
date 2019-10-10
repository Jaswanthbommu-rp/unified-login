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
	public class CheckPasswordExpiration : TestController
	{
		public CheckPasswordExpiration(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		private string payload;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;

		// CheckPasswordExpiration=/api/credential/checkpasswordexpiration

		[Fact, Trait("", "Happy Path")]
		public void GetCheckPasswordExpiration()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["CheckPasswordExpiration"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: payload);

			// Extract API's JSON Response
			
			CheckPasswordExpirationResponse checkPasswordExpirationResponse = JsonConvert.DeserializeObject<CheckPasswordExpirationResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(checkPasswordExpirationResponse.SeverityLevel);
			Assert.True((SeverityLevelType)Enum.Parse(typeof(SeverityLevelType), "None") == checkPasswordExpirationResponse.SeverityLevel
				, "(SeverityLevelType)Enum.Parse(typeof(SeverityLevelType), \"None\") == checkPasswordExpirationResponse.SeverityLevel");
			Assert.NotNull(checkPasswordExpirationResponse.DaysToExpire);
			Assert.True(checkPasswordExpirationResponse.DaysToExpire >= 0, "checkPasswordExpirationResponse.DaysToExpire >= 0");
			Assert.False(checkPasswordExpirationResponse.IsError, "usersAllSecurityQuestionResponse.isError");
			Assert.Null(checkPasswordExpirationResponse.ErrorReason);
		}
	}
}
