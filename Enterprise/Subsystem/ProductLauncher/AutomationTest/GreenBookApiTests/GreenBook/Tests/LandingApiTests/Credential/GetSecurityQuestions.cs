using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System;
using System.Collections.Generic;

namespace GreenBook.Tests
{
	public class GetSecurityQuestions : TestController
	{
		public GetSecurityQuestions(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
            //realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(Properties["enterpriseUsernameForStatusChange"])).RealPageId;
            userCredentials = Properties["enterpriseUsernameForSecurityQuestions"].Split('|');
            realPageId = reusable.GetRealPageId(userCredentials[0]);
        }

		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string realPageId;
        string[] userCredentials;

		// GetSecurityQuestions=/api/credential/GetSecurityQuestions

		[Fact, Trait("", "Happy Path")]
		public void GetGetSecurityQuestions()
		{
			// Retrieve Expected Security Questions assigned to User
			EndPointUrl = HostUrl + Properties["UserSelectedSecurityQuestions"] + "?realPageId=" + realPageId;
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
			ObjectOutput<List<SecurityQuestion>, ErrorData> usersAllSecurityQuestionResponse =
				JsonConvert.DeserializeObject<ObjectOutput<List<SecurityQuestion>, ErrorData>>(ResponseString);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["GetSecurityQuestions"] + WebUtility.UrlEncode(userCredentials[0]);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			SecurityQuestionResponse getSecurityQuestions = JsonConvert.DeserializeObject<SecurityQuestionResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(getSecurityQuestions.EnterpriseUserName);
			Assert.True(getSecurityQuestions.EnterpriseUserName == userCredentials[0]
                , "getSecurityQuestions.enterpriseUserName == userCredentials[0]");
			Assert.True(getSecurityQuestions.SecurityQuestions.Count > 0, "getSecurityQuestions.securityQuestions.Count > 0");
			for (int countSecurityQuestions = 0; countSecurityQuestions < getSecurityQuestions.SecurityQuestions.Count; countSecurityQuestions++)
			{
				Assert.NotNull(getSecurityQuestions.SecurityQuestions[countSecurityQuestions].SecurityQuestionId);
				Assert.NotNull(getSecurityQuestions.SecurityQuestions[countSecurityQuestions].Question);
				if (getSecurityQuestions.SecurityQuestions[countSecurityQuestions].SecurityQuestionId
					== usersAllSecurityQuestionResponse.obj[countSecurityQuestions].SecurityQuestionId)
				{
					Assert.True(getSecurityQuestions.SecurityQuestions[countSecurityQuestions].SecurityQuestionId
						== usersAllSecurityQuestionResponse.obj[countSecurityQuestions].SecurityQuestionId
						, "getSecurityQuestions.SecurityQuestions[countSecurityQuestions].SecurityQuestionId "
						+ "== usersAllSecurityQuestionResponse.obj[countSecurityQuestions].SecurityQuestionId");
					Assert.True(getSecurityQuestions.SecurityQuestions[countSecurityQuestions].Question
						== usersAllSecurityQuestionResponse.obj[countSecurityQuestions].Question
						, "getSecurityQuestions.SecurityQuestions[countSecurityQuestions].Question "
						+ "== usersAllSecurityQuestionResponse.SecurityQuestions[countSecurityQuestions].Question");
				}
			}
			Assert.NotNull(getSecurityQuestions.IsUserExist);
			Assert.True(getSecurityQuestions.IsUserExist, "getSecurityQuestions.isUserExist");
			Assert.NotNull(getSecurityQuestions.IsUserActive);
			Assert.True(getSecurityQuestions.IsUserActive, "getSecurityQuestions.IsUserActive");
			Assert.NotNull(getSecurityQuestions.IsUserLocked);
			Assert.False(getSecurityQuestions.IsUserLocked, "getSecurityQuestions.IsUserLocked");
			Assert.NotNull(getSecurityQuestions.IsUserExpired);
			Assert.False(getSecurityQuestions.IsUserExpired, "getSecurityQuestions.IsUserExpired");
			Assert.NotNull(getSecurityQuestions.IsUserTainted);
			Assert.False(getSecurityQuestions.IsUserTainted, "getSecurityQuestions.IsUserTainted");
			Assert.NotNull(getSecurityQuestions.IsUserPending);
			Assert.False(getSecurityQuestions.IsUserPending, "getSecurityQuestions.IsUserPending");
			Assert.NotNull(getSecurityQuestions.ActivityToken);
			Assert.True(getSecurityQuestions.ActivityToken.Length > 0, "getSecurityQuestions.activityToken.Length > 0");
			Assert.NotNull(getSecurityQuestions.IsError);
			Assert.False(getSecurityQuestions.IsError, "getSecurityQuestions.isError");
			Assert.Null(getSecurityQuestions.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetGetSecurityQuestionsNoUsername()
		{
			//Set up the API URL
			EndPointUrl = HostUrl + Properties["GetSecurityQuestions"];

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			//Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			//Assert
			Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.Empty(ResponseString);
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetGetSecurityQuestionsInvalidUsername", "The Username \"invalidUsername\" is incorrect or was not found.", false, "invalidUsername")]
		[InlineData("GetGetSecurityQuestionsUserWithoutSecurityQuestions", "User has no security questions defined.")]
		[InlineData("GetGetSecurityQuestionsUsernameWithThirdPartyIdentityProvider", "Forgot password is not applicable to users on external identity provider.")]
		[InlineData("GetGetSecurityQuestionsDisabledUser", "The user is not active in the system.", true)]
		[InlineData("GetGetSecurityQuestionsLockedUser", "The user account is locked.", true)]
		public void GetGetSecurityQuestionsNegativeCases(string testCase, string errorReason, bool isStatusChange = false, string username = "")
		{
			//Set up the API URL
			if (testCase == "GetGetSecurityQuestionsUsernameWithThirdPartyIdentityProvider")
			{
				username = Properties["enterpriseUsernameAzure"];
			}
			else if (testCase == "GetGetSecurityQuestionsUserWithoutSecurityQuestions")
			{
				username = Properties["enterpriseUsernameWithoutSecurityQuestions"];
			}

			if (isStatusChange)
			{
				username = Properties["enterpriseUsernameForStatusChange"];
				string statusTypeName = "";
				switch (testCase)
				{
					case "GetGetSecurityQuestionsDisabledUser":
						statusTypeName = "Disabled";
						break;
					case "GetGetSecurityQuestionsLockedUser":
						statusTypeName = "Locked";
						break;
				}
				EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=" + statusTypeName
					+ "&realPageId=" + realPageId;
				XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");
			}
			EndPointUrl = HostUrl + Properties["GetSecurityQuestions"] + username;

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			//Extract API's JSON Response
			SecurityQuestionResponse getSecurityQuestions = JsonConvert.DeserializeObject<SecurityQuestionResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			if (isStatusChange)
			{
				if (testCase == "GetGetSecurityQuestionsLockedUser")
				{
					EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Unlocked&realPageId=" + realPageId;
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");
				}
				else
				{
					EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Active&realPageId=" + realPageId;
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");
				}
			}

			//Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.Null(getSecurityQuestions.EnterpriseUserName);
			Assert.Null(getSecurityQuestions.SecurityQuestions);
			Assert.NotNull(getSecurityQuestions.IsUserExist);
			Assert.NotNull(getSecurityQuestions.IsUserActive);
			Assert.NotNull(getSecurityQuestions.IsUserLocked);
			if (isStatusChange)
			{
				Assert.True(getSecurityQuestions.IsUserExist, "getSecurityQuestions.isUserExist");
				switch (testCase)
				{
					case "GetGetSecurityQuestionsDisabledUser":
						Assert.False(getSecurityQuestions.IsUserActive, "getSecurityQuestions.IsUserActive");
						Assert.False(getSecurityQuestions.IsUserLocked, "getSecurityQuestions.IsUserLocked");
						break;
					case "GetGetSecurityQuestionsLockedUser":
						Assert.True(getSecurityQuestions.IsUserLocked, "getSecurityQuestions.IsUserLocked");
						Assert.True(getSecurityQuestions.IsUserActive, "getSecurityQuestions.IsUserActive");
						break;
				}
			}
			else
			{
				if (testCase == "GetGetSecurityQuestionsInvalidUsername")
				{
					Assert.False(getSecurityQuestions.IsUserExist, "getSecurityQuestions.isUserExist");
					Assert.False(getSecurityQuestions.IsUserActive, "getSecurityQuestions.IsUserActive");
				}
				else
				{
					Assert.True(getSecurityQuestions.IsUserExist, "getSecurityQuestions.isUserExist");
					Assert.True(getSecurityQuestions.IsUserActive, "getSecurityQuestions.IsUserActive");
				}
				Assert.False(getSecurityQuestions.IsUserLocked, "getSecurityQuestions.IsUserLocked");
			}

			Assert.NotNull(getSecurityQuestions.IsUserExpired);
			Assert.False(getSecurityQuestions.IsUserExpired, "getSecurityQuestions.IsUserExpired");
			Assert.NotNull(getSecurityQuestions.IsUserTainted);
			Assert.False(getSecurityQuestions.IsUserTainted, "getSecurityQuestions.IsUserTainted");
			Assert.NotNull(getSecurityQuestions.IsUserPending);
			Assert.False(getSecurityQuestions.IsUserPending, "getSecurityQuestions.IsUserPending");
			Assert.Null(getSecurityQuestions.ActivityToken);
			Assert.NotNull(getSecurityQuestions.IsError);
			Assert.True(getSecurityQuestions.IsError, "getSecurityQuestions.isError");
			Assert.NotNull(getSecurityQuestions.ErrorReason);
			Assert.True(getSecurityQuestions.ErrorReason == errorReason, $"getSecurityQuestions.errorReason is not \"{errorReason}\"");
		}
	}
}
