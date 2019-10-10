using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Data;

namespace GreenBook.Tests
{
	public class UserLoginAuth : TestController
	{
		public UserLoginAuth(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;

			authUsername = Properties["enterpriseUsername"];
			reusable = new TestUtilities(this);
		}
		private UserLogin expectedUserLogin;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		string payload, authUsername;

		// UserLoginAuth=/api/userlogin/Auth

		//[Fact, Trait("", "Happy Path")]
		public void PostUserLoginAuth()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload(authUsername, "P@ssw0rd");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Get Expected Response 
			expectedUserLogin = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLogins(JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(authUsername)).RealPageId));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(authenticateUserResponse.UserLogin.UserId);
			Assert.True(authenticateUserResponse.UserLogin.UserId == expectedUserLogin.UserId, "authenticateUserResponse.UserId == expectedUserLogin.UserId");
			Assert.NotNull(authenticateUserResponse.UserLogin.PartyId);
			Assert.True(authenticateUserResponse.UserLogin.PartyId == expectedUserLogin.PartyId, "authenticateUserResponse.PartyId == expectedUserLogin.PartyId");
			Assert.NotNull(authenticateUserResponse.UserLogin.RealPageId);
			Assert.True(authenticateUserResponse.UserLogin.RealPageId == expectedUserLogin.RealPageId, "authenticateUserResponse.RealPageId == expectedUserLogin.RealPageId");
			Assert.NotNull(authenticateUserResponse.UserLogin.LoginName);
			Assert.True(authenticateUserResponse.UserLogin.LoginName == expectedUserLogin.LoginName, "authenticateUserResponse.LoginName == expectedUserLogin.LoginName");

			Assert.True(authenticateUserResponse.UserLogin.LoginNameType == expectedUserLogin.LoginNameType, "authenticateUserResponse.LoginNameType == expectedUserLogin.LoginNameType");
			Assert.NotNull(authenticateUserResponse.UserLogin.IsActive);
			Assert.True(authenticateUserResponse.UserLogin.IsActive == expectedUserLogin.IsActive, "authenticateUserResponse.IsActive == expectedUserLogin.IsActive");
			Assert.NotNull(authenticateUserResponse.UserLogin.IsLocked);
			Assert.True(authenticateUserResponse.UserLogin.IsLocked == expectedUserLogin.IsLocked, "authenticateUserResponse.IsLocked == expectedUserLogin.IsLocked");
			Assert.True(authenticateUserResponse.UserLogin.IsTainted == expectedUserLogin.IsTainted, "authenticateUserResponse.IsTainted == expectedUserLogin.IsTainted");

			Assert.True(authenticateUserResponse.UserLogin.PasswordModifiedDate.ToString().Equals(expectedUserLogin.PasswordModifiedDate.ToString()), "authenticateUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLogin.PasswordModifiedDate.ToString())");
			Assert.True(authenticateUserResponse.UserLogin.FromDate.ToString().Equals(expectedUserLogin.FromDate.ToString()), "authenticateUserResponse.FromDate.ToString().Equals(expectedUserLogin.FromDate.ToString())");
			Assert.True(authenticateUserResponse.UserLogin.ThruDate.ToString().Equals(expectedUserLogin.ThruDate.ToString()), "authenticateUserResponse.ThruDate.ToString().Equals(expectedUserLogin.ThruDate.ToString())");

			Assert.NotNull(authenticateUserResponse.UserLogin.StatusSetDate);
			Assert.True(authenticateUserResponse.UserLogin.StatusSetDate.ToString().Equals(expectedUserLogin.StatusSetDate.ToString()), "authenticateUserResponse.StatusSetDate.ToString().Equals(expectedUserLogin.StatusSetDate.ToString())");
			Assert.True(authenticateUserResponse.UserLogin.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString()), "authenticateUserResponse.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString())");

			Assert.NotNull(authenticateUserResponse.UserLogin.IsSuperUser);
			Assert.True(authenticateUserResponse.UserLogin.IsSuperUser == expectedUserLogin.IsSuperUser, "authenticateUserResponse.IsSuperUser == expectedUserLogin.IsSuperUser");

			Assert.NotNull(authenticateUserResponse.IsError);
			Assert.False(authenticateUserResponse.IsError, "authenticateUserResponse.IsError");
			Assert.NotNull(authenticateUserResponse.ErrorReason);
			Assert.True(authenticateUserResponse.ErrorReason.Length <= 0, "authenticateUserResponse.ErrorReason.Length <= 0");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("PostUserLoginAuthValidUsernameAndInvalidPassword", "You have entered an invalid username and/or password. Please check your credentials.", "ValidUsername", "InvalidPassword")]
		[InlineData("PostUserLoginAuthInvalidUsernameAndPassword", "You have entered an invalid username and/or password. Please check your credentials.", "InvalidUsername", "InvalidPassword")]
		[InlineData("PostUserLoginAuthNoUsernameAndPassword", "\"Invalid parameter: enterpriseUserName\"", "", "")]
		[InlineData("PostUserLoginAuthNullUsernameAndPassword", "\"Invalid parameter: enterpriseUserName\"", null, null)]
		[InlineData("PostUserLoginAuthValidUsernameAndNoPassword", "\"Invalid parameter: password\"", "ValidUsername", "")]
		[InlineData("PostUserLoginAuthValidUsernameAndNullPassword", "\"Invalid parameter: password\"", "ValidUsername", null)]
		[InlineData("PostUserLoginAuthMaximumAttempt", "Max attempts to login exceeded. Your account is locked.", "aalfatov", "InvalidPassword")]
		[InlineData("PostUserLoginAuthMaximumAttemptMinusOne", "Max attempts to login exceeded. Your account is locked.", "aalfatov", "InvalidPassword")]
		public void PostUserLoginAuthNegativeCases(string testCase, string errorReason, string username = "ValidUsername", string password = "ValidPassword")
		{
			// Set up Payload
			username = username == "ValidUsername" ? authUsername : username;
			password = password == "ValidPassword" ? authUsername : password;
			payload = reusable.DoPostUserLoginAuthPayload(username, password);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			DataTable activityDetails = reusable.DoSelectActivity("1");
			int countMaxUserLoginAuthAttempts = 1;
			switch (testCase)
			{
				case "PostUserLoginAuthMaximumAttempt":
					countMaxUserLoginAuthAttempts = int.Parse(activityDetails.Rows[0]["MaxActivityAttemptCount"].ToString()) + 1;
					break;
				case "PostUserLoginAuthMaximumAttemptMinusOne":
					countMaxUserLoginAuthAttempts = int.Parse(activityDetails.Rows[0]["MaxActivityAttemptCount"].ToString());
					break;
			}

			for (int countUserLoginAuthAttempts = 0; countUserLoginAuthAttempts < countMaxUserLoginAuthAttempts; countUserLoginAuthAttempts++)
			{
				XunitTestOutPut.WriteLine("\nATTEMPT #" + (countUserLoginAuthAttempts + 1) + ":\nCalling " + HttpVerb.Post + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

				// Extract API's JSON Response
				XunitTestOutPut.WriteLine("\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			}
			
			// Get Expected Response 
			expectedUserLogin = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLogins(JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(username)).RealPageId));

			// Assert
			switch (ResponseHttpStatusCode)
			{
				case HttpStatusCode.OK:
					AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(ResponseString);
					Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
					switch (username)
					{
						default:
							Assert.NotNull(authenticateUserResponse.UserLogin.UserId);
							Assert.True(authenticateUserResponse.UserLogin.UserId == expectedUserLogin.UserId, "authenticateUserResponse.UserId == expectedUserLogin.UserId");
							Assert.NotNull(authenticateUserResponse.UserLogin.PartyId);
							Assert.True(authenticateUserResponse.UserLogin.PartyId == expectedUserLogin.PartyId, "authenticateUserResponse.PartyId == expectedUserLogin.PartyId");
							Assert.NotNull(authenticateUserResponse.UserLogin.RealPageId);
							Assert.True(authenticateUserResponse.UserLogin.RealPageId == expectedUserLogin.RealPageId, "authenticateUserResponse.RealPageId == expectedUserLogin.RealPageId");
							Assert.NotNull(authenticateUserResponse.UserLogin.LoginName);
							Assert.True(authenticateUserResponse.UserLogin.LoginName == expectedUserLogin.LoginName, "authenticateUserResponse.LoginName == expectedUserLogin.LoginName");

							Assert.True(authenticateUserResponse.UserLogin.LoginNameType == expectedUserLogin.LoginNameType, "authenticateUserResponse.LoginNameType == expectedUserLogin.LoginNameType");
							Assert.NotNull(authenticateUserResponse.UserLogin.IsActive);
							Assert.True(authenticateUserResponse.UserLogin.IsActive == expectedUserLogin.IsActive, "authenticateUserResponse.IsActive == expectedUserLogin.IsActive");
							Assert.NotNull(authenticateUserResponse.UserLogin.IsLocked);
							Assert.True(authenticateUserResponse.UserLogin.IsLocked == expectedUserLogin.IsLocked, "authenticateUserResponse.IsLocked == expectedUserLogin.IsLocked");
							Assert.True(authenticateUserResponse.UserLogin.IsTainted == expectedUserLogin.IsTainted, "authenticateUserResponse.IsTainted == expectedUserLogin.IsTainted");

							Assert.True(authenticateUserResponse.UserLogin.PasswordModifiedDate.ToString().Equals(expectedUserLogin.PasswordModifiedDate.ToString()), "authenticateUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLogin.PasswordModifiedDate.ToString())");
							Assert.True(authenticateUserResponse.UserLogin.FromDate.ToString().Equals(expectedUserLogin.FromDate.ToString()), "authenticateUserResponse.FromDate.ToString().Equals(expectedUserLogin.FromDate.ToString())");
							Assert.True(authenticateUserResponse.UserLogin.ThruDate.ToString().Equals(expectedUserLogin.ThruDate.ToString()), "authenticateUserResponse.ThruDate.ToString().Equals(expectedUserLogin.ThruDate.ToString())");

							Assert.NotNull(authenticateUserResponse.UserLogin.StatusSetDate);
							Assert.True(authenticateUserResponse.UserLogin.StatusSetDate.ToString().Equals(expectedUserLogin.StatusSetDate.ToString()), "authenticateUserResponse.StatusSetDate.ToString().Equals(expectedUserLogin.StatusSetDate.ToString())");
							Assert.True(authenticateUserResponse.UserLogin.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString()), "authenticateUserResponse.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString())");

							Assert.NotNull(authenticateUserResponse.UserLogin.IsSuperUser);
							Assert.True(authenticateUserResponse.UserLogin.IsSuperUser == expectedUserLogin.IsSuperUser, "authenticateUserResponse.IsSuperUser == expectedUserLogin.IsSuperUser");
							break;
						case "InvalidUsername":
							Assert.Null(authenticateUserResponse.UserLogin);
							break;
					}

					Assert.NotNull(authenticateUserResponse.IsError);
					Assert.True(authenticateUserResponse.IsError, "authenticateUserResponse.IsError");
					Assert.NotNull(authenticateUserResponse.ErrorReason);
					Assert.True(authenticateUserResponse.ErrorReason == errorReason, $"authenticateUserResponse.ErrorReason is not \"{errorReason}\".");
					break;
				case HttpStatusCode.BadRequest:
					Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
					Assert.NotNull(ResponseString);
					Assert.True(ResponseString == errorReason, $"ResponseString does not contain \"{errorReason}\".");
					break;
			}
		}
		
		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginAuthNullUserDeviceDetails()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload(authUsername, "P@ssw0rd");
			AuthUserDetails authUserDetailsPayload = JsonConvert.DeserializeObject<AuthUserDetails>(payload);
			authUserDetailsPayload.UserDeviceDetails = null;
			payload = JsonConvert.SerializeObject(authUserDetailsPayload);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Get Expected Response from DB
			UserLogin expectedUserLogin = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(authUsername));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(authenticateUserResponse.UserLogin.UserId);
			Assert.True(authenticateUserResponse.UserLogin.UserId == expectedUserLogin.UserId, "authenticateUserResponse.UserId == expectedUserLogin.UserId");
			Assert.NotNull(authenticateUserResponse.UserLogin.PartyId);
			Assert.True(authenticateUserResponse.UserLogin.PartyId == expectedUserLogin.PartyId, "authenticateUserResponse.PartyId == expectedUserLogin.PartyId");
			Assert.NotNull(authenticateUserResponse.UserLogin.RealPageId);
			Assert.True(authenticateUserResponse.UserLogin.RealPageId == expectedUserLogin.RealPageId, "authenticateUserResponse.RealPageId == expectedUserLogin.RealPageId");
			Assert.NotNull(authenticateUserResponse.UserLogin.LoginName);
			Assert.True(authenticateUserResponse.UserLogin.LoginName == expectedUserLogin.LoginName, "authenticateUserResponse.LoginName == expectedUserLogin.LoginName");

			if (authenticateUserResponse.UserLogin.LoginNameType != null)
			{
				Assert.NotNull(authenticateUserResponse.UserLogin.LoginNameType);
				Assert.True(authenticateUserResponse.UserLogin.LoginNameType == expectedUserLogin.LoginNameType, "authenticateUserResponse.LoginNameType == expectedUserLogin.LoginNameType");
			}

			Assert.NotNull(authenticateUserResponse.UserLogin.IsActive);
			Assert.True(authenticateUserResponse.UserLogin.IsActive == expectedUserLogin.IsActive, "authenticateUserResponse.IsActive == expectedUserLogin.IsActive");
			Assert.NotNull(authenticateUserResponse.UserLogin.IsLocked);
			Assert.True(authenticateUserResponse.UserLogin.IsLocked == expectedUserLogin.IsLocked, "authenticateUserResponse.IsLocked == expectedUserLogin.IsLocked");
			Assert.True(authenticateUserResponse.UserLogin.IsTainted == expectedUserLogin.IsTainted, "authenticateUserResponse.IsTainted == expectedUserLogin.IsTainted");

			if (authenticateUserResponse.UserLogin.PasswordModifiedDate != null)
			{
				Assert.NotNull(authenticateUserResponse.UserLogin.PasswordModifiedDate);
				Assert.True(authenticateUserResponse.UserLogin.PasswordModifiedDate.ToString().Equals(expectedUserLogin.PasswordModifiedDate.ToString()), "authenticateUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLogin.PasswordModifiedDate.ToString())");
			}
			if (authenticateUserResponse.UserLogin.FromDate != null)
			{
				Assert.NotNull(authenticateUserResponse.UserLogin.FromDate);
				Assert.True(authenticateUserResponse.UserLogin.FromDate.ToString().Equals(expectedUserLogin.FromDate.ToString()), "authenticateUserResponse.FromDate.ToString().Equals(expectedUserLogin.FromDate.ToString())");
			}
			if (authenticateUserResponse.UserLogin.ThruDate != null)
			{
				Assert.NotNull(authenticateUserResponse.UserLogin.ThruDate);
				Assert.True(authenticateUserResponse.UserLogin.ThruDate.ToString().Equals(expectedUserLogin.ThruDate.ToString()), "authenticateUserResponse.ThruDate.ToString().Equals(expectedUserLogin.ThruDate.ToString())");
			}

			Assert.NotNull(authenticateUserResponse.UserLogin.StatusSetDate);
			Assert.True(authenticateUserResponse.UserLogin.StatusSetDate.ToString().Equals(expectedUserLogin.StatusSetDate.ToString()), "authenticateUserResponse.StatusSetDate.ToString().Equals(expectedUserLogin.StatusSetDate.ToString())");
			Assert.True(authenticateUserResponse.UserLogin.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString()), "authenticateUserResponse.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString())");
			
			Assert.True(authenticateUserResponse.UserLogin.IsSuperUser == expectedUserLogin.IsSuperUser, "authenticateUserResponse.IsSuperUser == expectedUserLogin.IsSuperUser");
			
			Assert.NotNull(authenticateUserResponse.IsError);
			Assert.False(authenticateUserResponse.IsError, "authenticateUserResponse.IsError");
			Assert.NotNull(authenticateUserResponse.ErrorReason);
			Assert.True(authenticateUserResponse.ErrorReason.Length <= 0, "authenticateUserResponse.ErrorReason.Length <= 0");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginAuthMaximumAttemptMinusOne()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload(Properties["enterpriseUsername"], "invalidPassword");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			DataTable activityDetails = reusable.DoSelectActivity("1");
			string ResponseString = "";

			for (int countUserLoginAuthAttempts = 0
				; countUserLoginAuthAttempts < (int.Parse(activityDetails.Rows[0]["MaxActivityAttemptCount"].ToString()) - 1)
				; countUserLoginAuthAttempts++)
			{
				XunitTestOutPut.WriteLine("ATTEMPT #" + (countUserLoginAuthAttempts + 1) + ":\nCalling " + HttpVerb.Post + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
				
				XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			}

			// Extract API's JSON Response
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.Null(authenticateUserResponse.UserLogin);
			Assert.NotNull(authenticateUserResponse.IsError);
			Assert.True(authenticateUserResponse.IsError, "authenticateUserResponse.IsError");
			Assert.NotNull(authenticateUserResponse.ErrorReason);
			Assert.True(authenticateUserResponse.ErrorReason == "You have one more attempt to enter a valid user name and password before your account will be locked."
				, "authenticateUserResponse.ErrorReason == \"You have one more attempt to enter a valid user name and password before your account will be locked.\"");
		}
		
		//[Fact, Trait("", "Data Driven")]
		public void PostUserLoginAuthVerifyAccessAfterMaximumAttempt()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload("ia@test.com", "invalidPassword");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			DataTable activityDetails = reusable.DoSelectActivity("1");
			string ResponseString = "";

			for (int countUserLoginAuthAttempts = 0
				; countUserLoginAuthAttempts < int.Parse(activityDetails.Rows[0]["MaxActivityAttemptCount"].ToString())
				; countUserLoginAuthAttempts++)
			{
				XunitTestOutPut.WriteLine("ATTEMPT #" + (countUserLoginAuthAttempts + 1) + ":\nCalling " + HttpVerb.Post + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
				
				XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			}

			// Extract API's JSON Response
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(ResponseString);

			// Get Expected Response from DB
			UserLogin expectedUserLogin = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(authenticateUserResponse.UserLogin.UserId);
			Assert.True(authenticateUserResponse.UserLogin.UserId == expectedUserLogin.UserId, "authenticateUserResponse.UserId == expectedUserLogin.UserId");
			Assert.NotNull(authenticateUserResponse.UserLogin.PartyId);
			Assert.True(authenticateUserResponse.UserLogin.PartyId == expectedUserLogin.PartyId, "authenticateUserResponse.PartyId == expectedUserLogin.PartyId");
			Assert.NotNull(authenticateUserResponse.UserLogin.RealPageId);
			Assert.True(authenticateUserResponse.UserLogin.RealPageId == expectedUserLogin.RealPageId, "authenticateUserResponse.RealPageId == expectedUserLogin.RealPageId");
			Assert.NotNull(authenticateUserResponse.UserLogin.LoginName);
			Assert.True(authenticateUserResponse.UserLogin.LoginName == expectedUserLogin.LoginName, "authenticateUserResponse.LoginName == expectedUserLogin.LoginName");

			if (authenticateUserResponse.UserLogin.LoginNameType != null)
			{
				Assert.NotNull(authenticateUserResponse.UserLogin.LoginNameType);
				Assert.True(authenticateUserResponse.UserLogin.LoginNameType == expectedUserLogin.LoginNameType, "authenticateUserResponse.LoginNameType == expectedUserLogin.LoginNameType");
			}

			Assert.NotNull(authenticateUserResponse.UserLogin.IsActive);
			Assert.True(authenticateUserResponse.UserLogin.IsActive == expectedUserLogin.IsActive, "authenticateUserResponse.IsActive == expectedUserLogin.IsActive");
			Assert.NotNull(authenticateUserResponse.UserLogin.IsLocked);
			Assert.True(authenticateUserResponse.UserLogin.IsLocked == expectedUserLogin.IsLocked, "authenticateUserResponse.IsLocked == expectedUserLogin.IsLocked");
			Assert.NotNull(authenticateUserResponse.UserLogin.IsTainted);
			Assert.True(authenticateUserResponse.UserLogin.IsTainted == expectedUserLogin.IsTainted, "authenticateUserResponse.IsTainted == expectedUserLogin.IsTainted");

			if (authenticateUserResponse.UserLogin.PasswordModifiedDate != null)
			{
				Assert.NotNull(authenticateUserResponse.UserLogin.PasswordModifiedDate);
				Assert.True(authenticateUserResponse.UserLogin.PasswordModifiedDate.ToString().Equals(expectedUserLogin.PasswordModifiedDate.ToString()), "authenticateUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLogin.PasswordModifiedDate.ToString())");
			}
			if (authenticateUserResponse.UserLogin.FromDate != null)
			{
				Assert.NotNull(authenticateUserResponse.UserLogin.FromDate);
				Assert.True(authenticateUserResponse.UserLogin.FromDate.ToString().Equals(expectedUserLogin.FromDate.ToString()), "authenticateUserResponse.FromDate.ToString().Equals(expectedUserLogin.FromDate.ToString())");
			}
			if (authenticateUserResponse.UserLogin.ThruDate != null)
			{
				Assert.NotNull(authenticateUserResponse.UserLogin.ThruDate);
				Assert.True(authenticateUserResponse.UserLogin.ThruDate.ToString().Equals(expectedUserLogin.ThruDate.ToString()), "authenticateUserResponse.ThruDate.ToString().Equals(expectedUserLogin.ThruDate.ToString())");
			}

			Assert.NotNull(authenticateUserResponse.UserLogin.StatusSetDate);
			Assert.True(authenticateUserResponse.UserLogin.StatusSetDate.ToString().Equals(expectedUserLogin.StatusSetDate.ToString()), "authenticateUserResponse.StatusSetDate.ToString().Equals(expectedUserLogin.StatusSetDate.ToString())");
			Assert.NotNull(authenticateUserResponse.UserLogin.LastLogin);
			Assert.True(authenticateUserResponse.UserLogin.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString()), "authenticateUserResponse.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString())");
			
			Assert.True(authenticateUserResponse.UserLogin.IsSuperUser == expectedUserLogin.IsSuperUser, "authenticateUserResponse.IsSuperUser == expectedUserLogin.IsSuperUser");
			
			//if (authenticateUserResponse.UserLogin.Status != null)
			//{
			//	Assert.NotNull(authenticateUserResponse.UserLogin.Status);
			//	Assert.True(authenticateUserResponse.UserLogin.Status == expectedUserLogin.Status, "authenticateUserResponse.Status == expectedUserLogin.Status");
			//}

			Assert.NotNull(authenticateUserResponse.IsError);
			Assert.False(authenticateUserResponse.IsError, "authenticateUserResponse.IsError");
			Assert.NotNull(authenticateUserResponse.ErrorReason);
			Assert.True(authenticateUserResponse.ErrorReason.Length <= 0, "authenticateUserResponse.ErrorReason.Length <= 0");
		}
	}
}
