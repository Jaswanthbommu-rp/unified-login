using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Linq;
using GreenBook.Models;

namespace GreenBook.Tests
{
    public class ForgotPassword : TestController
	{
		public ForgotPassword(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			DatabaseController dbManager = new DatabaseController(DbConnString);
			maxActivityAttemptCountQuestionAttempts = int.Parse(dbManager.executeQuery("SELECT DISTINCT TOP 1 [MaxActivityAttemptCount], [ActivityCode] "
				+ "FROM [Identity].[Ident].[Activity] WHERE ActivityCode = 'ForgotPassword'").Rows[0]["MaxActivityAttemptCount"].ToString());

			securityQuestionsApiResponse = reusable.DoGetSecurityQuestionsApiResponseForPayload();
			forgotPasswordUsername = JsonConvert.DeserializeObject<SecurityQuestionResponse>(securityQuestionsApiResponse).EnterpriseUserName;

			PasswordPolicyDefault = reusable.DoGetPasswordPolicy();
			passwordLength = PasswordPolicyDefault.obj.MinimumLength;
		}
        JsonController jsonManager = new JsonController();
        TestUtilities reusable;
        private readonly ITestOutputHelper XunitTestOutPut;
        string[] listOfPasswords = new string[6];
		private int maxActivityAttemptCountQuestionAttempts, passwordLength;
		private string payload, forgotPasswordUsername, securityQuestionsApiResponse;

