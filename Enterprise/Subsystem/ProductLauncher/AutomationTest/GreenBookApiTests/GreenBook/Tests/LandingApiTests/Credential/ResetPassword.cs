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
using System.Data;

namespace GreenBook.Tests
{
    public class ResetPassword : TestController
    {
        public ResetPassword(ITestOutputHelper _xUnitTestOutput)

		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
            loginUser = Properties["LoginUser"].Split('|');
            _accessToken = GetClientToken(Properties["identityClientUrl"], loginUser[0], loginUser[1]);
        }

        private string payload;
        private string payload1;
        private string payload2;
        private string payload3;
        private string payload4;
        private string payload5;
        private string payload6;
        private string[] loginUser;

        JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
        private string userLoginsUser;
        private ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData> roleType;

        // ResetPassword=/api/credential/ResetPassword

        //[Fact, Trait("", "Happy Path")]
        public void PostResetPassword()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload(loginUser[1], null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            string newoldPassword = resetPasswordPayload.NewPassword;
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
            
            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.True(resetPassword.IsSuccess, "resetPassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.False(resetPassword.IsError, "resetPassword.isError");
            Assert.Null(resetPassword.ErrorReason);

            //// Set up Payload to change back to Password First ChangePassword
            payload1 = reusable.DoPostResetPasswordPayload(newoldPassword, null);
            UserResetPassword resetPasswordPayload1 = JsonConvert.DeserializeObject<UserResetPassword>(payload1);
            string newoldPassword1 = resetPasswordPayload1.NewPassword;
            payload1 = JsonConvert.SerializeObject(resetPasswordPayload1);
            XunitTestOutPut.WriteLine("Payload:\n" + payload1);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload1);
            ResetPasswordResponse resetPassword1 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            //// Set up Payload to change back to Password Second ChangePassword
            payload2 = reusable.DoPostResetPasswordPayload(newoldPassword1, null);
            UserResetPassword resetPasswordPayload2 = JsonConvert.DeserializeObject<UserResetPassword>(payload2);
            string newoldPassword2 = resetPasswordPayload2.NewPassword;
            payload2 = JsonConvert.SerializeObject(resetPasswordPayload2);
            XunitTestOutPut.WriteLine("Payload:\n" + payload2);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload2);
            ResetPasswordResponse resetPassword2 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            //// Set up Payload to change back to Password Third ChangePassword
            payload3 = reusable.DoPostResetPasswordPayload(newoldPassword2, null);
            UserResetPassword resetPasswordPayload3 = JsonConvert.DeserializeObject<UserResetPassword>(payload3);
            string newoldPassword3 = resetPasswordPayload3.NewPassword;
            payload3 = JsonConvert.SerializeObject(resetPasswordPayload3);
            XunitTestOutPut.WriteLine("Payload:\n" + payload3);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload3);
            ResetPasswordResponse resetPassword3 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            //// Set up Payload to change back to Password Fourth ChangePassword
            payload4 = reusable.DoPostResetPasswordPayload(newoldPassword3, null);
            UserResetPassword resetPasswordPayload4 = JsonConvert.DeserializeObject<UserResetPassword>(payload4);
            string newoldPassword4 = resetPasswordPayload4.NewPassword;
            payload4 = JsonConvert.SerializeObject(resetPasswordPayload4);
            XunitTestOutPut.WriteLine("Payload:\n" + payload4);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload4);
            ResetPasswordResponse resetPassword4 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            //// Set up Payload to change back to Password Fifth ChangePassword
            payload5 = reusable.DoPostResetPasswordPayload(newoldPassword4, loginUser[1]);
            UserResetPassword resetPasswordPayload5 = JsonConvert.DeserializeObject<UserResetPassword>(payload5);
            string newoldPassword5 = resetPasswordPayload5.NewPassword;
            payload5 = JsonConvert.SerializeObject(resetPasswordPayload5);
            XunitTestOutPut.WriteLine("Payload:\n" + payload5);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload5);
            ResetPasswordResponse resetPassword5 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
