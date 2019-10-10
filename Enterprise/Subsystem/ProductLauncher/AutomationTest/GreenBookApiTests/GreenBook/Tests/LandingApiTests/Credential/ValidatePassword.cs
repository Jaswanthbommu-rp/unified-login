using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Linq;

namespace GreenBook.Tests
{
    public class ValidatePassword : TestController
	{
		public ValidatePassword(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			PasswordPolicyDefault = reusable.DoGetPasswordPolicy();

			validatePasswordUsername = Properties["enterpriseUsername"];
		}

		//private string payload;
        JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string validatePasswordUsername;

		// ValidatePassword=/api/credential/ValidatePassword

		[Fact, Trait("", "Happy Path")]
		public void PostValidatePassword()
        {
			// Set up Payload
			//payload = reusable.DoPostValidatePasswordPayload(int.Parse(Properties["PortfolioId"]), Properties["enterpriseUsername"], null, PasswordPolicyDefault.obj.MinimumLength);
			//XunitTestOutPut.WriteLine("Payload:\n" + payload);
			//RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ValidatePassword validatePasswordPayload 
			//	= JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ValidatePassword>(payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength));

            // Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(validatePassword.IsSuccess);
            Assert.True(validatePassword.IsSuccess, "validatePassword.isSuccess");
            Assert.NotNull(validatePassword.IsError);
            Assert.False(validatePassword.IsError, "validatePassword.isError");
            Assert.NotNull(validatePassword.ErrorReason);
        }