		//ForgotPassword=/api/credential/ForgotPassword

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("PostForgotPassword")]
		[InlineData("PostForgotPasswordAverageValidLength")]
		[InlineData("PostForgotPasswordMaximumLength")]
		[InlineData("PostForgotPasswordMinimumLengthPlusOne")]
		[InlineData("PostForgotPasswordMaximumLengthMinusOne")]
		[InlineData("PostForgotPasswordWithThreeConsecutiveIdenticalCharacters")]
		[InlineData("PostForgotPasswordWithWhiteSpace")]
		public void PostForgotPasswordHappyPaths(string testCase)
		{
			// Set up Payload
			string password = "ValidPassword";
			switch (testCase)
			{
				case "PostForgotPasswordAverageValidLength":
					passwordLength = PasswordPolicyDefault.obj.MinimumLength + ((PasswordPolicyDefault.obj.MaximumLength - PasswordPolicyDefault.obj.MinimumLength) / 2);
					break;
				case "PostForgotPasswordMaximumLength":
					passwordLength = PasswordPolicyDefault.obj.MaximumLength;
					break;
				case "PostForgotPasswordMinimumLengthPlusOne":
					passwordLength = PasswordPolicyDefault.obj.MinimumLength + 1;
					break;
				case "PostForgotPasswordMaximumLengthMinusOne":
					passwordLength = PasswordPolicyDefault.obj.MaximumLength - 1;
					break;
				case "PostForgotPasswordWithThreeConsecutiveIdenticalCharacters":
					password = string.Concat("Pwww_0", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength);
					break;
				case "PostForgotPasswordWithWhiteSpace":
					password = string.Concat("Pw 0", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength);
					break;
			}
			payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, "ValidActivityToken", password, "ValidCorrectAnswerToken", passwordLength);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ForgotPassword"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response			
			ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(changePassword.EnterpriseUserName);
			Assert.True(changePassword.EnterpriseUserName == forgotPasswordUsername, "changePassword.enterpriseUserName == forgotPasswordUsername");
			Assert.NotNull(changePassword.IsSuccess);
			Assert.True(changePassword.IsSuccess, "changePassword.isSuccess");
			Assert.NotNull(changePassword.IsError);
			Assert.False(changePassword.IsError, "changePassword.isError");
			Assert.NotNull(changePassword.ErrorReason);
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("PostForgotPasswordNoPassword", "New Password is not specified.", "ValidUsername", "ValidActivityToken", "")]
		[InlineData("PostForgotPasswordNullPassword", "New Password is not specified.", "ValidUsername", "ValidActivityToken", null)]
		[InlineData("PostForgotPasswordNoUsername", "No Username specified.", "")]
		[InlineData("PostForgotPasswordNullUsername", "No Username specified.", null)]
		[InlineData("PostForgotPasswordInvalidUsername", "User name is incorrect or not found.", "invalidUsername")]
		[InlineData("PostForgotPasswordNoActivityToken", "Forgot Password Activity Token is not specified.", "ValidUsername", "")]
		[InlineData("PostForgotPasswordNullActivityToken", "Forgot Password Activity Token is not specified.", "ValidUsername", null)]
		[InlineData("PostForgotPasswordInvalidActivityToken", "Forgot Password Activity Token is expired.", "ValidUsername", "invalidActivityToken")]
		[InlineData("PostForgotPasswordExpiredActivityToken", "Forgot Password Activity Token is expired.")]
		[InlineData("PostForgotPasswordNoCorrectAnswerToken", "Correct Answer Token is not specified.", "ValidUsername", "ValidActivityToken", "ValidPassword", "")]
		[InlineData("PostForgotPasswordNullCorrectAnswerToken", "Correct Answer Token is not specified.", "ValidUsername", "ValidActivityToken", "ValidPassword", null)]
		[InlineData("PostForgotPasswordInvalidCorrectAnswerToken", "Correct Answer Token is expired.", "ValidUsername", "ValidActivityToken", "ValidPassword", "invalidCorrectAnswerToken")]
		[InlineData("PostForgotPasswordExpiredCorrectAnswerToken", "Correct Answer Token is expired.")]
		[InlineData("PostForgotPasswordMinimumLengthMinusOne", "Your password must be at least 8 characters.")]
		[InlineData("PostForgotPasswordMaximumLengthPlusOne", "Your password must be 20 characters or less.")]
		[InlineData("PostForgotPasswordNoUpperCaseLetter", "Your password must include minimum 1 lower case characters and minimum 1 upper case characters.")]
		[InlineData("PostForgotPasswordNoLowerCaseLetter", "Your password must include minimum 1 lower case characters and minimum 1 upper case characters.")]
		[InlineData("PostForgotPasswordNoSpecialCharacter", "Your password must include a special character.")]
		[InlineData("PostForgotPasswordSameAsUsername", "Your password cannot be the same as your Username.")]
		[InlineData("PostForgotPasswordNoNumber", "Your password must include 1 numeric characters.")]
		public void PostForgotPasswordNegativeCases(string testCase, string errorReason, string username = "ValidUsername", string activityToken = "ValidActivityToken", string password = "ValidPassword", string correctAnswerToken = "ValidCorrectAnswerToken")
		{
			// Set up Payload
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordRequest = new RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword();
			securityQuestionsApiResponse = reusable.DoGetSecurityQuestionsApiResponseForPayload();
			XunitTestOutPut.WriteLine("GET GetSecurityQuestions API Response:\n" + securityQuestionsApiResponse);

			payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, activityToken, password, correctAnswerToken);
			switch (testCase)
			{
				default:
					if (string.IsNullOrEmpty(username) || username != "ValidUsername")
					{
						changePasswordRequest = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);
						changePasswordRequest.EnterpriseUserName = username;
						payload = JsonConvert.SerializeObject(changePasswordRequest);
					}
					break;
				case "PostForgotPasswordExpiredActivityToken":
					XunitTestOutPut.WriteLine("GET GetSecurityQuestions API Expired Response:\n" + securityQuestionsApiResponse);
					forgotPasswordUsername = JsonConvert.DeserializeObject<SecurityQuestionResponse>(securityQuestionsApiResponse).EnterpriseUserName;

					EndPointUrl = HostUrl + Properties["GetSecurityQuestions"] + forgotPasswordUsername;
					XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
					XunitTestOutPut.WriteLine("GET GetSecurityQuestions API Recently Generated Response:\n"
						+ reusable.DoResetMaximumActivityAttempts(forgotPasswordUsername, EndPointUrl, HttpVerb.Get));
					break;
				case "PostForgotPasswordExpiredCorrectAnswerToken":
					XunitTestOutPut.WriteLine("GET GetSecurityQuestions API Response:\n" + securityQuestionsApiResponse);
					forgotPasswordUsername = JsonConvert.DeserializeObject<SecurityQuestionResponse>(securityQuestionsApiResponse).EnterpriseUserName;

					// Creates initial payload:
					changePasswordRequest = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);

					// Expires initial payload's CorrectAnswerToken:
					reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, activityToken, password, correctAnswerToken);
					break;
				case "PostForgotPasswordMinimumLengthMinusOne":
					payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, activityToken, password, correctAnswerToken, PasswordPolicyDefault.obj.MinimumLength - 1);
					break;
				case "PostForgotPasswordMaximumLengthPlusOne":
					payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, activityToken, password, correctAnswerToken, PasswordPolicyDefault.obj.MaximumLength + 1);
					break;
				case "PostForgotPasswordNoUpperCaseLetter":
					payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, activityToken, string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength).ToLower(), correctAnswerToken);
					break;
				case "PostForgotPasswordNoLowerCaseLetter":
					payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, activityToken, string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength).ToUpper(), correctAnswerToken);
					break;
				case "PostForgotPasswordNoSpecialCharacter":
					payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, activityToken, string.Concat("Ab01t", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength), correctAnswerToken);
					break;
				case "PostForgotPasswordSameAsUsername":
					securityQuestionsApiResponse = reusable.DoGetSecurityQuestionsApiResponseForPayload(Properties["enterpriseUsernameForForgotPassword2"]);
					payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, activityToken, Properties["enterpriseUsernameForForgotPassword2"], correctAnswerToken);
					break;
				case "PostForgotPasswordNoNumber":
					payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, activityToken, string.Concat("Ab!t", Guid.NewGuid().ToString("N").Select(c => (char)(c + 17))).Remove(PasswordPolicyDefault.obj.MinimumLength), correctAnswerToken);
					break;
			}
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ForgotPassword"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.Null(changePassword.EnterpriseUserName);
			Assert.NotNull(changePassword.IsSuccess);
			Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
			Assert.NotNull(changePassword.IsError);
			Assert.True(changePassword.IsError, "changePassword.isError");
			Assert.NotNull(changePassword.ErrorReason);
			Assert.True(changePassword.ErrorReason == errorReason, $"changePassword.ErrorReason != \"{errorReason}\"");
		}
		
		//[Fact, Trait("", "Negative Case")]
		public void PostForgotPasswordLastFivePreviouslySavedPassword()
		{
			ChangePasswordResponse changePassword = new ChangePasswordResponse();

			if (PasswordPolicyDefault.obj.PreventPasswordReuse)
			{
				string newLoginName = Guid.NewGuid().ToString().Remove(7);
				payload = reusable.DoPostNewUserPayload(newLoginName, "AutoFirstName", "AutoLastName", "", "regular user (no email)");

				EndPointUrl = HostUrl + Properties["User"];
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
				CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(ResponseString);

				_accessToken = GetClientToken(Properties["identityClientUrl"], newLoginName, "P@ssw0rd");

				EndPointUrl = HostUrl + Properties["ProfileDetails"];
				XunitTestOutPut.WriteLine("\nCalling " + HttpVerb.Get + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
				var profileDetailsResponse = JsonConvert.DeserializeObject<ObjectOutput<ProfileDetailTestModel, ErrorData>>(ResponseString);

				EndPointUrl = HostUrl + Properties["User"] + "/Validate-token?enterpriseUserName=" + WebUtility.UrlEncode(newLoginName)
						+ "&verificationToken=" + profileDetailsResponse.obj.VerificationActivityToken;
				XunitTestOutPut.WriteLine("\nCalling " + HttpVerb.Get + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
				var validateUserResponse = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString);
				
				listOfPasswords = new string[PasswordPolicyDefault.obj.NumberOfPasswordsToRemember + 1];
				int countPasswords = 0;
				listOfPasswords[countPasswords] = "P@ssw0rd";

				for (countPasswords = 1; countPasswords < PasswordPolicyDefault.obj.NumberOfPasswordsToRemember + 1; countPasswords++)
				{
					listOfPasswords[countPasswords] = "P@ssw0rd" + countPasswords;
					switch (countPasswords)
					{
						case 1:
							payload = reusable.DoPostSetUserSecurityQuestionsPayload(newLoginName, validateUserResponse.ValidateUserToken);
							XunitTestOutPut.WriteLine("Payload:\n" + payload);

							//Set up the API URL
							EndPointUrl = HostUrl + Properties["SetUserSecurityQuestions"];

							//Execute API
							XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
							GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

							payload = reusable.DoPostSetPasswordPayload(validateUserResponse.EnterpriseUserName, validateUserResponse.ValidateUserToken, "P@ssw0rd1");
							XunitTestOutPut.WriteLine("Payload:\n" + payload);

							// Set up the API URL
							EndPointUrl = HostUrl + Properties["SetPassword"];
							break;
						default:
							securityQuestionsApiResponse = reusable.DoGetSecurityQuestionsApiResponseForPayload(newLoginName);

							payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, "ValidActivityToken", listOfPasswords[countPasswords], "ValidCorrectAnswerToken");
							XunitTestOutPut.WriteLine("Payload:\n" + payload);

							// Set up the API URL
							EndPointUrl = HostUrl + Properties["ForgotPassword"];
							break;
					}
					// Execute API
					XunitTestOutPut.WriteLine("\nCalling " + HttpVerb.Post + " at " + EndPointUrl);
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
				}

				for (countPasswords = PasswordPolicyDefault.obj.NumberOfPasswordsToRemember; countPasswords >= 0; countPasswords--)
				{
					// Set up Payload
					securityQuestionsApiResponse = reusable.DoGetSecurityQuestionsApiResponseForPayload(newLoginName);

					payload = reusable.DoPostChangePasswordPayload(securityQuestionsApiResponse, "ValidActivityToken", listOfPasswords[countPasswords], "ValidCorrectAnswerToken");
					XunitTestOutPut.WriteLine("Payload:\n" + payload);

					// Set up the API URL
					EndPointUrl = HostUrl + Properties["ForgotPassword"];

					// Execute API
					XunitTestOutPut.WriteLine("\nCalling " + HttpVerb.Post + " at " + EndPointUrl);
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

					// Extract API's JSON Response
					changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(ResponseString);
					XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

					// Assert
					Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == responseHttpStatusCode");
					Assert.NotNull(changePassword.IsSuccess);
					Assert.NotNull(changePassword.IsError);
					Assert.NotNull(changePassword.ErrorReason);
					switch (countPasswords)
					{
						default:
							Assert.Null(changePassword.EnterpriseUserName);
							Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
							Assert.True(changePassword.IsError, "changePassword.isError");
							Assert.True(changePassword.ErrorReason == "Your password should not be from past 5 passwords", "changePassword.ErrorReason is not \"Your password should not be from past 5 passwords\".");
							break;
						case 0:
							Assert.NotNull(changePassword.EnterpriseUserName);
							Assert.True(changePassword.EnterpriseUserName == newLoginName, "changePassword.isSuccess");
							Assert.True(changePassword.IsSuccess, "changePassword.isSuccess");
							Assert.False(changePassword.IsError, "changePassword.isError");
							Assert.NotEmpty(changePassword.ErrorReason);
							break;
					}
				}
			}
		}
	}
}