/*
            //// Set up Payload to change back to P@ssw0rd
            payload6 = reusable.DoPostResetPasswordPayload(newoldPassword5, loginUser[1]);
            UserResetPassword resetPasswordPayload6 = JsonConvert.DeserializeObject<UserResetPassword>(payload6);
            string newoldPassword6 = resetPasswordPayload6.NewPassword;
            payload6 = JsonConvert.SerializeObject(resetPasswordPayload6);
            XunitTestOutPut.WriteLine("Payload:\n" + payload6);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload6);
            ResetPasswordResponse resetPassword6 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
*/
        }

        //[Fact, Trait("", "Data-Driven")]
        public void PostResetPasswordAverageValidLength()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload("P@ssw0rd", null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            string newoldPassword = resetPasswordPayload.NewPassword;
            resetPasswordPayload.NewPassword = string.Concat("P@ssw0rd", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(15);
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.True(resetPassword.IsSuccess, "resetPassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.False(resetPassword.IsError, "resetPassword.isError");
            Assert.Null(resetPassword.ErrorReason);

            //// Set up Payload to change back to Password First ChangePassword
            payload1 = reusable.DoPostResetPasswordPayload(newoldPassword, null);
            UserResetPassword resetPasswordPayload1 = JsonConvert.DeserializeObject<UserResetPassword>(payload1);
            string newoldPassword1 = resetPasswordPayload1.NewPassword;
            payload1 = JsonConvert.SerializeObject(resetPasswordPayload1);
            XunitTestOutPut.WriteLine("Payload:\n" + payload1);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload1);
            ResetPasswordResponse resetPassword1 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to Password Second ChangePassword
            payload2 = reusable.DoPostResetPasswordPayload(newoldPassword1, null);
            UserResetPassword resetPasswordPayload2 = JsonConvert.DeserializeObject<UserResetPassword>(payload2);
            string newoldPassword2 = resetPasswordPayload2.NewPassword;
            payload2 = JsonConvert.SerializeObject(resetPasswordPayload2);
            XunitTestOutPut.WriteLine("Payload:\n" + payload2);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload2);
            ResetPasswordResponse resetPassword2 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to Password Third ChangePassword
            payload3 = reusable.DoPostResetPasswordPayload(newoldPassword2, null);
            UserResetPassword resetPasswordPayload3 = JsonConvert.DeserializeObject<UserResetPassword>(payload3);
            string newoldPassword3 = resetPasswordPayload3.NewPassword;
            payload3 = JsonConvert.SerializeObject(resetPasswordPayload3);
            XunitTestOutPut.WriteLine("Payload:\n" + payload3);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload3);
            ResetPasswordResponse resetPassword3 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to Password Fourth ChangePassword
            payload4 = reusable.DoPostResetPasswordPayload(newoldPassword3, null);
            UserResetPassword resetPasswordPayload4 = JsonConvert.DeserializeObject<UserResetPassword>(payload4);
            string newoldPassword4 = resetPasswordPayload4.NewPassword;
            payload4 = JsonConvert.SerializeObject(resetPasswordPayload4);
            XunitTestOutPut.WriteLine("Payload:\n" + payload4);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload4);
            ResetPasswordResponse resetPassword4 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to Password Fifth ChangePassword
            payload5 = reusable.DoPostResetPasswordPayload(newoldPassword4, null);
            UserResetPassword resetPasswordPayload5 = JsonConvert.DeserializeObject<UserResetPassword>(payload5);
            string newoldPassword5 = resetPasswordPayload5.NewPassword;
            payload5 = JsonConvert.SerializeObject(resetPasswordPayload5);
            XunitTestOutPut.WriteLine("Payload:\n" + payload5);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload5);
            ResetPasswordResponse resetPassword5 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to P@ssw0rd
            payload6 = reusable.DoPostResetPasswordPayload(newoldPassword5, "P@ssw0rd");
            UserResetPassword resetPasswordPayload6 = JsonConvert.DeserializeObject<UserResetPassword>(payload6);
            string newoldPassword6 = resetPasswordPayload6.NewPassword;
            payload6 = JsonConvert.SerializeObject(resetPasswordPayload6);
            XunitTestOutPut.WriteLine("Payload:\n" + payload6);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload6);
            ResetPasswordResponse resetPassword6 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
        }

        //[Fact, Trait("", "Data-Driven")]
        public void PostResetPasswordMaximumLength()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload("P@ssw0rd", null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            string newoldPassword = resetPasswordPayload.NewPassword;
            resetPasswordPayload.NewPassword = string.Concat("P@ssw0rd", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(20);
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);


            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.True(resetPassword.IsSuccess, "resetPassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.False(resetPassword.IsError, "resetPassword.isError");
            Assert.Null(resetPassword.ErrorReason);

            //// Set up Payload to change back to Password First ChangePassword
            payload1 = reusable.DoPostResetPasswordPayload(newoldPassword, null);
            UserResetPassword resetPasswordPayload1 = JsonConvert.DeserializeObject<UserResetPassword>(payload1);
            string newoldPassword1 = resetPasswordPayload1.NewPassword;
            payload1 = JsonConvert.SerializeObject(resetPasswordPayload1);
            XunitTestOutPut.WriteLine("Payload:\n" + payload1);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload1);
            ResetPasswordResponse resetPassword1 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to Password Second ChangePassword
            payload2 = reusable.DoPostResetPasswordPayload(newoldPassword1, null);
            UserResetPassword resetPasswordPayload2 = JsonConvert.DeserializeObject<UserResetPassword>(payload2);
            string newoldPassword2 = resetPasswordPayload2.NewPassword;
            payload2 = JsonConvert.SerializeObject(resetPasswordPayload2);
            XunitTestOutPut.WriteLine("Payload:\n" + payload2);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload2);
            ResetPasswordResponse resetPassword2 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to Password Third ChangePassword
            payload3 = reusable.DoPostResetPasswordPayload(newoldPassword2, null);
            UserResetPassword resetPasswordPayload3 = JsonConvert.DeserializeObject<UserResetPassword>(payload3);
            string newoldPassword3 = resetPasswordPayload3.NewPassword;
            payload3 = JsonConvert.SerializeObject(resetPasswordPayload3);
            XunitTestOutPut.WriteLine("Payload:\n" + payload3);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload3);
            ResetPasswordResponse resetPassword3 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to Password Fourth ChangePassword
            payload4 = reusable.DoPostResetPasswordPayload(newoldPassword3, null);
            UserResetPassword resetPasswordPayload4 = JsonConvert.DeserializeObject<UserResetPassword>(payload4);
            string newoldPassword4 = resetPasswordPayload4.NewPassword;
            payload4 = JsonConvert.SerializeObject(resetPasswordPayload4);
            XunitTestOutPut.WriteLine("Payload:\n" + payload4);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload4);
            ResetPasswordResponse resetPassword4 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to Password Fifth ChangePassword
            payload5 = reusable.DoPostResetPasswordPayload(newoldPassword4, null);
            UserResetPassword resetPasswordPayload5 = JsonConvert.DeserializeObject<UserResetPassword>(payload5);
            string newoldPassword5 = resetPasswordPayload5.NewPassword;
            payload5 = JsonConvert.SerializeObject(resetPasswordPayload5);
            XunitTestOutPut.WriteLine("Payload:\n" + payload5);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload5);
            ResetPasswordResponse resetPassword5 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);

            //// Set up Payload to change back to P@ssw0rd
            payload6 = reusable.DoPostResetPasswordPayload(newoldPassword5, "P@ssw0rd");
            UserResetPassword resetPasswordPayload6 = JsonConvert.DeserializeObject<UserResetPassword>(payload6);
            string newoldPassword6 = resetPasswordPayload6.NewPassword;
            payload6 = JsonConvert.SerializeObject(resetPasswordPayload6);
            XunitTestOutPut.WriteLine("Payload:\n" + payload6);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload6);
            ResetPasswordResponse resetPassword6 = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
        }

        //[Fact, Trait("", "Negative Case")]
        public void PostResetPasswordMinimumLengthMinusOne()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload("P@ssw0rd", null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            resetPasswordPayload.NewPassword = string.Concat("P@ssw0rd", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(7);
            string newoldPassword = resetPasswordPayload.NewPassword;
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.False(resetPassword.IsSuccess, "resetPassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.True(resetPassword.IsError, "resetPassword.isError");
            Assert.NotNull(resetPassword.ErrorReason);
            Assert.True(resetPassword.ErrorReason == "Your password must be at least 8 characters.", 
                "resetPassword.ErrorReason == \"Your password must be at least 8 characters.\"");
        }

        //[Fact, Trait("", "Negative Case")]
        public void PostResetPasswordMaximumLengthPlusOne()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload("P@ssw0rd", null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            resetPasswordPayload.NewPassword = string.Concat("P@ssw0rd", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(21);
            string newoldPassword = resetPasswordPayload.NewPassword;
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.False(resetPassword.IsSuccess, "changePassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.True(resetPassword.IsError, "changePassword.isError");
            Assert.NotNull(resetPassword.ErrorReason);
            Assert.True(resetPassword.ErrorReason == "Your password must be 20 characters or less.", "resetPassword.ErrorReason == \"Your password must be 20 characters or less.\"");
        }

        //[Fact, Trait("", "Negative Case")]
        public void PostResetPasswordNoUpperCaseLetter()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload("P@ssw0rd", null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            resetPasswordPayload.NewPassword = string.Concat("P@ssw0rd", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(15).ToLower();
            string newoldPassword = resetPasswordPayload.NewPassword;
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.False(resetPassword.IsSuccess, "changePassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.True(resetPassword.IsError, "changePassword.isError");
            Assert.NotNull(resetPassword.ErrorReason);
            Assert.True(resetPassword.ErrorReason == "Your password must include an upper-case and a lower-case letter.", "resetPassword.ErrorReason == \"Your password must include an upper-case and a lower-case letter.\"");
        }

        //[Fact, Trait("", "Negative Case")]
        public void PostResetPasswordNoLowerCaseLetter()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload("P@ssw0rd", null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            resetPasswordPayload.NewPassword = string.Concat("P@ssw0rd", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(15).ToUpper();
            string newoldPassword = resetPasswordPayload.NewPassword;
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.False(resetPassword.IsSuccess, "resetPassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.True(resetPassword.IsError, "resetPassword.isError");
            Assert.NotNull(resetPassword.ErrorReason);
            Assert.True(resetPassword.ErrorReason == "Your password must include an upper-case and a lower-case letter.", "resetPassword.ErrorReason == \"Your password must include an upper-case and a lower-case letter.\"");
        }

        //[Fact, Trait("", "Negative Case")]
        public void PostResetPasswordNoSpecialCharacter()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload("P@ssw0rd", null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            resetPasswordPayload.NewPassword = string.Concat("Passw0rd", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(8);
            string newoldPassword = resetPasswordPayload.NewPassword;
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.False(resetPassword.IsSuccess, "resetPassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.True(resetPassword.IsError, "resetPassword.isError");
            Assert.NotNull(resetPassword.ErrorReason);
            Assert.True(resetPassword.ErrorReason == "Your password must include a special character.", "resetPassword.ErrorReason == \"Your password must include a special character.\"");
        }

        //[Fact, Trait("", "Negative Case")]
        public void PostResetPasswordSameAsUsername()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload("P@ssw0rd", null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            resetPasswordPayload.NewPassword = Properties["enterpriseUsername6"];
            string newoldPassword = resetPasswordPayload.NewPassword;
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.False(resetPassword.IsSuccess, "resetPassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.True(resetPassword.IsError, "resetPassword.isError");
            Assert.NotNull(resetPassword.ErrorReason);
            Assert.True(resetPassword.ErrorReason.Contains("Your password cannot be the same as your Username."), "resetPassword.ErrorReason.Contains(\"Your password cannot be the same as your Username.\")");
        }

        //[Fact, Trait("", "Negative Case")]
        public void PostResetPasswordNoNumber()
        {
            // Set up Payload
            payload = reusable.DoPostResetPasswordPayload("P@ssw0rd", null);
            UserResetPassword resetPasswordPayload = JsonConvert.DeserializeObject<UserResetPassword>(payload);
            resetPasswordPayload.NewPassword = string.Concat(Guid.NewGuid().ToString("N").Select(c => (char)(c + 17))).Remove(15);
            string newoldPassword = resetPasswordPayload.NewPassword;
            payload = JsonConvert.SerializeObject(resetPasswordPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["ResetPassword"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            ResetPasswordResponse resetPassword = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(resetPassword.IsSuccess);
            Assert.False(resetPassword.IsSuccess, "resetPassword.isSuccess");
            Assert.NotNull(resetPassword.IsError);
            Assert.True(resetPassword.IsError, "resetPassword.isError");
            Assert.NotNull(resetPassword.ErrorReason);
            Assert.True(resetPassword.ErrorReason == "Your password must include a number.", "resetPassword.ErrorReason == \"Your password must include a number.\"");
        }
    }
    }
