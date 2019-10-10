using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace GreenBook.Tests
{
    public class SetTemporaryPassword : TestController
	{
	    private string payload;
	    JsonController jsonManager = new JsonController();
	    TestUtilities reusable;
	    private readonly ITestOutputHelper XunitTestOutPut;
        string userTypeId, userName;

        public SetTemporaryPassword(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

        // SetTemporaryPassword=/api/credential/SetTemporaryPassword

        //[Theory]
        //[Trait("Data-Driven", "Happy Path")]
        //[InlineData("PostSetPassword", "404", "")]
        //[InlineData("PostSetPassword", "401", "")]
        //[InlineData("PostSetPassword", "402", "")]
       // [Fact, Trait("", "Happy Path")]
        public void PostSetTemporaryPasswordforNewUser()
	    {
	        string loginResponse;

            // Authorize User to get his details
            //GetClientToken(Properties["identityClientUrl"], "pradeep.kolanu@realpage.com", "Real@123");
            userTypeId = "404";
            string userType = "Regular User (No Email)";

            var newLoginName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
            //newLoginName = newLoginName.Replace("@ApiTest.com", "2018");
            
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

            long personaId = createUserResponse.PersonaId;
	        Guid realpageId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona(personaId)).RealPageId;

            // Validate New User with the password set while creation
	        //loginResponse = GetClientToken(Properties["identityClientUrl"], newLoginName, "P@ssw0rd");
	        //Assert.DoesNotContain("error", loginResponse);
            
            // Setting Temporary Password
            UserResetPassword resetPassword = new UserResetPassword();
	        resetPassword.OldPassword = "P@ssw0rd";
	        resetPassword.NewPassword = "Real@123";
            resetPassword.RealPageId = realpageId;
	        
            payload = JsonConvert.SerializeObject(resetPassword);
	        EndPointUrl = HostUrl + Properties["SetTemporaryPassword"];
	        GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

	        ResetPasswordResponse resetPasswordResponse = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            Assert.True(resetPasswordResponse.IsSuccess);
            Assert.False(resetPasswordResponse.IsError);
            Assert.Null(resetPasswordResponse.ErrorReason);

            // Validate New user with password set during Reset
            loginResponse = GetClientToken(Properties["identityClientUrl"], newLoginName, resetPassword.NewPassword);
            Assert.DoesNotContain("error",loginResponse);
        }

	    //[Theory]
	    //[Trait("Data-Driven", "Happy Path")]
	    //[InlineData("PostSetPassword-MinLength", "404", "", "Pc1@", "Fail")]
	    //[InlineData("PostSetPassword-MaxLength", "404", "", "abcdefghijklmnopqrstK1@", "Fail")]
	    //[InlineData("PostSetPassword-Number", "404", "", "P@ssworddd", "Fail")]
	    //[InlineData("PostSetPassword-SplChar", "404", "", "Password123", "Fail")]
	    //[InlineData("PostSetPassword-Positive", "404", "", "P@ssw0rd", "Pass")]
	    public void PostSetTemporaryPasswordValidations(string testCase, string userTypeId, string userName, string pwd, string expResponse)
	    {
	        string loginResponse;

	        // Authorize User to get his details
	        GetClientToken(Properties["identityClientUrl"], "pradeep.kolanu@realpage.com", "Real@123");

	        ProfileDetail profileDetailRequest = createUserPayload(userTypeId, userName);
	        payload = JsonConvert.SerializeObject(profileDetailRequest);

	        EndPointUrl = HostUrl + Properties["User"];
	        GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
	        CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(ResponseString);

	        long personaId = createUserResponse.PersonaId;
	        Guid realpageId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona(personaId)).RealPageId;

	        // Validate New User with the password set while creation
	        loginResponse = GetClientToken(Properties["identityClientUrl"], profileDetailRequest.userLogin.LoginName, profileDetailRequest.userLogin.Password);
	        Assert.DoesNotContain("error", loginResponse);

	        // Setting Temporary Password
	        UserResetPassword resetPassword = new UserResetPassword();
	        resetPassword.OldPassword = "";
	        resetPassword.NewPassword = pwd;
	        resetPassword.RealPageId = realpageId;

	        payload = JsonConvert.SerializeObject(resetPassword);
	        EndPointUrl = HostUrl + Properties["SetTemporaryPassword"];
	        GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

	        ResetPasswordResponse resetPasswordResponse = JsonConvert.DeserializeObject<ResetPasswordResponse>(ResponseString);
            if(expResponse == "Fail")
            { 
	        Assert.False(resetPasswordResponse.IsSuccess);
	        Assert.True(resetPasswordResponse.IsError);
	        Assert.NotNull(resetPasswordResponse.ErrorReason);
            }
            else
	        {
	        Assert.True(resetPasswordResponse.IsSuccess);
	        Assert.False(resetPasswordResponse.IsError);
	        Assert.Null(resetPasswordResponse.ErrorReason);
	        }
        }


        public ProfileDetail createUserPayload(string userTypeId, string loginName)
	    {
	    string dt = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssTZD");
	    dt = dt.Replace("-", "").Replace(":","");

        if (loginName == "")
	    loginName = "K" + dt;

	    ProfileDetail profileDetailRequest = new ProfileDetail();
	    profileDetailRequest.userLogin = new UserLogin();
	    profileDetailRequest.organization = new List<Organization>();
        profileDetailRequest.productBatch = new List<ProductBatch>();
        //profileDetailRequest.TelecommunicationNumber = new List<TelecommunicationNumber>();

            profileDetailRequest.FirstName = "AutoFirst";
	    profileDetailRequest.LastName = "AutoLast";
	    profileDetailRequest.UserTypeId = Convert.ToInt32(userTypeId);
	    profileDetailRequest.userLogin.FromDate = DateTime.Now;
	    profileDetailRequest.userLogin.IsActive = true;
            
	    if (userTypeId == "404")
	    {
	        profileDetailRequest.userLogin.LoginName = loginName;
	        profileDetailRequest.Password = "Real@123";
	    }
	    else
	    {
	        profileDetailRequest.userLogin.LoginName = loginName;
	    }

            return profileDetailRequest;
	    }



	}
}