		//[Fact, Trait("", "Data-Driven")]
		public void PostValidatePasswordAverageValidLength()
		{
			// Set up Payload
			//payload = reusable.DoPostValidatePasswordPayload(int.Parse(Properties["PortfolioId"]), Properties["enterpriseUsername"], null
			//	, int.Parse(Properties["MinimumLength"]) + ((int.Parse(Properties["MaximumLength"]) - int.Parse(Properties["MinimumLength"]))/2));
			//XunitTestOutPut.WriteLine("Payload:\n" + payload);
			//RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ValidatePassword validatePasswordPayload
			//	= JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ValidatePassword>(payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0!"
				, Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength 
				+ ((PasswordPolicyDefault.obj.MaximumLength - PasswordPolicyDefault.obj.MinimumLength)/2)));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.True(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.False(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
		}

		//[Fact, Trait("", "Data-Driven")]
		public void PostValidatePasswordMaximumLength()
		{
			// Set up Payload
			//payload = reusable.DoPostValidatePasswordPayload(int.Parse(Properties["PortfolioId"]), Properties["enterpriseUsername"], null, int.Parse(Properties["MaximumLength"]));
			//XunitTestOutPut.WriteLine("Payload:\n" + payload);
			//RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ValidatePassword validatePasswordPayload
			//	= JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ValidatePassword>(payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MaximumLength));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.True(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.False(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
		}

		//[Fact, Trait("", "Data-Driven")]
		public void PostValidatePasswordMinimumLengthPlusOne()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength + 1));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.True(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.False(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
		}

		//[Fact, Trait("", "Data-Driven")]
		public void PostValidatePasswordMaximumLengthMinusOne()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MaximumLength - 1));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.True(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.False(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordNoPassword()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" 
				+ WebUtility.UrlEncode(validatePasswordUsername) + "&passwordToValidate=";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("Internal System Error. Please contact RealPage support with error reference Id - ")
				, "ResponseString.Contains(\"Internal System Error. Please contact RealPage support with error reference Id - \")");
		}
		
		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordUserEmailAddressAsPassword()
		{
			// Set up Payload
			string realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", realPageId);
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress, IErrorData> electronicAddress
				= JsonConvert.DeserializeObject<ObjectListOutput<
					RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress, IErrorData>>(ResponseString);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName="
				+ WebUtility.UrlEncode(validatePasswordUsername) + "&passwordToValidate=" + electronicAddress.list[0].AddressString;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.True(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
			Assert.True(validatePassword.ErrorReason.Contains("Your password cannot contain your Email Address."), "validatePassword.ErrorReason.Contains(\"Your password cannot contain your Email Address.\")");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordNoUsername()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=&passwordToValidate="
				+ WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MaximumLength - 1));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("Internal System Error. Please contact RealPage support with error reference Id - ")
				, "ResponseString.Contains(\"Internal System Error. Please contact RealPage support with error reference Id - \")");
		}
		
		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordInvalidUsername()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=invalidUsername&passwordToValidate="
				+ WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MaximumLength - 1));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.True(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
			Assert.True(validatePassword.ErrorReason == "User name is incorrect or not found.", "validatePassword.ErrorReason == \"User name is incorrect or not found.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		//public void PostValidatePasswordFifthAmongTheFivePreviouslySavedPassword()
		//{
		//	RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword validatePasswordPayload;
		//	ChangePasswordResponse validatePassword = new ChangePasswordResponse();
		//	HttpStatusCode responseHttpStatusCode = new HttpStatusCode();

		//	if (Convert.ToBoolean(Properties["PreventPasswordReuse"]))
		//	{
		//		listOfPasswords = new string[Convert.ToInt32(Properties["NumberOfPasswordsToRemember"]) + 1];

		//		for (int countChangePassword = 0; countChangePassword < Convert.ToInt32(Properties["NumberOfPasswordsToRemember"]) + 1; countChangePassword++)
		//		{
		//			// Set up Payload for the List of Passwords to be tracked:
		//			payload = reusable.DoPostChangePasswordPayload(
		//				Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));

		//			validatePasswordPayload = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);

		//			listOfPasswords[countChangePassword] = validatePasswordPayload.NewPassword;

		//			XunitTestOutPut.WriteLine("Payload:\n" + payload);
		//			XunitTestOutPut.WriteLine("New Password for ChangePassword Attempt #" + (countChangePassword + 1) + ": " + validatePasswordPayload.NewPassword);

		//			// Set up the API URL
		//			EndPointUrl = HostUrl + Properties["ValidatePassword"];

		//			// Execute API
		//			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
		//			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//			// Extract API's JSON Response
		//			
		//			validatePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(ResponseString);
		//			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//		}

		//		// Set up Payload
		//		payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, listOfPasswords[0], null, int.Parse(Properties["MinimumLength"]));
		//		XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//		// Set up the API URL
		//		EndPointUrl = HostUrl + Properties["ValidatePassword"];

		//		// Execute API
		//		XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
		//		var responseForAssertion = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//		// Extract API's JSON Response
		//		var ResponseStringForAssertion = GetHttpWebResponseString(responseForAssertion);
		//		validatePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(ResponseStringForAssertion);
		//		XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + responseForAssertion.StatusCode + "\n\n" + ResponseStringForAssertion);
		//		responseHttpStatusCode = responseForAssertion.StatusCode;

		//		// Assert
		//		Assert.True(HttpStatusCode.OK == responseHttpStatusCode, "HttpStatusCode.OK == responseHttpStatusCode");
		//		Assert.Null(validatePassword.EnterpriseUserName);
		//		Assert.NotNull(validatePassword.IsSuccess);
		//		Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
		//		Assert.NotNull(validatePassword.IsError);
		//		Assert.True(validatePassword.IsError, "validatePassword.isError");
		//		Assert.NotNull(validatePassword.ErrorReason);
		//		Assert.True(validatePassword.ErrorReason == "This password has already been used. Please try again.", "validatePassword.ErrorReason == \"This password has already been used. Please try again.\"");

		//	}
		//}

		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordMinimumLengthMinusOne()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength - 1));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.True(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
			Assert.True(validatePassword.ErrorReason == "Your password must be at least " + PasswordPolicyDefault.obj.MinimumLength + " characters.", "validatePassword.ErrorReason == \"Your password must be at least " + PasswordPolicyDefault.obj.MinimumLength + " characters.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordMaximumLengthPlusOne()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MaximumLength + 1));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.True(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
			Assert.True(validatePassword.ErrorReason == "Your password must be " + PasswordPolicyDefault.obj.MaximumLength + " characters or less.", "validatePassword.ErrorReason == \"Your password must be " + PasswordPolicyDefault.obj.MaximumLength + " characters or less.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordNoUpperCaseLetter()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MaximumLength).ToLower());

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.True(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
			Assert.True(validatePassword.ErrorReason == "Your password must include an upper-case and a lower-case letter.", "validatePassword.ErrorReason == \"Your password must include an upper-case and a lower-case letter.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordNoLowerCaseLetter()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MaximumLength).ToUpper());

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.True(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
			Assert.True(validatePassword.ErrorReason == "Your password must include an upper-case and a lower-case letter.", "validatePassword.ErrorReason == \"Your password must include an upper-case and a lower-case letter.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordNoSpecialCharacter()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Ab0", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MaximumLength));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.True(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
			Assert.True(validatePassword.ErrorReason == "Your password must include a special character.", "validatePassword.ErrorReason == \"Your password must include a special character.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordSameAsUsername()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(validatePasswordUsername);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.True(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
			Assert.True(validatePassword.ErrorReason == "Your password cannot be the same as your Username.", "validatePassword.ErrorReason == \"Your password cannot be the same as your Username.\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostValidatePasswordNoNumber()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode((String.Concat("Pw_", Guid.NewGuid().ToString("N").Select(c => (char)(c + 17))).Remove(PasswordPolicyDefault.obj.MinimumLength)));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.False(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.True(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
			Assert.True(validatePassword.ErrorReason == "Your password must include a number.", "validatePassword.ErrorReason == \"Your password must include a number.\"");
		}

		//[Fact, Trait("", "Data-Driven")]
		public void PostValidatePasswordWithThreeConsecutiveIdenticalCharacters()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("Pwww_9", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.True(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.False(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
		}

		//[Fact, Trait("", "Data-Driven")]
		public void PostValidatePasswordWithWhiteSpace()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ValidatePassword"] + "?enterpriseUserName=" + WebUtility.UrlEncode(validatePasswordUsername)
				+ "&passwordToValidate=" + WebUtility.UrlEncode(string.Concat("P w9", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(PasswordPolicyDefault.obj.MinimumLength));

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			
			ValidatePasswordResponse validatePassword = JsonConvert.DeserializeObject<ValidatePasswordResponse>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(validatePassword.IsSuccess);
			Assert.True(validatePassword.IsSuccess, "validatePassword.isSuccess");
			Assert.NotNull(validatePassword.IsError);
			Assert.False(validatePassword.IsError, "validatePassword.isError");
			Assert.NotNull(validatePassword.ErrorReason);
		}
	}
}
