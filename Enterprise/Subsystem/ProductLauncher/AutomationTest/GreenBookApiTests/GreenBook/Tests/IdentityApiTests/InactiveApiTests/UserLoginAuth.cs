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
	public class UserLoginAuth : TestBase
	{
		public UserLoginAuth(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;

			authUsername = Properties["enterpriseUsername"];
		}

		JsonController jsonManager = new JsonController();
		TestReusables reusable = new TestReusables();
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
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(responseValue);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

			// Get Expected Response from DB
			UserLogin expectedUserLogin = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(authUsername));

			// Assert
			Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
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
			Assert.NotNull(authenticateUserResponse.UserLogin.LastLogin);
			Assert.True(authenticateUserResponse.UserLogin.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString()), "authenticateUserResponse.LastLogin.ToString().Equals(expectedUserLogin.LastLogin.ToString())");
			
			Assert.NotNull(authenticateUserResponse.UserLogin.IsSuperUser);
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

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginAuthValidUsernameAndPassword()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload(Properties["enterpriseUsername"], "invalidPassword");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(responseValue);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

			// Assert
			Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
			Assert.Null(authenticateUserResponse.UserLogin);
			Assert.NotNull(authenticateUserResponse.IsError);
			Assert.True(authenticateUserResponse.IsError, "authenticateUserResponse.IsError");
			Assert.NotNull(authenticateUserResponse.ErrorReason);
			Assert.True(authenticateUserResponse.ErrorReason == "You have entered an invalid username and/or password. Please check your credentials."
				, "authenticateUserResponse.ErrorReason == \"You have entered an invalid username and / or password.Please check your credentials.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginAuthInvalidUsernameAndPassword()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload("invalidUsername", "invalidPassword");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(responseValue);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

			// Assert
			Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
			Assert.Null(authenticateUserResponse.UserLogin);
			Assert.NotNull(authenticateUserResponse.IsError);
			Assert.True(authenticateUserResponse.IsError, "authenticateUserResponse.IsError");
			Assert.NotNull(authenticateUserResponse.ErrorReason);
			Assert.True(authenticateUserResponse.ErrorReason == "You have entered an invalid username and/or password. Please check your credentials."
				, "authenticateUserResponse.ErrorReason == \"You have entered an invalid username and / or password.Please check your credentials.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginAuthNoUsernameAndPassword()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload("", "");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == response.StatusCode, "HttpStatusCode.BadRequest == response.StatusCode");
			Assert.NotNull(responseValue);
			Assert.True(responseValue == "\"Invalid parameter: enterpriseUserName\"", "responseValue == \"\"Invalid parameter: enterpriseUserName\"\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginAuthNullUsernameAndPassword()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload(null, null);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == response.StatusCode, "HttpStatusCode.BadRequest == response.StatusCode");
			Assert.NotNull(responseValue);
			Assert.True(responseValue == "\"Invalid parameter: enterpriseUserName\"", "responseValue == \"\"Invalid parameter: enterpriseUserName\"\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginAuthValidUsernameAndNoPassword()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload(Properties["enterpriseUsername"], "");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == response.StatusCode, "HttpStatusCode.BadRequest == response.StatusCode");
			Assert.NotNull(responseValue);
			Assert.True(responseValue == "\"Invalid parameter: password\"", "responseValue == \"\"Invalid parameter: password\"\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginAuthValidUsernameAndNullPassword()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload(Properties["enterpriseUsername"], "");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == response.StatusCode, "HttpStatusCode.BadRequest == response.StatusCode");
			Assert.NotNull(responseValue);
			Assert.True(responseValue == "\"Invalid parameter: password\"", "responseValue == \"\"Invalid parameter: password\"\"");
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
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(responseValue);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

			// Get Expected Response from DB
			UserLogin expectedUserLogin = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(authUsername));

			// Assert
			Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
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
			HttpWebResponse response = null;
			string responseValue = "";

			for (int countUserLoginAuthAttempts = 0
				; countUserLoginAuthAttempts < (int.Parse(activityDetails.Rows[0]["MaxActivityAttemptCount"].ToString()) - 1)
				; countUserLoginAuthAttempts++)
			{
				XunitTestOutPut.WriteLine("ATTEMPT #" + (countUserLoginAuthAttempts + 1) + ":\nCalling " + HttpVerb.Post + " at " + EndPointUrl);
				response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
				responseValue = GetHttpWebResponseValue(response);
				XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);
			}

			// Extract API's JSON Response
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(responseValue);

			// Assert
			Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
			Assert.Null(authenticateUserResponse.UserLogin);
			Assert.NotNull(authenticateUserResponse.IsError);
			Assert.True(authenticateUserResponse.IsError, "authenticateUserResponse.IsError");
			Assert.NotNull(authenticateUserResponse.ErrorReason);
			Assert.True(authenticateUserResponse.ErrorReason == "You have one more attempt to enter a valid user name and password before your account will be locked."
				, "authenticateUserResponse.ErrorReason == \"You have one more attempt to enter a valid user name and password before your account will be locked.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginAuthMaximumAttempt()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload("ia@test.com", "invalidPassword");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			DataTable activityDetails = reusable.DoSelectActivity("1");
			HttpWebResponse response = null;
			string responseValue = "";

			for (int countUserLoginAuthAttempts = 0
				; countUserLoginAuthAttempts < int.Parse(activityDetails.Rows[0]["MaxActivityAttemptCount"].ToString())
				; countUserLoginAuthAttempts++)
			{
				XunitTestOutPut.WriteLine("ATTEMPT #" + (countUserLoginAuthAttempts + 1) + ":\nCalling " + HttpVerb.Post + " at " + EndPointUrl);
				response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
				responseValue = GetHttpWebResponseValue(response);
				XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);
			}

			// Extract API's JSON Response
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(responseValue);

			// Assert
			Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
			Assert.Null(authenticateUserResponse.UserLogin);
			Assert.NotNull(authenticateUserResponse.IsError);
			Assert.True(authenticateUserResponse.IsError, "authenticateUserResponse.IsError");
			Assert.NotNull(authenticateUserResponse.ErrorReason);
			Assert.True(authenticateUserResponse.ErrorReason == "Your account has been locked for 30 minutes.  You may want to click the “Forget Password?” link and reset your password."
				, "authenticateUserResponse.ErrorReason == \"Your account has been locked for 30 minutes.  You may want to click the “Forget Password?” link and reset your password.\"");
		}

		////[Fact, Trait("", "Data Driven")]
		public void PostUserLoginAuthVerifyAccessAfterMaximumAttempt()
		{
			// Set up Payload
			payload = reusable.DoPostUserLoginAuthPayload("ia@test.com", "invalidPassword");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginAuth"];

			// Execute API
			DataTable activityDetails = reusable.DoSelectActivity("1");
			HttpWebResponse response = null;
			string responseValue = "";

			for (int countUserLoginAuthAttempts = 0
				; countUserLoginAuthAttempts < int.Parse(activityDetails.Rows[0]["MaxActivityAttemptCount"].ToString())
				; countUserLoginAuthAttempts++)
			{
				XunitTestOutPut.WriteLine("ATTEMPT #" + (countUserLoginAuthAttempts + 1) + ":\nCalling " + HttpVerb.Post + " at " + EndPointUrl);
				response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
				responseValue = GetHttpWebResponseValue(response);
				XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);
			}

			// Extract API's JSON Response
			AuthenticateUserResponse authenticateUserResponse = JsonConvert.DeserializeObject<AuthenticateUserResponse>(responseValue);

			// Get Expected Response from DB
			UserLogin expectedUserLogin = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser));

			// Assert
			Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
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
