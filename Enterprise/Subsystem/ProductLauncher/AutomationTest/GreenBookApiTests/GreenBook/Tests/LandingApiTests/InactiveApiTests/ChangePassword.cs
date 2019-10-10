using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Data;
using System;
using System.Linq;

namespace GreenBook.Tests
{
    public class ChangePassword : TestController
    {
        private string payload;
        JsonController jsonManager = new JsonController();
        TestUtilities reusable;
        private readonly ITestOutputHelper XunitTestOutPut;
        DatabaseController dbManager;
        string[] listOfPasswords = new string[6];

        public ChangePassword(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
        }

        // ChangePassword=/api/credential/ChangePassword
		
        //[Fact, Trait("", "Happy Path")]
        //public void PostChangePassword()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.NotNull(changePassword.EnterpriseUserName);
        //    Assert.True(changePassword.EnterpriseUserName == Properties["enterpriseUsername"], "changePassword.enterpriseUserName == Properties[\"enterpriseUsername\"]");
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.True(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.False(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //}

        //[Fact, Trait("", "Data-Driven")]
        //public void PostChangePasswordAverageValidLength()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1 && Properties["MaximumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null
        //        , int.Parse(Properties["MinimumLength"]) + ((int.Parse(Properties["MaximumLength"]) - int.Parse(Properties["MinimumLength"])) / 2));
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.NotNull(changePassword.EnterpriseUserName);
        //    Assert.True(changePassword.EnterpriseUserName == Properties["enterpriseUsername"], "changePassword.enterpriseUserName == Properties[\"enterpriseUsername\"]");
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.True(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.False(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //}

        //[Fact, Trait("", "Data-Driven")]
        //public void PostChangePasswordMaximumLength()
        //{
        //    // Set up Payload
        //    if (Properties["MaximumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MaximumLength"]));
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.NotNull(changePassword.EnterpriseUserName);
        //    Assert.True(changePassword.EnterpriseUserName == Properties["enterpriseUsername"], "changePassword.enterpriseUserName == Properties[\"enterpriseUsername\"]");
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.True(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.False(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //}

        //[Fact, Trait("", "Data-Driven")]
        //public void PostChangePasswordMinimumLengthPlusOne()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]) + 1);
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.NotNull(changePassword.EnterpriseUserName);
        //    Assert.True(changePassword.EnterpriseUserName == Properties["enterpriseUsername"], "changePassword.enterpriseUserName == Properties[\"enterpriseUsername\"]");
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.True(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.False(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //}

        //[Fact, Trait("", "Data-Driven")]
        //public void PostChangePasswordMaximumLengthMinusOne()
        //{
        //    // Set up Payload
        //    if (Properties["MaximumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MaximumLength"]) - 1);
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.NotNull(changePassword.EnterpriseUserName);
        //    Assert.True(changePassword.EnterpriseUserName == Properties["enterpriseUsername"], "changePassword.enterpriseUserName == Properties[\"enterpriseUsername\"]");
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.True(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.False(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNoPassword()
        //{
        //    // Set up Payloa
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, "", null);
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "New Password is not specified.", "changePassword.ErrorReason == \"New Password is not specified.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNullPassword()
        //{
        //    // Set up Payload
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, "null", null).Replace("\"null\"", "null");
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "New Password is not specified.", "changePassword.ErrorReason == \"New Password is not specified.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordUserEmailAddressAsPassword()
        //{
        //    // Set up Payload
        //    dbManager = new DatabaseController(Properties["dbConnection"]);
        //    DataTable userEmailAddress =
        //        dbManager.executeQuery(string.Format("select Email from [IdentityNewSchema].[Auth].[Users] where LoginId = '{0}'", Properties["enterpriseUsername"]));
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, userEmailAddress.Rows[0][0].ToString(), null);
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason.Contains("Your password cannot contain your Email Address."), "changePassword.ErrorReason.Contains(\"Your password cannot contain your Email Address.\")");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNoUsername()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"])).Replace(Properties["enterpriseUsername"], "");
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "No Username specified.", "changePassword.ErrorReason == \"No Username specified.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNullUsername()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"])).Replace("\"" + Properties["enterpriseUsername"] + "\"", "null");
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "No Username specified.", "changePassword.ErrorReason == \"No Username specified.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordInvalidUsername()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"])).Replace(Properties["enterpriseUsername"], "invalidUsername");
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "User name is incorrect or not found.", "changePassword.ErrorReason == \"User name is incorrect or not found.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNoActivityToken()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordPayload
        //        = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);
        //    changePasswordPayload.ActivityToken = "";
        //    payload = JsonConvert.SerializeObject(changePasswordPayload);

        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Forgot Password Activity Token is not specified.", "changePassword.ErrorReason == \"Forgot Password Activity Token is not specified.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNullActivityToken()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordPayload
        //        = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);
        //    changePasswordPayload.ActivityToken = "null";
        //    payload = JsonConvert.SerializeObject(changePasswordPayload).Replace("\"null\"", "null");

        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Forgot Password Activity Token is not specified.", "changePassword.ErrorReason == \"Forgot Password Activity Token is not specified.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordInvalidActivityToken()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordPayload
        //        = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);
        //    changePasswordPayload.ActivityToken = "invalidActivityToken";
        //    payload = JsonConvert.SerializeObject(changePasswordPayload);

        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Forgot Password Activity Token is expired.", "changePassword.ErrorReason == \"Forgot Password Activity Token is expired.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordExpiredActivityToken()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    reusable.DoResetMaximumActivityAttempts(Properties["enterpriseUsername"], HostUrl + Properties["GetSecurityQuestions"] + Properties["enterpriseUsername"], HttpVerb.Get);

        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Forgot Password Activity Token is expired.", "changePassword.ErrorReason == \"Forgot Password Activity Token is expired.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNoCorrectAnswerToken()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordPayload
        //        = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);
        //    changePasswordPayload.CorrectAnswerToken = "";
        //    payload = JsonConvert.SerializeObject(changePasswordPayload);

        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Correct Answer Token is not specified.", "changePassword.ErrorReason == \"Correct Answer Token is not specified.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNullCorrectAnswerToken()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordPayload
        //        = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);
        //    changePasswordPayload.CorrectAnswerToken = "null";
        //    payload = JsonConvert.SerializeObject(changePasswordPayload).Replace("\"null\"", "null");

        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Correct Answer Token is not specified.", "changePassword.ErrorReason == \"Correct Answer Token is not specified.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordInvalidCorrectAnswerToken()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordPayload
        //        = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);
        //    changePasswordPayload.CorrectAnswerToken = "invalidCorrectAnswerToken";
        //    payload = JsonConvert.SerializeObject(changePasswordPayload);

        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Correct Answer Token is expired.", "changePassword.ErrorReason == \"Correct Answer Token is expired.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordExpiredCorrectAnswerToken()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordPayload
        //        = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);

        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));
        //    RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordPayloadSecondAttempt
        //        = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);

        //    changePasswordPayload.ActivityToken = changePasswordPayloadSecondAttempt.ActivityToken;
        //    payload = JsonConvert.SerializeObject(changePasswordPayload);

        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Correct Answer Token is expired.", "changePassword.ErrorReason == \"Correct Answer Token is expired.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordFifthAmongTheFivePreviouslySavedPassword()
        //{
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1 && Properties["NumberOfPasswordsToRemember"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }

        //    RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword changePasswordPayload;
        //    ChangePasswordResponse changePassword = new ChangePasswordResponse();
        //    HttpStatusCode responseHttpStatusCode = new HttpStatusCode();

        //    if (Convert.ToBoolean(Properties["PreventPasswordReuse"]))
        //    {
        //        listOfPasswords = new string[Convert.ToInt32(Properties["NumberOfPasswordsToRemember"]) + 1];

        //        for (int countChangePassword = 0; countChangePassword < Convert.ToInt32(Properties["NumberOfPasswordsToRemember"]) + 1; countChangePassword++)
        //        {
        //            // Set up Payload for the List of Passwords to be tracked:
        //            payload = reusable.DoPostChangePasswordPayload(
        //                Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]));

        //            changePasswordPayload = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.ChangePassword>(payload);

        //            listOfPasswords[countChangePassword] = changePasswordPayload.NewPassword;

        //            XunitTestOutPut.WriteLine("Payload:\n" + payload);
        //            XunitTestOutPut.WriteLine("New Password for ChangePassword Attempt #" + (countChangePassword + 1) + ": " + changePasswordPayload.NewPassword);

        //            // Set up the API URL
        //            EndPointUrl = HostUrl + Properties["ChangePassword"];

        //            // Execute API
        //            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //            var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //            // Extract API's JSON Response
        //            var responseValue = GetHttpWebResponseValue(response);
        //            changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);
        //        }

        //        // Set up Payload
        //        payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, listOfPasswords[0], null, int.Parse(Properties["MinimumLength"]));
        //        XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //        // Set up the API URL
        //        EndPointUrl = HostUrl + Properties["ChangePassword"];

        //        // Execute API
        //        XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //        var responseForAssertion = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //        // Extract API's JSON Response
        //        var responseValueForAssertion = GetHttpWebResponseValue(responseForAssertion);
        //        changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValueForAssertion);
        //        XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + responseForAssertion.StatusCode + "\n\n" + responseValueForAssertion);
        //        responseHttpStatusCode = responseForAssertion.StatusCode;

        //        // Assert
        //        Assert.True(HttpStatusCode.OK == responseHttpStatusCode, "HttpStatusCode.OK == responseHttpStatusCode");
        //            Assert.Null(changePassword.EnterpriseUserName);
        //            Assert.NotNull(changePassword.IsSuccess);
        //            Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //            Assert.NotNull(changePassword.IsError);
        //            Assert.True(changePassword.IsError, "changePassword.isError");
        //            Assert.NotNull(changePassword.ErrorReason);
        //       Assert.True(changePassword.ErrorReason == "This password has already been used. Please try again.", "changePassword.ErrorReason == \"This password has already been used. Please try again.\"");

        //    }
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordMinimumLengthMinusOne()
        //{
        //    // Set up Payload
        //    if (Properties["MinimumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"]) -1);

        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Your password must be at least 8 characters.", "changePassword.ErrorReason == \"Your password must be at least 8 characters.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordMaximumLengthPlusOne()
        //{
        //    // Set up Payload
        //    if (Properties["MaximumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }
        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MaximumLength"]) + 1);
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Your password must be 20 characters or less.", "changePassword.ErrorReason == \"Your password must be 20 characters or less.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNoUpperCaseLetter()
        //{
        //    // Set up Payload
        //    if (Properties["MaximumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }

        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"])).ToLower();
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Your password must include an upper-case and a lower-case letter.", "changePassword.ErrorReason == \"Your password must include an upper-case and a lower-case letter.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNoLowerCaseLetter()
        //{
        //    // Set up Payload
        //    if (Properties["MaximumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }

        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, null, null, int.Parse(Properties["MinimumLength"])).ToUpper();
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Your password must include an upper-case and a lower-case letter.", "changePassword.ErrorReason == \"Your password must include an upper-case and a lower-case letter.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNoSpecialCharacter()
        //{
        //    // Set up Payload
        //    if (Properties["MaximumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }

        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, (string.Concat("Ab0", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(int.Parse(Properties["MinimumLength"]))), null, int.Parse(Properties["MinimumLength"]));
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Your password must include a number.", "changePassword.ErrorReason == \"Your password must include a number.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordSameAsUsername()
        //{
        //    // Set up Payload
        //    if (Properties["MaximumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }

        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, Properties["enterpriseUsername"], null, int.Parse(Properties["MinimumLength"]));
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Your password cannot be the same as your Username.", "changePassword.ErrorReason == \"Your password cannot be the same as your Username.\"");
        //}

        //[Fact, Trait("", "Negative Case")]
        //public void PostChangePasswordNoNumber()
        //{
        //    // Set up Payload
        //    if (Properties["MaximumLength"].Replace(" ", "").Length < 1)
        //    {
        //        Properties = reusable.DoSelectPasswordPolicy();
        //    }

        //    payload = reusable.DoPostChangePasswordPayload(Properties["enterpriseUsername"], null, (String.Concat(Guid.NewGuid().ToString("N").Select(c => (char)(c + 17))).Remove(int.Parse(Properties["MinimumLength"]))), null, int.Parse(Properties["MinimumLength"]));
        //    XunitTestOutPut.WriteLine("Payload:\n" + payload);

        //    // Set up the API URL
        //    EndPointUrl = HostUrl + Properties["ChangePassword"];

        //    // Execute API
        //    XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
        //    var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

        //    // Extract API's JSON Response
        //    var responseValue = GetHttpWebResponseValue(response);
        //    ChangePasswordResponse changePassword = JsonConvert.DeserializeObject<ChangePasswordResponse>(responseValue);
        //    XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + response.StatusCode + "\n\n" + responseValue);

        //    // Assert
        //    Assert.True(HttpStatusCode.OK == response.StatusCode, "HttpStatusCode.OK == response.StatusCode");
        //    Assert.Null(changePassword.EnterpriseUserName);
        //    Assert.NotNull(changePassword.IsSuccess);
        //    Assert.False(changePassword.IsSuccess, "changePassword.isSuccess");
        //    Assert.NotNull(changePassword.IsError);
        //    Assert.True(changePassword.IsError, "changePassword.isError");
        //    Assert.NotNull(changePassword.ErrorReason);
        //    Assert.True(changePassword.ErrorReason == "Your password must include a number.", "changePassword.ErrorReason == \"Your password must include a number.\"");
        //}
    }
}
