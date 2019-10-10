using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Linq;

namespace GreenBook.Tests
{
    public class SetPassword : TestController
	{
        private string userLoginsUser;
        private ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData> roleType;

        public SetPassword(ITestOutputHelper _xUnitTestOutput)
		{
            reusable = new TestUtilities(this);
            this.XunitTestOutPut = _xUnitTestOutput;

/*
            EndPointUrl = HostUrl + Properties["RoleType"] + WebUtility.UrlEncode("User Role");
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            roleType = JsonConvert.DeserializeObject<ObjectListOutput<
                RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData>>(ResponseString);

            userLoginsUser = "rpgreenbooksintegration" + Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
            payload = reusable.DoPostNewUserPayload(userLoginsUser);

            var verificationToken = reusable.DoPostNewUserToken(payload, roleType.list[0].PartyRoleTypeId);

            EndPointUrl = HostUrl + Properties["User"] + "/Validate-token?enterpriseUserName=" + WebUtility.UrlEncode(userLoginsUser)
                + "&verificationToken=" + verificationToken;

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            ValidateUserResponse validateUserResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

            payload = reusable.DoPostSetPasswordPayload(userLoginsUser, verificationToken);

            EndPointUrl = HostUrl + Properties["SetPassword"];
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            var s = ResponseString;
*/
        }

		private string payload, getUserValidateResponsePath, getUserValidateResponse;
        JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private ValidateUserResponse validateUserResponse;
		private int passwordLength;

        // SetPassword=/api/credential/SetPassword

        [Fact, Trait("", "Happy Path")]
        public void PostSetPassword()
        {   
            EndPointUrl = HostUrl + Properties["RoleType"] + WebUtility.UrlEncode("User Role");
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            roleType = JsonConvert.DeserializeObject<ObjectListOutput<
                RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData>>(ResponseString);

            userLoginsUser = "rpgreenbooksintegration" + Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
            payload = reusable.DoPostNewUserPayload(userLoginsUser);

            var verificationToken = reusable.DoPostNewUserToken(payload, roleType.list[0].PartyRoleTypeId);

            EndPointUrl = HostUrl + Properties["User"] + "/Validate-token?enterpriseUserName=" + WebUtility.UrlEncode(userLoginsUser)
                + "&verificationToken=" + verificationToken;

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            ValidateUserResponse validateUserResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            payload = reusable.DoPostSetPasswordPayload(userLoginsUser, verificationToken);

            EndPointUrl = HostUrl + Properties["SetPassword"];
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            ChangePasswordResponse response = JsonConvert.DeserializeObject<ChangePasswordResponse>(ResponseString);

            Assert.NotNull(response.IsSuccess);
            Assert.NotNull(response.EnterpriseUserName);
            Assert.NotNull(response.IsError);
            Assert.NotNull(response.ErrorReason);

            //Additional Asserts
            Assert.True(response.IsSuccess == true);
            Assert.True(userLoginsUser == response.EnterpriseUserName);
            Assert.True(response.IsError == false);
            Assert.True(response.ErrorReason == "");
        }



