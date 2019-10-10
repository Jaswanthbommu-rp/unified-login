using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace GreenBook.Tests
{
	public class NewUser : TestController
	{
		public NewUser(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
			newLoginName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
			PasswordPolicyDefault = reusable.DoGetPasswordPolicy();
		}

		private string payload;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private static string newLoginName;

		// NewUser=/api/newuser

		[Theory]
		[Trait("Data-Driven", "Happy Path")]
		[InlineData("PostNewUser", "Regular User")]
		[InlineData("PostNewUserSuperUser", "RealPage System Administrator")]
		[InlineData("PostNewUserWithoutEmail", "Regular User (No Email)")]
		public void PostNewUserHappyPaths(string testCase, string userType, bool isThirdParty = false)
		{
			// Set up Payload
			if (testCase == "PostNewUserWithoutEmail")
			{
				newLoginName = newLoginName.Replace("@ApiTest.com", "2017");
			}
			payload = reusable.DoPostNewUserPayload(newLoginName);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["NewUser"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.Null(createUserResponse.EmailTemplate);
			if (testCase == "PostNewUserWithoutEmail")
			{
				Assert.Null(createUserResponse.EmailStatus);
			}
			else
			{
				Assert.True(createUserResponse.EmailStatus == "Email sent successfully.", "createUserResponse.EmailStatus is not \"Email sent successfully.\"");
			}
			Assert.NotNull(createUserResponse.UserStatus);
			Assert.True(createUserResponse.UserStatus == "User created successfully."
				, "createUserResponse.UserStatus == \"User created successfully.\"");
			// Check Newly created User's initial Status:
			bool? isPending = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLogins(
				JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(newLoginName)).RealPageId)).IsPending;
			Assert.True(isPending, $"Newly created user is not Pending Status. (isPending={isPending})");
			// Check if userToken is accepted by GET User/Validate API:
			Assert.NotNull(createUserResponse.UserToken);
			Assert.True(createUserResponse.UserToken.Length > 0, "createUserResponse.UserToken.Length > 0");
			EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(newLoginName)
				+ "&newUserRegistrationToken=" + createUserResponse.UserToken;

			XunitTestOutPut.WriteLine("\nCalling " + HttpVerb.Get + " at " + EndPointUrl + " to test the NewUser Token.");
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			ValidateUserResponse validateUserResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validateUserResponse.EnterpriseUserName);
			Assert.True(validateUserResponse.EnterpriseUserName == newLoginName, "validateUserResponse.EnterpriseUserName == newLoginName");
			Assert.NotNull(validateUserResponse.ValidateUserToken);
			Assert.True(validateUserResponse.ValidateUserToken.Length > 0, "validateUserResponse.ValidateUserToken.Length > 0");
			Assert.NotNull(validateUserResponse.IsError);
			Assert.False(validateUserResponse.IsError, "validateUserResponse.IsError");
			Assert.Null(validateUserResponse.ErrorReason);
			Assert.NotNull(createUserResponse.PersonaId);
			Assert.True(createUserResponse.PersonaId > 0, "createUserResponse.PersonaId > 0");
			//Assert.NotNull(createUserResponse.IsError);
			//Assert.False(createUserResponse.IsError, "createUserResponse.isError");
			//Assert.Null(createUserResponse.ErrorReason);
		}
		
		[Theory]
		[Trait("Data-Driven", "Negative Case")]
		// userType = Regular User
		[InlineData("PostNewUserWhiteSpaceUsername", "Username is required.", "Regular User", " ")]
		[InlineData("PostNewUserNoUsername", "Username is required.", "Regular User", "")]
		[InlineData("PostNewUserNullUsername", "Username is required.", "Regular User", null)]
		[InlineData("PostNewUserWhiteSpaceFirstName", "First name is required.", "Regular User", "AutoUsername", " ")]
		[InlineData("PostNewUserNoFirstName", "First name is required.", "Regular User", "AutoUsername", "")]
		[InlineData("PostNewUserNullFirstName", "First name is required.", "Regular User", "AutoUsername", null)]
		[InlineData("PostNewUserWhiteSpaceLastName", "Last name is required.", "Regular User", "AutoUsername", "AutoFirstName", " ")]
		[InlineData("PostNewUserNoLastName", "Last name is required.", "Regular User", "AutoUsername", "AutoFirstName", "")]
		[InlineData("PostNewUserNullLastName", "Last name is required.", "Regular User", "AutoUsername", "AutoFirstName", null)]
		[InlineData("PostNewUserExistingUsername", "Username already exists!", "Regular User", "james@test.com")]
		// userType = RealPage System Administrator
		[InlineData("PostNewUserSuperUserWhiteSpaceUsername", "Username is required.", "RealPage System Administrator", " ")]
		[InlineData("PostNewUserSuperUserNoUsername", "Username is required.", "RealPage System Administrator", "")]
		[InlineData("PostNewUserSuperUserNullUsername", "Username is required.", "RealPage System Administrator", null)]
		[InlineData("PostNewUserSuperUserWhiteSpaceFirstName", "First name is required.", "RealPage System Administrator", "AutoUsername", " ")]
		[InlineData("PostNewUserSuperUserNoFirstName", "First name is required.", "RealPage System Administrator", "AutoUsername", "")]
		[InlineData("PostNewUserSuperUserNullFirstName", "First name is required.", "RealPage System Administrator", "AutoUsername", null)]
		[InlineData("PostNewUserSuperUserWhiteSpaceLastName", "Last name is required.", "RealPage System Administrator", "AutoUsername", "AutoFirstName", " ")]
		[InlineData("PostNewUserSuperUserNoLastName", "Last name is required.", "RealPage System Administrator", "AutoUsername", "AutoFirstName", "")]
		[InlineData("PostNewUserSuperUserNullLastName", "Last name is required.", "RealPage System Administrator", "AutoUsername", "AutoFirstName", null)]
		[InlineData("PostNewUserSuperUserExistingUsername", "Username already exists!", "RealPage System Administrator", "james@test.com")]
		// userType = Regular User (No Email)
		[InlineData("PostNewUserWithoutEmailWhiteSpaceUsername", "Username is required.", "Regular User (No Email)", " ")]
		[InlineData("PostNewUserWithoutEmailNoUsername", "Username is required.", "Regular User (No Email)", "")]
		[InlineData("PostNewUserWithoutEmailNullUsername", "Username is required.", "Regular User (No Email)", null)]
		[InlineData("PostNewUserWithoutEmailWhiteSpaceFirstName", "First name is required.", "Regular User (No Email)", "AutoUsername", " ")]
		[InlineData("PostNewUserWithoutEmailNoFirstName", "First name is required.", "Regular User (No Email)", "AutoUsername", "")]
		[InlineData("PostNewUserWithoutEmailNullFirstName", "First name is required.", "Regular User (No Email)", "AutoUsername", null)]
		[InlineData("PostNewUserWithoutEmailWhiteSpaceLastName", "Last name is required.", "Regular User (No Email)", "AutoUsername", "AutoFirstName", " ")]
		[InlineData("PostNewUserWithoutEmailNoLastName", "Last name is required.", "Regular User (No Email)", "AutoUsername", "AutoFirstName", "")]
		[InlineData("PostNewUserWithoutEmailNullLastName", "Last name is required.", "Regular User (No Email)", "AutoUsername", "AutoFirstName", null)]
		[InlineData("PostNewUserWithoutEmailExistingUsername", "Username already exists!", "Regular User (No Email)", "james@test.com")]
		// userType = Invalid User Type
		[InlineData("PostNewUserInvalidUserType", "Invalid user type!", "0")]
		[InlineData("PostNewUserNullUserType", "Invalid user type!", "Regular User")]
		public void PostNewUserNegativeCases(string testCase, string errorReason, string userType, string username = "AutoUsername", string firstName = "AutoFirstName", string lastName = "AutoLastName")
		{
			// Set up Payload
			if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrEmpty(username) && username != "james@test.com")
			{
				username = newLoginName;
			}
			payload = reusable.DoPostNewUserPayload(username, firstName, lastName, "", userType);
			if (testCase == "PostNewUserNullUserType")
			{
				payload = payload.Replace("401", "null");
			}
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["NewUser"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.Null(createUserResponse.EmailTemplate);
			Assert.Null(createUserResponse.EmailStatus);
			Assert.NotNull(createUserResponse.UserStatus);
			Assert.True(createUserResponse.UserStatus == errorReason
			, $"createUserResponse.UserStatus is not \"{errorReason}\"");
			Assert.Null(createUserResponse.UserToken);
			Assert.NotNull(createUserResponse.PersonaId);
			Assert.True(createUserResponse.PersonaId == 0, "createUserResponse.PersonaId == 0");
			Assert.NotNull(createUserResponse.Status.Success);
			Assert.False(createUserResponse.Status.Success, "createUserResponse.Status.Success is true.");
			Assert.NotNull(createUserResponse.Status.ErrorCode);
			Assert.True(createUserResponse.Status.ErrorCode.Contains("10002"), "createUserResponse.Status.ErrorCode doesn't contain \"10002\".");
			Assert.NotNull(createUserResponse.Status.ErrorMsg);
			Assert.True(createUserResponse.Status.ErrorMsg == errorReason
				, $"createUserResponse.Status.ErrorMsg is not \"{errorReason}\"");
		}		
	}
}
