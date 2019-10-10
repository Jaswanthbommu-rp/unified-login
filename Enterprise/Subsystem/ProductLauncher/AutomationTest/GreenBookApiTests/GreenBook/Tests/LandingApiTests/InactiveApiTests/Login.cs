using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace GreenBook.Tests
{
	public class Login : TestController
	{
		private string payload;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;

		public Login(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		//Login=/api/log/login

		/*Disabling this test since POST Login has already been deprecated and 
		  replaced by POST UserLogins.*/
		//[Fact, Trait("", "Happy Path")]
		public void PostLogin()
		{
			// Set up Payload
			payload = reusable.DoPostLoginPayload(Properties["enterpriseUsername"], "LoginSuccess");

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Login"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			LoginSuccessResponse activityAttemptResponse = JsonConvert.DeserializeObject<LoginSuccessResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(activityAttemptResponse.EnterpriseUserName);
			Assert.True(activityAttemptResponse.EnterpriseUserName == Properties["enterpriseUsername"]
				, "activityAttemptResponse.EnterpriseUserName == Properties[\"enterpriseUsername\"]");
			Assert.NotNull(activityAttemptResponse.IsSuccess);
			Assert.True(activityAttemptResponse.IsSuccess, "activityAttemptResponse.IsSuccess");
			Assert.False(activityAttemptResponse.IsError, "activityAttemptResponse.IsError");
			Assert.Null(activityAttemptResponse.ErrorReason);
		}
	}
}