        //[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("PostSetPassword")]
		[InlineData("PostSetPasswordAverageValidLength")]
		[InlineData("PostSetPasswordMaximumLength")]
		[InlineData("PostSetPasswordMinimumLengthPlusOne")]
		[InlineData("PostSetPasswordMaximumLengthMinusOne")]
		[InlineData("PostSetPasswordWithWhiteSpace")]
		[InlineData("PostSetPasswordWithThreeConsecutiveIdenticalCharacters")]
		[InlineData("PostSetPasswordRegularUserWithoutEmail")]
		public void PostSetPasswordHappyPaths(string testCase)
		{
			// Set up Payload
			string password = "AutoPassword";
			switch (testCase)
			{
				case "PostSetPasswordAverageValidLength":
					passwordLength = PasswordPolicyDefault.obj.MinimumLength + ((PasswordPolicyDefault.obj.MaximumLength - PasswordPolicyDefault.obj.MinimumLength) / 2);
					break;
				case "PostSetPasswordMaximumLength":
					passwordLength = PasswordPolicyDefault.obj.MaximumLength;
					break;
				case "PostSetPasswordMinimumLengthPlusOne":
					passwordLength = PasswordPolicyDefault.obj.MinimumLength + 1;
					break;
				case "PostSetPasswordMaximumLengthMinusOne":
					passwordLength = PasswordPolicyDefault.obj.MaximumLength - 1;
					break;
				case "PostSetPasswordWithWhiteSpace":
					password = string.Concat("Ab 0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength);
					break;
				case "PostSetPasswordWithThreeConsecutiveIdenticalCharacters":
					password = string.Concat("Abbb0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MaximumLength);
					break;
			}
			payload = reusable.DoPostSetPasswordPayload(validateUserResponse.EnterpriseUserName
				, validateUserResponse.ValidateUserToken, password, passwordLength);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ChangePasswordResponse setPassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(setPassword.EnterpriseUserName);
			Assert.True(setPassword.EnterpriseUserName == validateUserResponse.EnterpriseUserName, "setPassword.EnterpriseUserName == Properties[\"enterpriseUsername\"]");
			Assert.NotNull(setPassword.IsSuccess);
			Assert.True(setPassword.IsSuccess, "setPassword.isSuccess");
			Assert.NotNull(setPassword.IsError);
			Assert.False(setPassword.IsError, "setPassword.isError");
			Assert.NotNull(setPassword.ErrorReason);
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("PostSetPasswordNullUsername", "InternalServerError", null)]
		[InlineData("PostSetPasswordNullPassword", "InternalServerError", "AutoUsername", null)]
		[InlineData("PostSetPasswordNoUsername", "BadRequest", "")]
		[InlineData("PostSetPasswordNoPassword", "BadRequest", "AutoUsername", "")]
		public void PostSetPasswordNegativeCases(string testCase, string httpStatusCode, string username = "AutoUsername", string password = "AutoPassword")
		{
			// Set up Payload
			username = string.IsNullOrEmpty(username) ? username : validateUserResponse.EnterpriseUserName;
			payload = reusable.DoPostSetPasswordPayload(username, validateUserResponse.ValidateUserToken, password);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.NotNull(ResponseString);
			if (httpStatusCode == "InternalServerError")
			{
				Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, $"ResponseHttpStatusCode is not \"{httpStatusCode}\".");
				Assert.True(ResponseString.Contains("Internal System Error. Please contact RealPage support with error reference Id - ")
					, "ResponseString.Contains(\"Internal System Error. Please contact RealPage support with error reference Id - \")");
			}
			else if (httpStatusCode == "BadRequest")
			{
				Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, $"ResponseHttpStatusCode is not \"{httpStatusCode}\".");
				Assert.Empty(ResponseString);
			}
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("PostSetPasswordInvalidUsername", "Username is incorrect or not found.", "invalidUsername")]
		[InlineData("PostSetPasswordNoActivityToken", "Set Password Activity Token is not specified.", "AutoUsername", "AutoPassword", "")]
		[InlineData("PostSetPasswordNullActivityToken", "Set Password Activity Token is not specified.", "AutoUsername", "AutoPassword", null)]
		[InlineData("PostSetPasswordInvalidActivityToken", "Activity Token is expired.", "AutoUsername", "AutoPassword", "invalidToken")]
		[InlineData("PostSetPasswordExpiredActivityToken", "Set Password Activity Token is not specified.")]
		[InlineData("PostSetPasswordMinimumLengthMinusOne", "Your password must be at least {0} characters.")]
		[InlineData("PostSetPasswordMaximumLengthPlusOne", "Your password must be {0} characters or less.")]
		[InlineData("PostSetPasswordNoUpperCaseLetter", "Your password must include an upper-case and a lower-case letter.")]
		[InlineData("PostSetPasswordNoLowerCaseLetter", "Your password must include an upper-case and a lower-case letter.")]
		[InlineData("PostSetPasswordNoSpecialCharacter", "Your password must include a special character.")]
		[InlineData("PostSetPasswordSameAsUsername", "Your password cannot be the same as your Username.")]
		[InlineData("PostSetPasswordNoNumber", "Your password must include a number.")]
		[InlineData("PostSetPasswordExternalIdentityProviderUser", "Set password is not applicable to users on external identity provider.")]
		[InlineData("PostSetPasswordTemporaryPassword", "Your password cannot be the same as your temporary password.", "AutoUsername", "P@ssw0rd")]
		public void PostSetPasswordNegativeCasesWithErrorReason(string testCase, string errorReason, string username = "AutoUsername", string password = "AutoPassword", string validateUserToken = "AutoToken")
		{
			// Set up Payload
			username = string.IsNullOrEmpty(username) || username != "AutoUsername" ? username : validateUserResponse.EnterpriseUserName;
			validateUserToken = string.IsNullOrEmpty(validateUserToken) || validateUserToken != "AutoToken" ? validateUserToken : validateUserResponse.ValidateUserToken;
			switch (testCase)
			{
				case "PostSetPasswordExpiredActivityToken":
					string PostNewUserResponsePath = DataPath + "PostNewUserResponse.json";
					string PostNewUserResponse = jsonManager.LoadJsonAsString(PostNewUserResponsePath);
					CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(PostNewUserResponse);
					EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(validateUserResponse.EnterpriseUserName)
						+ "&newUserRegistrationToken=" + createUserResponse.UserToken;
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
					validateUserResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
					validateUserToken = validateUserResponse.ValidateUserToken;
					// Reexecuting this statement to expire the ValidateUserToken:
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
					EndPointUrl = HostUrl + Properties["SetPassword"];
					break;
				case "PostSetPasswordMinimumLengthMinusOne":
					passwordLength = PasswordPolicyDefault.obj.MinimumLength - 1;
					errorReason = string.Format(errorReason, PasswordPolicyDefault.obj.MinimumLength);
					break;
				case "PostSetPasswordMaximumLengthPlusOne":
					passwordLength = PasswordPolicyDefault.obj.MaximumLength + 1;
					errorReason = string.Format(errorReason, PasswordPolicyDefault.obj.MaximumLength);
					break;
				case "PostSetPasswordNoUpperCaseLetter":
					password = string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength).ToLower();
					break;
				case "PostSetPasswordNoLowerCaseLetter":
					password = string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength).ToUpper();
					break;
				case "PostSetPasswordNoSpecialCharacter":
					password = string.Concat("Ab01t", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength);
					break;
				case "PostSetPasswordSameAsUsername":
					password = validateUserResponse.EnterpriseUserName;
					break;
				case "PostSetPasswordNoNumber":
					password = string.Concat("Ab!t", Guid.NewGuid().ToString("N").Select(c => (char)(c + 17))).Remove(PasswordPolicyDefault.obj.MinimumLength);
					break;
				case "PostSetPasswordExternalIdentityProviderUser":
					getUserValidateResponsePath = DataPath + "GetUserValidateExternalIdentityProviderResponse.json";
					getUserValidateResponse = jsonManager.LoadJsonAsString(getUserValidateResponsePath);
					validateUserResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(getUserValidateResponse);
					username = validateUserResponse.EnterpriseUserName;
					validateUserToken = validateUserResponse.ValidateUserToken;
					password = "P@ssw0rd";
					break;
			}
			payload = reusable.DoPostSetPasswordPayload(username, validateUserToken, password, passwordLength);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			ChangePasswordResponse setPassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.Null(setPassword.EnterpriseUserName);
			Assert.NotNull(setPassword.IsSuccess);
			Assert.False(setPassword.IsSuccess, "setPassword.isSuccess");
			Assert.NotNull(setPassword.IsError);
			Assert.True(setPassword.IsError, "setPassword.isError");
			Assert.NotNull(setPassword.ErrorReason);
			Assert.True(setPassword.ErrorReason == errorReason, $"setPassword.ErrorReason is not \"{errorReason}\"");
		}
	}
}
