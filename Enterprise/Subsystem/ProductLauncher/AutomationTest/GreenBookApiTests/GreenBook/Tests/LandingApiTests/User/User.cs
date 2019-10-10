using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GreenBook.Tests
{
    public class User : TestController
    {
		public User(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId;
			realpageId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona(personaId)).RealPageId;
			existingLoginName = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLogins(realpageId)).LoginName;

			newLoginName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
            loginUser = Properties["LoginUser"].Split('|');
            
		}
		
		private CreateUserResponse<ErrorData> createUserResponse;
		private string payload, newLoginName, existingLoginName;
		private Guid realpageId;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private long personaId;
        String[] loginUser;
        
		// User=/api/User

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetUser")]
		public void GetUserHappyPaths(string testCase)
		{
			// Extract Expected JSON Responses
			_accessToken = GetClientToken(Properties["identityClientUrl"], loginUser[0], loginUser[1]);
			EndPointUrl = HostUrl + Properties["Persons"];
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			ObjectListOutput<ProfileDetail, ErrorData> personResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProfileDetail, ErrorData>>(ResponseString);

			//Set up the API URL
			personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId;
			realpageId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona(personaId)).RealPageId;
			EndPointUrl = HostUrl + Properties["User"] + "/" + realpageId;

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectOutput<ProfileDetail, ErrorData> userResponse = JsonConvert.DeserializeObject<ObjectOutput<ProfileDetail, ErrorData>>(ResponseString);

			ProfileDetail expectedProfileDetailResponse = new ProfileDetail();
			foreach (ProfileDetail profileDetail in personResponse.list)
			{
				if (profileDetail.userLogin.UserId == userResponse.obj.userLogin.UserId)
				{
					expectedProfileDetailResponse = profileDetail;
					break;
				}
			}

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK != ResponseHttpStatusCode");
			Assert.NotNull(userResponse.obj.userLogin.UserId);
			Assert.True(userResponse.obj.userLogin.UserId == expectedProfileDetailResponse.userLogin.UserId, "userResponse.obj.userLogin.UserId != userLoginResponse.UserId");
			Assert.NotNull(userResponse.obj.userLogin.PartyId);
			Assert.True(userResponse.obj.userLogin.PartyId == expectedProfileDetailResponse.userLogin.PartyId, "userResponse.obj.userLogin.PartyId != userLoginResponse.PartyId");
			Assert.NotNull(userResponse.obj.userLogin.RealPageId);
			Assert.True(userResponse.obj.userLogin.RealPageId == expectedProfileDetailResponse.userLogin.RealPageId, "userResponse.obj.userLogin.RealPageId != userLoginResponse.RealPageId");
			Assert.NotNull(userResponse.obj.userLogin.LoginName);
			Assert.True(userResponse.obj.userLogin.LoginName == expectedProfileDetailResponse.userLogin.LoginName, "userResponse.obj.userLogin.LoginName != userLoginResponse.LoginName");
			Assert.Null(userResponse.obj.userLogin.LoginNameType);

			if (userResponse.obj.userLogin.ThruDate == DateTime.Now)
			{
				Assert.NotNull(userResponse.obj.userLogin.IsActive);
				Assert.False(userResponse.obj.userLogin.IsActive, "userResponse.obj.userLogin.IsActive != false");
				Assert.NotNull(userResponse.obj.userLogin.IsExpired);
				Assert.True(userResponse.obj.userLogin.IsExpired, "userResponse.obj.userLogin.IsExpired != true");
			}
			else
			{
				Assert.NotNull(userResponse.obj.userLogin.IsActive);
				Assert.True(userResponse.obj.userLogin.IsActive, "userResponse.obj.userLogin.IsActive != True");
				Assert.NotNull(userResponse.obj.userLogin.IsExpired);
				Assert.False(userResponse.obj.userLogin.IsExpired, "userResponse.obj.userLogin.IsExpired != False");
			}

			Assert.NotNull(userResponse.obj.userLogin.IsLocked);
			Assert.False(userResponse.obj.userLogin.IsLocked, "userResponse.obj.userLogin.IsLocked != False");
			Assert.NotNull(userResponse.obj.userLogin.IsPending);
			Assert.False(userResponse.obj.userLogin.IsPending, "userResponse.obj.userLogin.IsPending != False");
			Assert.NotNull(userResponse.obj.userLogin.PasswordModifiedDate);
			Assert.NotNull(userResponse.obj.userLogin.FromDate);
			Assert.True(userResponse.obj.userLogin.FromDate == expectedProfileDetailResponse.userLogin.FromDate, "userResponse.obj.userLogin.FromDate != userLoginResponse.FromDate");
			//Assert.NotNull(userResponse.obj.userLogin.ThruDate);
			Assert.True(userResponse.obj.userLogin.ThruDate == expectedProfileDetailResponse.userLogin.ThruDate, "userResponse.obj.userLogin.ThruDate != userLoginResponse.ThruDate");
			Assert.NotNull(userResponse.obj.userLogin.IsSuperUser);
			Assert.True(userResponse.obj.userLogin.IsSuperUser == expectedProfileDetailResponse.userLogin.IsSuperUser, "userResponse.obj.userLogin.IsSuperUser != userLoginResponse.IsSuperUser");
			Assert.NotNull(userResponse.obj.userLogin.Status);
			Assert.True(userResponse.obj.userLogin.Status == expectedProfileDetailResponse.userLogin.Status, "userResponse.obj.userLogin.Status != userLoginResponse.Status");
			Assert.True(userResponse.obj.userLogin.UserRoleType == expectedProfileDetailResponse.userLogin.UserRoleType, "userResponse.obj.userLogin.UserRoleType != userLoginResponse.UserRoleType");
			Assert.NotNull(userResponse.obj.userLogin.Is3rdPartyIDP);
			Assert.False(userResponse.obj.userLogin.Is3rdPartyIDP, "userResponse.obj.userLogin.Is3rdPartyIDP != false");
		}

		//[Fact, Trait("", "Happy Path")]
		public void GetUserValidate()
		{
			//Set up the API URL
			payload = reusable.DoPostNewUserPayload(newLoginName);

			EndPointUrl = HostUrl + Properties["User"];
			
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(ResponseString);

			EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(newLoginName)
				+ "&newUserRegistrationToken=" + createUserResponse.UserToken;
						
			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			ValidateUserResponse validateUserResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validateUserResponse.EnterpriseUserName);
			Assert.True(validateUserResponse.EnterpriseUserName == newLoginName, "validateUserResponse.EnterpriseUserName == newLoginName");
			Assert.NotNull(validateUserResponse.ValidateUserToken);
			Assert.True(validateUserResponse.ValidateUserToken.Length > 0, "validateUserResponse.ValidateUserToken.Length > 0");
			Assert.NotNull(validateUserResponse.IsError);
			Assert.False(validateUserResponse.IsError, "validateUserResponse.IsError");
			Assert.Null(validateUserResponse.ErrorReason);
		}

        [Fact, Trait("", "Happy Path")]
        public void GetUserValidateToken()
        {
            ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData> roleType;
            string userLoginsUser;

            EndPointUrl = HostUrl + Properties["RoleType"] + WebUtility.UrlEncode("User Role");
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            roleType = JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData>>(ResponseString);

            userLoginsUser = "rpgreenbooksintegration" + Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";

            //Set up the API URL
            payload = reusable.DoPostNewUserPayload(newLoginName);

            var verificationToken = reusable.DoPostNewUserToken(payload, roleType.list[0].PartyRoleTypeId);

            EndPointUrl = HostUrl + Properties["User"] + "/Validate-token?enterpriseUserName=" + WebUtility.UrlEncode(newLoginName)
                + "&verificationToken=" + verificationToken;

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            ValidateUserResponse validateUserResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(validateUserResponse.EnterpriseUserName);
            Assert.True(validateUserResponse.EnterpriseUserName == newLoginName, "validateUserResponse.EnterpriseUserName == newLoginName");
            Assert.NotNull(validateUserResponse.ValidateUserToken);
            Assert.True(validateUserResponse.ValidateUserToken.Length > 0, "validateUserResponse.ValidateUserToken.Length > 0");
            Assert.NotNull(validateUserResponse.IsError);
            Assert.False(validateUserResponse.IsError, "validateUserResponse.IsError");
            Assert.Null(validateUserResponse.ErrorReason);
        }


        //[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetUserValidateInvalidUsernameAndNoToken", "invalidUsername", null)]
		[InlineData("GetUserValidateValidUsernameAndNoToken", "", null)]
		[InlineData("GetUserValidateNoUsernameAndToken", null, null)]
		public void GetUserValidateNegativeCasesInternalServerError(string testCase, string username = "", string token = "")
		{
			//Set up the API URL
			// username
			if (username != null && username.Length == 0)
			{
				username = newLoginName;
			}
			// token
			if (token != null && token.Length == 0)
			{
				token = createUserResponse.UserToken;
			}
			EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + username
				+ "&newUserRegistrationToken=" + token;

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.StartsWith("Internal System Error. Please contact RealPage support with error reference Id - ")
				, "ResponseString.StartsWith(\"Internal System Error.Please contact RealPage support with error reference Id - \")");
		}
		
		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetUserValidateValidUsernameAndInvalidToken", "Validation token does not match with user.", "", "invalidToken")]
		[InlineData("GetUserValidateInvalidUsernameAndToken", "User login information is missing.", "invalidUsername", "invalidToken")]
		//[InlineData("GetUserValidatePreviouslyCompletedProfile", "Profile already completed.")]
		public void GetUserValidateNegativeCases(string testCase, string errorReason, string username = "", string token = "")
		{
			//Set up the API URL
			// username
			if (username.Length == 0)
			{
				username = existingLoginName;
			}
			// token
			if (token.Length == 0)
			{
				token = createUserResponse.UserToken;
			}
			EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + username.Replace(" ", "")
				+ "&newUserRegistrationToken=" + token.Replace(" ", "");

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			ValidateUserResponse validateUserResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validateUserResponse.EnterpriseUserName);
			Assert.True(validateUserResponse.EnterpriseUserName == username
				, $"validateUserResponse.EnterpriseUserName is not \"{username}\".");
			Assert.Null(validateUserResponse.ValidateUserToken);
			Assert.NotNull(validateUserResponse.IsError);
			Assert.True(validateUserResponse.IsError, "validateUserResponse.IsError");
			Assert.NotNull(validateUserResponse.ErrorReason);
			Assert.True(validateUserResponse.ErrorReason == errorReason
				, $"validateUserResponse.ErrorReason is not \"{errorReason}\"");
		}
		
		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("PutUserProfileDetailUser", "Regular User", "1485150@ApiTest.com")]
		//[InlineData("PutUserProfileDetailUserSuperUser", "RealPage System Administrator", "e170be7@ApiTest.com")]
		[InlineData("PutUserProfileDetailUserWithoutEmail", "Regular User (No Email)", "7b445722017")]
		public void PutUserProfileDetailHappyPaths(string testCase, string userType, string username)
		{
			// Set up Payload
			payload = reusable.DoPutUserProfileDetailPayload(username, userType);

			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["User"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectOutput<RepositoryResponse, ErrorData> putUserProfileDetailResponse = JsonConvert.DeserializeObject<
				ObjectOutput<RepositoryResponse, ErrorData>>(ResponseString);

			// Revert User Updates if LoginName/Username is updated
			XunitTestOutPut.WriteLine("\n\nRevert User Updates if LoginName/Username is updated:\n");
			ProfileDetail profileDetail = JsonConvert.DeserializeObject<ProfileDetail>(payload);
			profileDetail.FirstName = "Nigel";
			profileDetail.LastName = "Lopez";
			payload = JsonConvert.SerializeObject(profileDetail);
			XunitTestOutPut.WriteLine("Payload to revert User Updates:\n" + payload + "\nCalling " + HttpVerb.Put + " at " + EndPointUrl + " to revert changes.");
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(putUserProfileDetailResponse.obj.RealPageId);
			Assert.True(putUserProfileDetailResponse.obj.RealPageId == JsonConvert.DeserializeObject<ProfileDetail>(payload).RealPageId
				, "putUserProfileDetailResponse.obj.RealPageId == JsonConvert.DeserializeObject<ProfileDetail>(payload).RealPageId");
			Assert.NotNull(putUserProfileDetailResponse.obj.Id);
			Assert.True(putUserProfileDetailResponse.obj.Id == 0, "putUserProfileDetailResponse.obj.Id == 0");
			Assert.NotNull(putUserProfileDetailResponse.Status.Success);
			Assert.True(putUserProfileDetailResponse.Status.Success, "putUserProfileDetailResponse.Status.Success");
			Assert.NotNull(putUserProfileDetailResponse.Status.ErrorCode);
			Assert.True(putUserProfileDetailResponse.Status.ErrorCode == ""
				, "putUserProfileDetailResponse.Status.ErrorCode == \"\"");
			Assert.NotNull(putUserProfileDetailResponse.Status.ErrorMsg);
			Assert.True(putUserProfileDetailResponse.Status.ErrorMsg == ""
				, "putUserProfileDetailResponse.Status.ErrorMsg == \"\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PutUserProfileDetailNullUserType()
		{
			// Set up Payload
			payload = reusable.DoPutUserProfileDetailPayload(existingLoginName);
			payload = payload.Replace("401", "null");
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL			
			EndPointUrl = HostUrl + Properties["User"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("The request is invalid.")
				, "ResponseString.Contains(\"The request is invalid.\")");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		// userType = Regular User
		[InlineData("PutUserProfileDetailWhiteSpaceUsername", "Username is required.", "Regular User", " ")]
		[InlineData("PutUserProfileDetailNoUsername", "Username is required.", "Regular User", "")]
		[InlineData("PutUserProfileDetailNullUsername", "Username is required.", "Regular User", null)]
		[InlineData("PutUserProfileDetailExistingUsername", "Update User: User already exists", "Regular User", "nigel.lopez@test.com")]
		// userType = RealPage System Administrator
		[InlineData("PutUserProfileDetailSuperUserWhiteSpaceUsername", "Username is required.", "RealPage System Administrator", " ")]
		[InlineData("PutUserProfileDetailSuperUserNoUsername", "Username is required.", "RealPage System Administrator", "")]
		[InlineData("PutUserProfileDetailSuperUserNullUsername", "Username is required.", "RealPage System Administrator", null)]
		[InlineData("PutUserProfileDetailSuperUserExistingUsername", "Update User: User already exists", "RealPage System Administrator", "nigel.lopez@test.com")]
		// userType = Regular User (No Email)
		[InlineData("PutUserProfileDetailWithoutEmailWhiteSpaceUsername", "Username is required.", "Regular User (No Email)", " ")]
		[InlineData("PutUserProfileDetailWithoutEmailNoUsername", "Username is required.", "Regular User (No Email)", "")]
		[InlineData("PutUserProfileDetailWithoutEmailNullUsername", "Username is required.", "Regular User (No Email)", null)]
		[InlineData("PutUserProfileDetailWithoutEmailExistingUsername", "Update User: User already exists", "Regular User (No Email)", "nigel.lopez@test.com")]
		// userType = Invalid User Type
		[InlineData("PutUserProfileDetailInvalidUserType", "Invalid user type.", "0")]
		[InlineData("PutUserProfileDetailNullUserType", "Invalid user type.")]
		public void PutUserProfileDetailNegativeCases(string testCase, string errorReason, string userType = "Regular User", string username = "AutoUsername")
		{
			// Set up Payload
			ProfileDetail profileDetail = JsonConvert.DeserializeObject<ProfileDetail>(reusable.DoPutUserProfileDetailPayload(existingLoginName, userType));
			profileDetail.userLogin.LoginName = (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrEmpty(username) && username != "nigel.lopez@test.com") ? newLoginName : username;

			payload = JsonConvert.SerializeObject(profileDetail);
			if (testCase == "PutUserProfileDetailNullUserType")
			{
				payload = payload.Replace("401", "null");
			}
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["User"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectOutput<ProfileDetail, ErrorData> putUserProfileDetailResponse = JsonConvert.DeserializeObject<
				ObjectOutput<ProfileDetail, ErrorData>>(ResponseString);

			// Revert User Updates if LoginName/Username is updated
			if (string.IsNullOrWhiteSpace(putUserProfileDetailResponse.obj.userLogin.LoginName) && string.IsNullOrEmpty(putUserProfileDetailResponse.obj.userLogin.LoginName) && putUserProfileDetailResponse.obj.userLogin.LoginName == "nigel.lopez@test.com")
			{
				XunitTestOutPut.WriteLine("\n\nRevert User Updates if LoginName/Username is updated:\n");
				profileDetail = JsonConvert.DeserializeObject<ProfileDetail>(payload);
				profileDetail.userLogin.LoginName = existingLoginName;
				profileDetail.FirstName = "Nigel";
				profileDetail.LastName = "Lopez";
				payload = JsonConvert.SerializeObject(profileDetail);
				XunitTestOutPut.WriteLine("Payload to revert User Updates:\n" + payload + "\nCalling " + HttpVerb.Put + " at " + EndPointUrl + " to revert changes.");
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);
			}

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//Assert.NotNull(putUserProfileDetailResponse.obj.RealPageId);
			//Assert.False(putUserProfileDetailResponse.obj.RealPageId == JsonConvert.DeserializeObject<ProfileDetail>(payload).RealPageId
			//	, "putUserProfileDetailResponse.obj.RealPageId == JsonConvert.DeserializeObject<ProfileDetail>(payload).RealPageId");
			Assert.NotNull(putUserProfileDetailResponse.Status.Success);
			Assert.False(putUserProfileDetailResponse.Status.Success, "putUserProfileDetailResponse.Status.Success is true.");
			Assert.NotNull(putUserProfileDetailResponse.Status.ErrorCode);
			Assert.True(putUserProfileDetailResponse.Status.ErrorCode.Contains("User.UpdateUser")
				, "putUserProfileDetailResponse.Status.ErrorCode does not contain \"User.UpdateUser\".");
			Assert.NotNull(putUserProfileDetailResponse.Status.ErrorMsg);
			Assert.True(putUserProfileDetailResponse.Status.ErrorMsg == errorReason
				, $"putUserProfileDetailResponse.Status.ErrorMsg is not {errorReason}.");
		}

		[Theory]
		[Trait("Data-Driven", "Happy Path")]
		[InlineData("PostNewUser", "Regular User")]
		[InlineData("PostNewUserSuperUser", "RealPage System Administrator")]
		[InlineData("PostNewUserWithoutEmail", "Regular User (No Email)")]
		public void PostNewUserHappyPaths(string testCase, string userType, bool isThirdParty = false)
		{
            // regular user - 401
            // regular user (no email) - 404
            // realpage system administrator - 400
            
            // Set up Payload
            if (testCase == "PostNewUserWithoutEmail")
			{
				newLoginName = newLoginName.Replace("@ApiTest.com", "2018");
			}
			payload = reusable.DoPostNewUserPayload(newLoginName, "AutoFirstName", "AutoLastName", "", userType);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["User"];

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
			Assert.True(createUserResponse.UserStatus == "User created successfully."
				, "createUserResponse.UserStatus == \"User created successfully.\"");			
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
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
		[InlineData("PostNewUserExistingUsername", "The User Login already exists.", "Regular User", "james@test.com")]
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
		[InlineData("PostNewUserSuperUserExistingUsername", "The User Login already exists.", "RealPage System Administrator", "james@test.com")]
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
		[InlineData("PostNewUserWithoutEmailExistingUsername", "The User Login already exists.", "Regular User (No Email)", "james@test.com")]
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
			EndPointUrl = HostUrl + Properties["User"];

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
			Assert.True(createUserResponse.Status.ErrorCode.Contains("User.CreateUser"), "createUserResponse.Status.ErrorCode doesn't contain \"10002\".");
			Assert.NotNull(createUserResponse.Status.ErrorMsg);
			Assert.True(createUserResponse.Status.ErrorMsg == errorReason
				, $"createUserResponse.Status.ErrorMsg is not \"{errorReason}\"");
		}





        //[Fact, Trait("", "Perf")]
        public void PostNewUserPerf()
        {
            // regular user - 401
            // regular user (no email) - 404
            // realpage system administrator - 400


            for (int i=4000; i<=4750;i++)
            { 

            // Set up Payload
            string userType = "Regular User (No Email)";
            bool isThirdParty = false;

            newLoginName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
            newLoginName = "KK" + newLoginName.Replace("@ApiTest.com", "Perf.com")+i;
            
            
            payload = reusable.DoPostNewUserPerfPayload(newLoginName, "AutoFirstName", "AutoLastName", "", userType);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["User"];

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
            Assert.True(createUserResponse.UserStatus == "User created successfully."
                , "createUserResponse.UserStatus == \"User created successfully.\"");

            Thread.Sleep(1000);
        }

        }


    }
}
