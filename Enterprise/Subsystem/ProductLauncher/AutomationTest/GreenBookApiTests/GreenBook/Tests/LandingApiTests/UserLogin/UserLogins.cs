using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Collections.Generic;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Linq;

namespace GreenBook.Tests
{
    public class UserLogins : TestController
    {
		public UserLogins(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
           // userLoginsUser = Properties[];

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

            
            //            EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(userLoginsUser)
            //                + "&newUserRegistrationToken=" + reusable.DoPostNewUserToken(payload, roleType.list.First().PartyRoleTypeId);
              //          XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl + "to get the ValidateUser ActivityToken.");

                //        GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                  //      newUserToken = JsonConvert.DeserializeObject<ValidateUserResponse>(ResponseString).ValidateUserToken;
            

            payload = reusable.DoPostSetPasswordPayload(userLoginsUser, verificationToken);
			
			EndPointUrl = HostUrl + Properties["SetPassword"];
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            ChangePasswordResponse response = JsonConvert.DeserializeObject<ChangePasswordResponse>(ResponseString);
*/   
            }

        private string payload, userLoginsUser;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string newUserToken, existingLoginName;
		private ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData> roleType;
        private Guid realpageId;
        private long personaId;
        
        // UserLogins=/api/UserLogins

        [Fact, Trait("", "Happy Path")]
		public void GetUserLogins()
		{
            // Set up the API URL
            personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId;
            realpageId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona(personaId)).RealPageId;
            existingLoginName = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLogins(realpageId)).LoginName;
            EndPointUrl = HostUrl + Properties["UserLogins"] + realpageId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			UserLogin userLoginRes = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            Assert.True(userLoginRes.PartyId > 0);
            Assert.True(userLoginRes.RealPageId != null);
            Assert.True(userLoginRes.IsActive != null);
            Assert.True(userLoginRes.IsLocked != null);
            Assert.True(userLoginRes.IsPending != null);
            Assert.True(userLoginRes.IsExpired != null);
            Assert.True(userLoginRes.IsForceReSetPassword != null);
            Assert.True(userLoginRes.PasswordModifiedDate != null);
            Assert.True(userLoginRes.FromDate != null);
            Assert.True(userLoginRes.StatusSetDate != null);
            Assert.True(userLoginRes.Status != null);
            Assert.True(userLoginRes.Is3rdPartyIDP != null);
            Assert.True(userLoginRes.UserId > 0);
            Assert.True(userLoginRes.LoginName != null);
            Assert.True(userLoginRes.TimeZoneOffset != null);

            // Additional Asserts
            Assert.True(userLoginRes.RealPageId == realpageId);
            Assert.True(userLoginRes.LoginName == existingLoginName);
            Assert.True(userLoginRes.IsActive == true);
            Assert.True(userLoginRes.IsLocked == false);
            Assert.True(userLoginRes.IsPending == false);
            Assert.True(userLoginRes.IsExpired == false);
            Assert.True(userLoginRes.Is3rdPartyIDP == false);

            /*
                        // Get Expected Response from DB
                        //UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(payload);

                        // Assert
                        Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
                        Assert.NotNull(userLoginResponse.UserId);
                        Assert.True(userLoginResponse.UserId == expectedUserLoginUser.UserId, "userLoginResponse.UserId == expectedUserLoginUser.UserId");
                        Assert.NotNull(userLoginResponse.PartyId);
                        Assert.True(userLoginResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginResponse.PartyId == expectedUserLoginUser.PartyId");
                        Assert.NotNull(userLoginResponse.RealPageId);
                        Assert.True(userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId");
                        Assert.NotNull(userLoginResponse.LoginName);
                        Assert.True(userLoginResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginResponse.LoginName == expectedUserLoginUser.LoginName");

                        if (userLoginResponse.LoginNameType != null)
                        {
                            Assert.NotNull(userLoginResponse.LoginNameType);
                            Assert.True(userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
                        }

                        Assert.NotNull(userLoginResponse.IsActive);
                        Assert.True(userLoginResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginResponse.IsActive == expectedUserLoginUser.IsActive");
                        Assert.NotNull(userLoginResponse.IsLocked);
                        Assert.True(userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked");
                        Assert.True(userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted");

                        if (userLoginResponse.PasswordModifiedDate != null)
                        {
                            Assert.NotNull(userLoginResponse.PasswordModifiedDate);
                            Assert.True(userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
                        }
                        if (userLoginResponse.FromDate != null)
                        {
                            Assert.NotNull(userLoginResponse.FromDate);
                            Assert.True(userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
                        }
                        if (userLoginResponse.ThruDate != null)
                        {
                            Assert.NotNull(userLoginResponse.ThruDate);
                            Assert.True(userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
                        }

                        Assert.NotNull(userLoginResponse.StatusSetDate);
                        Assert.True(userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
                        Assert.NotNull(userLoginResponse.LastLogin);
                        Assert.True(userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");

                        Assert.NotNull(userLoginResponse.IsSuperUser);
                        Assert.True(userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");

               //         if (userLoginResponse.Status != null)
                        //{
                        //	Assert.NotNull(userLoginResponse.Status);
                        //	Assert.True(userLoginResponse.Status == expectedUserLoginUser.Status, "userLoginResponse.Status == expectedUserLoginUser.Status");
                        //}
            */
        }

        //[Fact, Trait("", "Negative Case")]
		public void GetUserLoginsInvalidRealPageId()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["UserLogins"] + "InvalidRealPageId";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
		}

        [Fact, Trait("", "Happy Path")]
        public void PostUserLogins()
        {
            personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId;
            realpageId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona(personaId)).RealPageId;
            existingLoginName = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLogins(realpageId)).LoginName;

            // Set up Payloadue, false, false, "2
            payload = reusable.DoPostPutUserLogins(0, 0, "00000000-0000-00000000-000000000000", "1", true, false, false, "2018-04-02T08:15:30.738Z", "2018-04-02T08:15:30.738Z", "2018-04-02T08:15:30.738Z", true, "string", "null", "2018-04-02T08:15:30.738Z", "2018-04-02T08:15:30.738Z", HttpVerb.Post);

            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            //Set up the API URL
            //EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
            EndPointUrl = HostUrl + Properties["UserLogins"] + realpageId;

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            
            UserLogin.UserLoginOutputResult userloginResponse = JsonConvert.DeserializeObject<UserLogin.UserLoginOutputResult>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(userloginResponse.NewUserId);
        }


        //[Fact, Trait("", "Happy Path")]
        public void PutUserLogins()
        {
            // Set up Payload
            payload = reusable.DoPostPutUserLogins(51, 71, "7CA43C1D-4591-45EC-AA72-454E94493920", "1", true, false, false, "2018-04-03T08:15:30.738Z", 
                                                    "2018-04-03T08:15:30.738Z", "2017-04-03T08:15:30.738Z", true, "string", "dan12@test.com", 
                                                    "2018-04-03T08:15:30.738Z", "2017-04-03T08:15:30.738Z", HttpVerb.Put);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            //Set up the API URL
            EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

            // Extract API's JSON Response
            
            UserLogin userLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Get Expected Response from DB
            UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(payload);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(userLoginResponse.UserId);
            Assert.True(userLoginResponse.UserId == expectedUserLoginUser.UserId, "userLoginResponse.UserId == expectedUserLoginUser.UserId");
            Assert.NotNull(userLoginResponse.PartyId);
            Assert.True(userLoginResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginResponse.PartyId == expectedUserLoginUser.PartyId");
            Assert.NotNull(userLoginResponse.RealPageId);
            Assert.True(userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId");
            Assert.NotNull(userLoginResponse.LoginName);
            Assert.True(userLoginResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginResponse.LoginName == expectedUserLoginUser.LoginName");

            if (userLoginResponse.LoginNameType != null)
            {
                Assert.NotNull(userLoginResponse.LoginNameType);
                Assert.True(userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
            }

            Assert.NotNull(userLoginResponse.IsActive);
            Assert.True(userLoginResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginResponse.IsActive == expectedUserLoginUser.IsActive");
            Assert.NotNull(userLoginResponse.IsLocked);
            Assert.True(userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked");
            Assert.NotNull(userLoginResponse.IsTainted);
            Assert.True(userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted");

            if (userLoginResponse.PasswordModifiedDate != null)
            {
                Assert.NotNull(userLoginResponse.PasswordModifiedDate);
                Assert.True(userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
            }
            if (userLoginResponse.FromDate != null)
            {
                Assert.NotNull(userLoginResponse.FromDate);
                Assert.True(userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
            }
            if (userLoginResponse.ThruDate != null)
            {
                Assert.NotNull(userLoginResponse.ThruDate);
                Assert.True(userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
            }

            Assert.NotNull(userLoginResponse.StatusSetDate);
            Assert.True(userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
            Assert.NotNull(userLoginResponse.LastLogin);
            Assert.True(userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");
			
            Assert.True(userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");

            //if (userLoginResponse.Status != null)
            //{
            //    Assert.NotNull(userLoginResponse.Status);
            //    Assert.True(userLoginResponse.Status == expectedUserLoginUser.Status, "userLoginResponse.Status == expectedUserLoginUser.Status");
            //}
        }

        //[Fact, Trait("", "Negative Case")]
        public void PostUserLoginsInvalidRealPageId()
        {
            // Set up the API URL
            EndPointUrl = HostUrl + Properties["UserLogins"] + "InvalidRealPageId";

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

            // Extract API's JSON Response
            
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
            Assert.NotNull(ResponseString);
            Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
        }

        //[Fact, Trait("", "Negative Case")]
        public void PutUserLoginsInvalidRealPageId()
        {
            // Set up the API URL
            EndPointUrl = HostUrl + Properties["UserLogins"] + "InvalidRealPageId";

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: "");

            // Extract API's JSON Response
            
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
            Assert.NotNull(ResponseString);
            Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
        }

		//[Fact, Trait("", "Data-Driven")]
		//public void PutUserLoginsLockUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	// Execute API for Payload
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponse.IsLocked = true; //Locking the User

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponse);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin userLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Get Expected Response from DB
		//	UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(payload);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(userLoginResponse.UserId);
		//	Assert.True(userLoginResponse.UserId == expectedUserLoginUser.UserId, "userLoginResponse.UserId == expectedUserLoginUser.UserId");
		//	Assert.NotNull(userLoginResponse.PartyId);
		//	Assert.True(userLoginResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginResponse.PartyId == expectedUserLoginUser.PartyId");
		//	Assert.NotNull(userLoginResponse.RealPageId);
		//	Assert.True(userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId");
		//	Assert.NotNull(userLoginResponse.LoginName);
		//	Assert.True(userLoginResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginResponse.LoginName == expectedUserLoginUser.LoginName");

		//	if (userLoginResponse.LoginNameType != null)
		//	{
		//		Assert.NotNull(userLoginResponse.LoginNameType);
		//		Assert.True(userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
		//	}

		//	Assert.NotNull(userLoginResponse.IsActive);
		//	Assert.True(userLoginResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginResponse.IsActive == expectedUserLoginUser.IsActive");
		//	Assert.NotNull(userLoginResponse.IsLocked);
		//	Assert.True(userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked");
		//	Assert.NotNull(userLoginResponse.IsTainted);
		//	Assert.True(userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted");

		//	if (userLoginResponse.PasswordModifiedDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.PasswordModifiedDate);
		//		Assert.True(userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
		//	}
		//	if (userLoginResponse.FromDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.FromDate);
		//		Assert.True(userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
		//	}
		//	if (userLoginResponse.ThruDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.ThruDate);
		//		Assert.True(userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
		//	}

		//	Assert.NotNull(userLoginResponse.StatusSetDate);
		//	Assert.True(userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
		//	Assert.NotNull(userLoginResponse.LastLogin);
		//	Assert.True(userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");
			
		//	Assert.True(userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");

		//	//if (userLoginResponse.Status != null)
		//	//{
		//	//	Assert.NotNull(userLoginResponse.Status);
		//	//	Assert.True(userLoginResponse.Status == expectedUserLoginUser.Status, "userLoginResponse.Status == expectedUserLoginUser.Status");
		//	//}

		//	string verifyLockedUser = GetClientToken(Properties["identityClientUrl"], userLoginsUser, "P@ssw0rd");
		//	Assert.NotNull(verifyLockedUser);
		//	Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//	XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PutUserLoginsUnlockUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	// Execute API for Payload
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponse.IsLocked = false; //Unlocking the User

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponse);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin userLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Get Expected Response from DB
		//	UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(payload);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(userLoginResponse.UserId);
		//	Assert.True(userLoginResponse.UserId == expectedUserLoginUser.UserId, "userLoginResponse.UserId == expectedUserLoginUser.UserId");
		//	Assert.NotNull(userLoginResponse.PartyId);
		//	Assert.True(userLoginResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginResponse.PartyId == expectedUserLoginUser.PartyId");
		//	Assert.NotNull(userLoginResponse.RealPageId);
		//	Assert.True(userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId");
		//	Assert.NotNull(userLoginResponse.LoginName);
		//	Assert.True(userLoginResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginResponse.LoginName == expectedUserLoginUser.LoginName");

		//	if (userLoginResponse.LoginNameType != null)
		//	{
		//		Assert.NotNull(userLoginResponse.LoginNameType);
		//		Assert.True(userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
		//	}

		//	Assert.NotNull(userLoginResponse.IsActive);
		//	Assert.True(userLoginResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginResponse.IsActive == expectedUserLoginUser.IsActive");
		//	Assert.NotNull(userLoginResponse.IsLocked);
		//	Assert.True(userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked");
		//	Assert.NotNull(userLoginResponse.IsTainted);
		//	Assert.True(userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted");

		//	if (userLoginResponse.PasswordModifiedDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.PasswordModifiedDate);
		//		Assert.True(userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
		//	}
		//	if (userLoginResponse.FromDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.FromDate);
		//		Assert.True(userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
		//	}
		//	if (userLoginResponse.ThruDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.ThruDate);
		//		Assert.True(userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
		//	}

		//	Assert.NotNull(userLoginResponse.StatusSetDate);
		//	Assert.True(userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
		//	Assert.NotNull(userLoginResponse.LastLogin);
		//	Assert.True(userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");

		//	Assert.NotNull(userLoginResponse.IsSuperUser);
		//	Assert.True(userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");

		//	//if (userLoginResponse.Status != null)
		//	//{
		//	//	Assert.NotNull(userLoginResponse.Status);
		//	//	Assert.True(userLoginResponse.Status == expectedUserLoginUser.Status, "userLoginResponse.Status == expectedUserLoginUser.Status");
		//	//}

		//	string verifyUnlockedUser = GetClientToken(Properties["identityClientUrl"], userLoginsUser, "P@ssw0rd");
		//	Assert.NotNull(verifyUnlockedUser);
		//	Assert.True(!verifyUnlockedUser.Contains("User account is locked."), "!verifyUnlockedUser.Contains(\"User account is locked.\")");
		//	XunitTestOutPut.WriteLine("\n\nLog-in Attempt Token:\n" + verifyUnlockedUser);
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsLockUserBatch()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Set up the GET Organization API URL
		//	EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId.ToString());

		//	// Execute GET Organization API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract GET Organization API's JSON Response
		//	
		//	List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
		//		= JsonConvert.DeserializeObject<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?organizationRealPageId="
		//		+ organizationsResponse[0].RealPageId + "&userLoginStatusType=Locked&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");
				
		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail")? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsLockUserAllRecords()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Set up the GET Organization API URL
		//	EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId.ToString());

		//	// Execute GET Organization API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract GET Organization API's JSON Response
		//	
		//	List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
		//		= JsonConvert.DeserializeObject<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?organizationRealPageId="
		//		+ organizationsResponse[0].RealPageId + "&userLoginStatusType=Locked&updateType=AllRecords");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsUnlockUserBatch()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Set up the GET Organization API URL
		//	EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId.ToString());

		//	// Execute GET Organization API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract GET Organization API's JSON Response
		//	
		//	List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
		//		= JsonConvert.DeserializeObject<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?organizationRealPageId="
		//		+ organizationsResponse[0].RealPageId + "&userLoginStatusType=Unlocked&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsUnlockUserAllRecords()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Set up the GET Organization API URL
		//	EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId.ToString());

		//	// Execute GET Organization API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract GET Organization API's JSON Response
		//	
		//	List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
		//		= JsonConvert.DeserializeObject<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?organizationRealPageId="
		//		+ organizationsResponse[0].RealPageId + "&userLoginStatusType=Unlocked&updateType=AllRecords");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsLockUserWithoutUpdateType()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Set up the GET Organization API URL
		//	EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId.ToString());

		//	// Execute GET Organization API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract GET Organization API's JSON Response
		//	
		//	List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
		//		= JsonConvert.DeserializeObject<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?organizationRealPageId="
		//		+ organizationsResponse[0].RealPageId + "&userLoginStatusType=Locked&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsUnlockUserWithoutUpdateType()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Set up the GET Organization API URL
		//	EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId.ToString());

		//	// Execute GET Organization API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract GET Organization API's JSON Response
		//	
		//	List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
		//		= JsonConvert.DeserializeObject<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?organizationRealPageId="
		//		+ organizationsResponse[0].RealPageId + "&userLoginStatusType=Unlocked&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		//[Fact, Trait("", "Negative Case")]
		//public void PatchUserLoginsLockUnlockUserWithoutUserLoginStatusType()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Set up the GET Organization API URL
		//	EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId.ToString());

		//	// Execute GET Organization API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract GET Organization API's JSON Response
		//	
		//	List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
		//		= JsonConvert.DeserializeObject<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?organizationRealPageId="
		//		+ organizationsResponse[0].RealPageId + "&userLoginStatusType=&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(ResponseString.Contains("Null parameter: updateUserLoginData")
		//		, "ResponseString.Contains(\"Null parameter: updateUserLoginData\")");
		//}

		////[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsUnlockUserWithoutOrganizationRealPageId()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=Unlocked&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		////[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsLockUserWithoutOrganizationRealPageId()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=Locked&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		////[Fact, Trait("", "Negative Case")]
		//public void PatchUserLoginsLockUnlockUserWithOrganizationRealPageIdOnly()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Set up the GET Organization API URL
		//	EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}", JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId.ToString());

		//	// Execute GET Organization API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract GET Organization API's JSON Response
		//	
		//	List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization> organizationsResponse
		//		= JsonConvert.DeserializeObject<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Organization>>(ResponseString);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?organizationRealPageId="
		//		+ organizationsResponse[0].RealPageId + "&userLoginStatusType=&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(ResponseString.Contains("Null parameter: updateUserLoginData")
		//		, "ResponseString.Contains(\"Null parameter: updateUserLoginData\")");
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsUnlockUserWithUserLoginStatusTypeOnly()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=Unlocked&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsLockUserWithUserLoginStatusTypeOnly()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=Locked&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		//[Fact, Trait("", "Negative Case")]
		//public void PatchUserLoginsLockUnlockUserWithUpdateTypeOnly()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(ResponseString.Contains("Null parameter: updateUserLoginData")
		//		, "ResponseString.Contains(\"Null parameter: updateUserLoginData\")");
		//}

		//[Fact, Trait("", "Negative Case")]
		//public void PatchUserLoginsLockUnlockUserWithoutParameterValues()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(ResponseString.Contains("Null parameter: updateUserLoginData")
		//		, "ResponseString.Contains(\"Null parameter: updateUserLoginData\")");
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PutUserLoginsActivateUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	// Execute API for Payload
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponse.IsActive = true; //Activating the User
		//	expectedUserLoginResponse.IsLocked = false; //Unlocking the User

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponse);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin userLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Get Expected Response from DB
		//	UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(payload);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(userLoginResponse.UserId);
		//	Assert.True(userLoginResponse.UserId == expectedUserLoginUser.UserId, "userLoginResponse.UserId == expectedUserLoginUser.UserId");
		//	Assert.NotNull(userLoginResponse.PartyId);
		//	Assert.True(userLoginResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginResponse.PartyId == expectedUserLoginUser.PartyId");
		//	Assert.NotNull(userLoginResponse.RealPageId);
		//	Assert.True(userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId");
		//	Assert.NotNull(userLoginResponse.LoginName);
		//	Assert.True(userLoginResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginResponse.LoginName == expectedUserLoginUser.LoginName");

		//	if (userLoginResponse.LoginNameType != null)
		//	{
		//		Assert.NotNull(userLoginResponse.LoginNameType);
		//		Assert.True(userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
		//	}

		//	Assert.NotNull(userLoginResponse.IsActive);
		//	Assert.True(userLoginResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginResponse.IsActive == expectedUserLoginUser.IsActive");
		//	Assert.NotNull(userLoginResponse.IsLocked);
		//	Assert.True(userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked");
		//	Assert.NotNull(userLoginResponse.IsTainted);
		//	Assert.True(userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted");

		//	if (userLoginResponse.PasswordModifiedDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.PasswordModifiedDate);
		//		Assert.True(userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
		//	}
		//	if (userLoginResponse.FromDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.FromDate);
		//		Assert.True(userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
		//	}
		//	if (userLoginResponse.ThruDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.ThruDate);
		//		Assert.True(userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
		//	}

		//	Assert.NotNull(userLoginResponse.StatusSetDate);
		//	Assert.True(userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
		//	Assert.NotNull(userLoginResponse.LastLogin);
		//	Assert.True(userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");

		//	Assert.True(userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");

		//	//if (userLoginResponse.Status != null)
		//	//{
		//	//	Assert.NotNull(userLoginResponse.Status);
		//	//	Assert.True(userLoginResponse.Status == expectedUserLoginUser.Status, "userLoginResponse.Status == expectedUserLoginUser.Status");
		//	//}

		//	string verifyLockedUser = GetClientToken(Properties["identityClientUrl"], userLoginsUser, "P@ssw0rd");
		//	Assert.NotNull(verifyLockedUser);
		//	Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//	XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PutUserLoginsDisableUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	// Execute API for Payload
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponse.IsActive = false; //Disabling the User
		//	expectedUserLoginResponse.IsLocked = false; //Unlocking the User

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponse);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	UserLogin userLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Get Expected Response from DB
		//	UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(payload);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(userLoginResponse.UserId);
		//	Assert.True(userLoginResponse.UserId == expectedUserLoginUser.UserId, "userLoginResponse.UserId == expectedUserLoginUser.UserId");
		//	Assert.NotNull(userLoginResponse.PartyId);
		//	Assert.True(userLoginResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginResponse.PartyId == expectedUserLoginUser.PartyId");
		//	Assert.NotNull(userLoginResponse.RealPageId);
		//	Assert.True(userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginResponse.RealPageId == expectedUserLoginUser.RealPageId");
		//	Assert.NotNull(userLoginResponse.LoginName);
		//	Assert.True(userLoginResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginResponse.LoginName == expectedUserLoginUser.LoginName");

		//	if (userLoginResponse.LoginNameType != null)
		//	{
		//		Assert.NotNull(userLoginResponse.LoginNameType);
		//		Assert.True(userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
		//	}

		//	Assert.NotNull(userLoginResponse.IsActive);
		//	Assert.True(userLoginResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginResponse.IsActive == expectedUserLoginUser.IsActive");
		//	Assert.NotNull(userLoginResponse.IsLocked);
		//	Assert.True(userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginResponse.IsLocked == expectedUserLoginUser.IsLocked");
		//	Assert.NotNull(userLoginResponse.IsTainted);
		//	Assert.True(userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginResponse.IsTainted == expectedUserLoginUser.IsTainted");

		//	if (userLoginResponse.PasswordModifiedDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.PasswordModifiedDate);
		//		Assert.True(userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
		//	}
		//	if (userLoginResponse.FromDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.FromDate);
		//		Assert.True(userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
		//	}
		//	if (userLoginResponse.ThruDate != null)
		//	{
		//		Assert.NotNull(userLoginResponse.ThruDate);
		//		Assert.True(userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
		//	}

		//	Assert.NotNull(userLoginResponse.StatusSetDate);
		//	Assert.True(userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
		//	Assert.NotNull(userLoginResponse.LastLogin);
		//	Assert.True(userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");

		//	Assert.NotNull(userLoginResponse.IsSuperUser);
		//	Assert.True(userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");

		//	//if (userLoginResponse.Status != null)
		//	//{
		//	//	Assert.NotNull(userLoginResponse.Status);
		//	//	Assert.True(userLoginResponse.Status == expectedUserLoginUser.Status, "userLoginResponse.Status == expectedUserLoginUser.Status");
		//	//}

		//	string verifyUnlockedUser = GetClientToken(Properties["identityClientUrl"], userLoginsUser, "P@ssw0rd");
		//	Assert.NotNull(verifyUnlockedUser);
		//	Assert.True(!verifyUnlockedUser.Contains("User account is locked."), "!verifyUnlockedUser.Contains(\"User account is locked.\")");
		//	XunitTestOutPut.WriteLine("\n\nLog-in Attempt Token:\n" + verifyUnlockedUser);
		//}

		//[Fact, Trait("", "Happy Path")]
		//public void PatchUserLoginsActivateUserBatch()
		//{
		//	// Set up Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	userLoginsUser = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
		//	payload = reusable.DoPostEmailNotificationPayload(userLoginsUser);

		//	EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(userLoginsUser)
		//		+ "&newUserRegistrationToken=" + reusable.DoPostNewUserToken(payload, roleType.list.First().PartyRoleTypeId);
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl + "to get the ValidateUser ActivityToken.");
		//	newUserToken = JsonConvert.DeserializeObject<ValidateUserResponse>(GetHttpWebResponseString(
		//		GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: ""))).ValidateUserToken;

		//	payload = reusable.DoPostSetPasswordPayload(userLoginsUser, null, newUserToken);

		//	EndPointUrl = HostUrl + Properties["SetPassword"];
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?userLoginStatusType=Active&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	ObjectListOutput<UserLogin, IErrorData> userLoginResponseList = JsonConvert.DeserializeObject<
		//		ObjectListOutput<UserLogin, IErrorData>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.list.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].UserId);
		//		Assert.True(userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].PartyId);
		//		Assert.True(userLoginResponseList.list[countUserLogins].PartyId == expectedUserLoginResponseList[countUserLogins].PartyId
		//			, "userLoginResponseList.list[countUserLogins].PartyId == expectedUserLoginResponseList[countExpectedUserLogins].PartyId");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList.list[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponseList.list[countUserLogins].RealPageId == expectedUserLoginResponseList[countExpectedUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].LoginName);
		//		Assert.True(userLoginResponseList.list[countUserLogins].LoginName == expectedUserLoginResponseList[countUserLogins].LoginName
		//			, "userLoginResponseList.list[countUserLogins].LoginName == expectedUserLoginResponseList[countExpectedUserLogins].LoginName");
		//		Assert.True(userLoginResponseList.list[countUserLogins].LoginNameType == expectedUserLoginResponseList[countUserLogins].LoginNameType
		//			, "userLoginResponseList.list[countUserLogins].LoginNameType == expectedUserLoginResponseList[countExpectedUserLogins].LoginNameType");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].IsActive);
		//		Assert.True(userLoginResponseList.list[countUserLogins].IsActive == expectedUserLoginResponseList[countUserLogins].IsActive
		//			, "userLoginResponseList.list[countUserLogins].IsActive == expectedUserLoginResponseList[countExpectedUserLogins].IsActive");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].IsLocked);
		//		Assert.True(userLoginResponseList.list[countUserLogins].IsLocked == expectedUserLoginResponseList[countUserLogins].IsLocked
		//			, "userLoginResponseList.list[countUserLogins].IsLocked == expectedUserLoginResponseList[countExpectedUserLogins].IsLocked");
		//		Assert.True(userLoginResponseList.list[countUserLogins].IsTainted == expectedUserLoginResponseList[countUserLogins].IsTainted
		//			, "userLoginResponseList.list[countUserLogins].IsTainted == expectedUserLoginResponseList[countExpectedUserLogins].IsTainted");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].IsPending);
		//		Assert.True(userLoginResponseList.list[countUserLogins].IsPending == expectedUserLoginResponseList[countUserLogins].IsPending
		//			, "userLoginResponseList.list[countUserLogins].IsPending == expectedUserLoginResponseList[countExpectedUserLogins].IsPending");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].IsExpired);
		//		Assert.True(userLoginResponseList.list[countUserLogins].IsExpired == expectedUserLoginResponseList[countUserLogins].IsExpired
		//			, "userLoginResponseList.list[countUserLogins].IsExpired == expectedUserLoginResponseList[countExpectedUserLogins].IsExpired");
		//		Assert.True(userLoginResponseList.list[countUserLogins].PasswordModifiedDate == expectedUserLoginResponseList[countUserLogins].PasswordModifiedDate
		//			, "userLoginResponseList.list[countUserLogins].PasswordModifiedDate == expectedUserLoginResponseList[countExpectedUserLogins].PasswordModifiedDate");
		//		Assert.True(userLoginResponseList.list[countUserLogins].FromDate == expectedUserLoginResponseList[countUserLogins].FromDate
		//			, "userLoginResponseList.list[countUserLogins].FromDate == expectedUserLoginResponseList[countExpectedUserLogins].FromDate");
		//		Assert.True(userLoginResponseList.list[countUserLogins].ThruDate == expectedUserLoginResponseList[countUserLogins].ThruDate
		//			, "userLoginResponseList.list[countUserLogins].ThruDate == expectedUserLoginResponseList[countExpectedUserLogins].ThruDate");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].StatusSetDate);
		//		Assert.True(userLoginResponseList.list[countUserLogins].StatusSetDate == expectedUserLoginResponseList[countUserLogins].StatusSetDate
		//			, "userLoginResponseList.list[countUserLogins].StatusSetDate == expectedUserLoginResponseList[countExpectedUserLogins].StatusSetDate");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].LastLogin);
		//		Assert.True(userLoginResponseList.list[countUserLogins].LastLogin == expectedUserLoginResponseList[countUserLogins].LastLogin
		//			, "userLoginResponseList.list[countUserLogins].LastLogin == expectedUserLoginResponseList[countExpectedUserLogins].LastLogin");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].IsSuperUser);
		//		Assert.True(userLoginResponseList.list[countUserLogins].IsSuperUser == expectedUserLoginResponseList[countUserLogins].IsSuperUser
		//			, "userLoginResponseList.list[countUserLogins].IsSuperUser == expectedUserLoginResponseList[countExpectedUserLogins].IsSuperUser");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].Status);
		//		Assert.True(userLoginResponseList.list[countUserLogins].Status == expectedUserLoginResponseList[countUserLogins].Status
		//			, "userLoginResponseList.list[countUserLogins].Status == expectedUserLoginResponseList[countExpectedUserLogins].Status");
		//		Assert.True(userLoginResponseList.list[countUserLogins].Password == expectedUserLoginResponseList[countUserLogins].Password
		//			, "userLoginResponseList.list[countUserLogins].Password == expectedUserLoginResponseList[countExpectedUserLogins].Password");
		//		Assert.NotNull(userLoginResponseList.list[countUserLogins].IsInternalAdmin);
		//		Assert.True(userLoginResponseList.list[countUserLogins].IsInternalAdmin == expectedUserLoginResponseList[countUserLogins].IsInternalAdmin
		//			, "userLoginResponseList.list[countUserLogins].IsInternalAdmin == expectedUserLoginResponseList[countExpectedUserLogins].IsInternalAdmin");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName, "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//	Assert.NotNull(userLoginResponseList.Status.ErrorMsg);
		//	Assert.True(userLoginResponseList.Status.ErrorMsg.Length < 1
		//		, "userLoginResponseList.ErrorMessage.Length < 1");
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsActivateUserAllRecords()
		//{
		//	// Set up Payload
		//	payload = reusable.DoPatchUserLoginsPayload();
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?userLoginStatusType=Active&updateType=AllRecords");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	ObjectListOutput<UserLogin, IErrorData> userLoginResponseList = JsonConvert.DeserializeObject<
		//		ObjectListOutput<UserLogin, IErrorData>>(ResponseString);

		//	// Extract Expected Results
		//	List<UserLogin> expectedUserLoginResponseList = JsonConvert.DeserializeObject<List<UserLogin>>(payload);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.list.Count; countUserLogins++)
		//	{
		//		for (int countExpectedUserLogins = 0; countExpectedUserLogins < expectedUserLoginResponseList.Count; countExpectedUserLogins++)
		//		{
		//			if (userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId)
		//			{
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].UserId);
		//				Assert.True(userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId
		//					, "userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].PartyId);
		//				Assert.True(userLoginResponseList.list[countUserLogins].PartyId == expectedUserLoginResponseList[countExpectedUserLogins].PartyId
		//					, "userLoginResponseList.list[countUserLogins].PartyId == expectedUserLoginResponseList[countExpectedUserLogins].PartyId");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].RealPageId);
		//				Assert.True(userLoginResponseList.list[countUserLogins].RealPageId == expectedUserLoginResponseList[countExpectedUserLogins].RealPageId
		//					, "userLoginResponseList.list[countUserLogins].RealPageId == expectedUserLoginResponseList[countExpectedUserLogins].RealPageId");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].LoginName);
		//				Assert.True(userLoginResponseList.list[countUserLogins].LoginName == expectedUserLoginResponseList[countExpectedUserLogins].LoginName
		//					, "userLoginResponseList.list[countUserLogins].LoginName == expectedUserLoginResponseList[countExpectedUserLogins].LoginName");
		//				Assert.True(userLoginResponseList.list[countUserLogins].LoginNameType == expectedUserLoginResponseList[countExpectedUserLogins].LoginNameType
		//					, "userLoginResponseList.list[countUserLogins].LoginNameType == expectedUserLoginResponseList[countExpectedUserLogins].LoginNameType");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsActive);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsActive == expectedUserLoginResponseList[countExpectedUserLogins].IsActive
		//					, "userLoginResponseList.list[countUserLogins].IsActive == expectedUserLoginResponseList[countExpectedUserLogins].IsActive");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsLocked);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsLocked == expectedUserLoginResponseList[countExpectedUserLogins].IsLocked
		//					, "userLoginResponseList.list[countUserLogins].IsLocked == expectedUserLoginResponseList[countExpectedUserLogins].IsLocked");
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsTainted == expectedUserLoginResponseList[countExpectedUserLogins].IsTainted
		//					, "userLoginResponseList.list[countUserLogins].IsTainted == expectedUserLoginResponseList[countExpectedUserLogins].IsTainted");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsPending);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsPending == expectedUserLoginResponseList[countExpectedUserLogins].IsPending
		//					, "userLoginResponseList.list[countUserLogins].IsPending == expectedUserLoginResponseList[countExpectedUserLogins].IsPending");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsExpired);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsExpired == expectedUserLoginResponseList[countExpectedUserLogins].IsExpired
		//					, "userLoginResponseList.list[countUserLogins].IsExpired == expectedUserLoginResponseList[countExpectedUserLogins].IsExpired");
		//				Assert.True(userLoginResponseList.list[countUserLogins].PasswordModifiedDate == expectedUserLoginResponseList[countExpectedUserLogins].PasswordModifiedDate
		//					, "userLoginResponseList.list[countUserLogins].PasswordModifiedDate == expectedUserLoginResponseList[countExpectedUserLogins].PasswordModifiedDate");
		//				Assert.True(userLoginResponseList.list[countUserLogins].FromDate == expectedUserLoginResponseList[countExpectedUserLogins].FromDate
		//					, "userLoginResponseList.list[countUserLogins].FromDate == expectedUserLoginResponseList[countExpectedUserLogins].FromDate");
		//				Assert.True(userLoginResponseList.list[countUserLogins].ThruDate == expectedUserLoginResponseList[countExpectedUserLogins].ThruDate
		//					, "userLoginResponseList.list[countUserLogins].ThruDate == expectedUserLoginResponseList[countExpectedUserLogins].ThruDate");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].StatusSetDate);
		//				Assert.True(userLoginResponseList.list[countUserLogins].StatusSetDate == expectedUserLoginResponseList[countExpectedUserLogins].StatusSetDate
		//					, "userLoginResponseList.list[countUserLogins].StatusSetDate == expectedUserLoginResponseList[countExpectedUserLogins].StatusSetDate");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].LastLogin);
		//				Assert.True(userLoginResponseList.list[countUserLogins].LastLogin == expectedUserLoginResponseList[countExpectedUserLogins].LastLogin
		//					, "userLoginResponseList.list[countUserLogins].LastLogin == expectedUserLoginResponseList[countExpectedUserLogins].LastLogin");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsSuperUser);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsSuperUser == expectedUserLoginResponseList[countExpectedUserLogins].IsSuperUser
		//					, "userLoginResponseList.list[countUserLogins].IsSuperUser == expectedUserLoginResponseList[countExpectedUserLogins].IsSuperUser");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].Status);
		//				Assert.True(userLoginResponseList.list[countUserLogins].Status == expectedUserLoginResponseList[countExpectedUserLogins].Status
		//					, "userLoginResponseList.list[countUserLogins].Status == expectedUserLoginResponseList[countExpectedUserLogins].Status");
		//				Assert.True(userLoginResponseList.list[countUserLogins].Password == expectedUserLoginResponseList[countExpectedUserLogins].Password
		//					, "userLoginResponseList.list[countUserLogins].Password == expectedUserLoginResponseList[countExpectedUserLogins].Password");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsInternalAdmin);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsInternalAdmin == expectedUserLoginResponseList[countExpectedUserLogins].IsInternalAdmin
		//					, "userLoginResponseList.list[countUserLogins].IsInternalAdmin == expectedUserLoginResponseList[countExpectedUserLogins].IsInternalAdmin");

		//				string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//					, expectedUserLoginResponseList[countExpectedUserLogins].LoginName, "P@ssw0rd");
		//				Assert.NotNull(verifyLockedUser);
		//				if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//				{
		//					Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//				}
		//				else
		//				{
		//					Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//				}
		//				XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//			}
		//		}
		//	}
		//	Assert.NotNull(userLoginResponseList.Status.Success);
		//	Assert.True(userLoginResponseList.Status.Success, "userLoginResponseList.Status.Success");
		//	Assert.NotNull(userLoginResponseList.Status.ErrorCode);
		//	Assert.True(userLoginResponseList.Status.ErrorCode.Length < 1
		//		, "userLoginResponseList.ErrorCode.Length < 1");
		//	Assert.NotNull(userLoginResponseList.Status.ErrorMsg);
		//	Assert.True(userLoginResponseList.Status.ErrorMsg.Length < 1
		//		, "userLoginResponseList.ErrorMessage.Length < 1");
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsDisableUserBatch()
		//{
		//	// Set up Payload
		//	payload = reusable.DoPatchUserLoginsPayload();
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?userLoginStatusType=Disable&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	ObjectListOutput<UserLogin, IErrorData> userLoginResponseList = JsonConvert.DeserializeObject<
		//		ObjectListOutput<UserLogin, IErrorData>>(ResponseString);

		//	// Extract Expected Results
		//	List<UserLogin> expectedUserLoginResponseList = JsonConvert.DeserializeObject<List<UserLogin>>(payload);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.list.Count; countUserLogins++)
		//	{
		//		for (int countExpectedUserLogins = 0; countExpectedUserLogins < expectedUserLoginResponseList.Count; countExpectedUserLogins++)
		//		{
		//			if (userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId)
		//			{
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].UserId);
		//				Assert.True(userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId
		//					, "userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].PartyId);
		//				Assert.True(userLoginResponseList.list[countUserLogins].PartyId == expectedUserLoginResponseList[countExpectedUserLogins].PartyId
		//					, "userLoginResponseList.list[countUserLogins].PartyId == expectedUserLoginResponseList[countExpectedUserLogins].PartyId");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].RealPageId);
		//				Assert.True(userLoginResponseList.list[countUserLogins].RealPageId == expectedUserLoginResponseList[countExpectedUserLogins].RealPageId
		//					, "userLoginResponseList.list[countUserLogins].RealPageId == expectedUserLoginResponseList[countExpectedUserLogins].RealPageId");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].LoginName);
		//				Assert.True(userLoginResponseList.list[countUserLogins].LoginName == expectedUserLoginResponseList[countExpectedUserLogins].LoginName
		//					, "userLoginResponseList.list[countUserLogins].LoginName == expectedUserLoginResponseList[countExpectedUserLogins].LoginName");
		//				Assert.True(userLoginResponseList.list[countUserLogins].LoginNameType == expectedUserLoginResponseList[countExpectedUserLogins].LoginNameType
		//					, "userLoginResponseList.list[countUserLogins].LoginNameType == expectedUserLoginResponseList[countExpectedUserLogins].LoginNameType");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsActive);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsActive == expectedUserLoginResponseList[countExpectedUserLogins].IsActive
		//					, "userLoginResponseList.list[countUserLogins].IsActive == expectedUserLoginResponseList[countExpectedUserLogins].IsActive");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsLocked);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsLocked == expectedUserLoginResponseList[countExpectedUserLogins].IsLocked
		//					, "userLoginResponseList.list[countUserLogins].IsLocked == expectedUserLoginResponseList[countExpectedUserLogins].IsLocked");
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsTainted == expectedUserLoginResponseList[countExpectedUserLogins].IsTainted
		//					, "userLoginResponseList.list[countUserLogins].IsTainted == expectedUserLoginResponseList[countExpectedUserLogins].IsTainted");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsPending);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsPending == expectedUserLoginResponseList[countExpectedUserLogins].IsPending
		//					, "userLoginResponseList.list[countUserLogins].IsPending == expectedUserLoginResponseList[countExpectedUserLogins].IsPending");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsExpired);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsExpired == expectedUserLoginResponseList[countExpectedUserLogins].IsExpired
		//					, "userLoginResponseList.list[countUserLogins].IsExpired == expectedUserLoginResponseList[countExpectedUserLogins].IsExpired");
		//				Assert.True(userLoginResponseList.list[countUserLogins].PasswordModifiedDate == expectedUserLoginResponseList[countExpectedUserLogins].PasswordModifiedDate
		//					, "userLoginResponseList.list[countUserLogins].PasswordModifiedDate == expectedUserLoginResponseList[countExpectedUserLogins].PasswordModifiedDate");
		//				Assert.True(userLoginResponseList.list[countUserLogins].FromDate == expectedUserLoginResponseList[countExpectedUserLogins].FromDate
		//					, "userLoginResponseList.list[countUserLogins].FromDate == expectedUserLoginResponseList[countExpectedUserLogins].FromDate");
		//				Assert.True(userLoginResponseList.list[countUserLogins].ThruDate == expectedUserLoginResponseList[countExpectedUserLogins].ThruDate
		//					, "userLoginResponseList.list[countUserLogins].ThruDate == expectedUserLoginResponseList[countExpectedUserLogins].ThruDate");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].StatusSetDate);
		//				Assert.True(userLoginResponseList.list[countUserLogins].StatusSetDate == expectedUserLoginResponseList[countExpectedUserLogins].StatusSetDate
		//					, "userLoginResponseList.list[countUserLogins].StatusSetDate == expectedUserLoginResponseList[countExpectedUserLogins].StatusSetDate");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].LastLogin);
		//				Assert.True(userLoginResponseList.list[countUserLogins].LastLogin == expectedUserLoginResponseList[countExpectedUserLogins].LastLogin
		//					, "userLoginResponseList.list[countUserLogins].LastLogin == expectedUserLoginResponseList[countExpectedUserLogins].LastLogin");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsSuperUser);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsSuperUser == expectedUserLoginResponseList[countExpectedUserLogins].IsSuperUser
		//					, "userLoginResponseList.list[countUserLogins].IsSuperUser == expectedUserLoginResponseList[countExpectedUserLogins].IsSuperUser");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].Status);
		//				Assert.True(userLoginResponseList.list[countUserLogins].Status == expectedUserLoginResponseList[countExpectedUserLogins].Status
		//					, "userLoginResponseList.list[countUserLogins].Status == expectedUserLoginResponseList[countExpectedUserLogins].Status");
		//				Assert.True(userLoginResponseList.list[countUserLogins].Password == expectedUserLoginResponseList[countExpectedUserLogins].Password
		//					, "userLoginResponseList.list[countUserLogins].Password == expectedUserLoginResponseList[countExpectedUserLogins].Password");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsInternalAdmin);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsInternalAdmin == expectedUserLoginResponseList[countExpectedUserLogins].IsInternalAdmin
		//					, "userLoginResponseList.list[countUserLogins].IsInternalAdmin == expectedUserLoginResponseList[countExpectedUserLogins].IsInternalAdmin");

		//				string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//					, expectedUserLoginResponseList[countExpectedUserLogins].LoginName, "P@ssw0rd");
		//				Assert.NotNull(verifyLockedUser);
		//				if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//				{
		//					Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//				}
		//				else
		//				{
		//					Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//				}
		//				XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//			}
		//		}
		//	}
		//	Assert.NotNull(userLoginResponseList.Status.Success);
		//	Assert.True(userLoginResponseList.Status.Success, "userLoginResponseList.Status.Success");
		//	Assert.NotNull(userLoginResponseList.Status.ErrorCode);
		//	Assert.True(userLoginResponseList.Status.ErrorCode.Length < 1
		//		, "userLoginResponseList.ErrorCode.Length < 1");
		//	Assert.NotNull(userLoginResponseList.Status.ErrorMsg);
		//	Assert.True(userLoginResponseList.Status.ErrorMsg.Length < 1
		//		, "userLoginResponseList.ErrorMessage.Length < 1");
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsDisableUserAllRecords()
		//{
		//	// Set up Payload
		//	payload = reusable.DoPatchUserLoginsPayload();
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?userLoginStatusType=Disable&updateType=AllRecords");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Reactivate users
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?userLoginStatusType=Disable&updateType=AllRecords");
			
		//	XunitTestOutPut.WriteLine("Reactivating User by calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);
			
		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	ObjectListOutput<UserLogin, IErrorData> userLoginResponseList = JsonConvert.DeserializeObject<
		//		ObjectListOutput<UserLogin, IErrorData>>(ResponseString);

		//	// Extract Expected Results
		//	List<UserLogin> expectedUserLoginResponseList = JsonConvert.DeserializeObject<List<UserLogin>>(payload);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.list.Count; countUserLogins++)
		//	{
		//		for (int countExpectedUserLogins = 0; countExpectedUserLogins < expectedUserLoginResponseList.Count; countExpectedUserLogins++)
		//		{
		//			if (userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId)
		//			{
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].UserId);
		//				Assert.True(userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId
		//					, "userLoginResponseList.list[countUserLogins].UserId == expectedUserLoginResponseList[countExpectedUserLogins].UserId");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].PartyId);
		//				Assert.True(userLoginResponseList.list[countUserLogins].PartyId == expectedUserLoginResponseList[countExpectedUserLogins].PartyId
		//					, "userLoginResponseList.list[countUserLogins].PartyId == expectedUserLoginResponseList[countExpectedUserLogins].PartyId");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].RealPageId);
		//				Assert.True(userLoginResponseList.list[countUserLogins].RealPageId == expectedUserLoginResponseList[countExpectedUserLogins].RealPageId
		//					, "userLoginResponseList.list[countUserLogins].RealPageId == expectedUserLoginResponseList[countExpectedUserLogins].RealPageId");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].LoginName);
		//				Assert.True(userLoginResponseList.list[countUserLogins].LoginName == expectedUserLoginResponseList[countExpectedUserLogins].LoginName
		//					, "userLoginResponseList.list[countUserLogins].LoginName == expectedUserLoginResponseList[countExpectedUserLogins].LoginName");
		//				Assert.True(userLoginResponseList.list[countUserLogins].LoginNameType == expectedUserLoginResponseList[countExpectedUserLogins].LoginNameType
		//					, "userLoginResponseList.list[countUserLogins].LoginNameType == expectedUserLoginResponseList[countExpectedUserLogins].LoginNameType");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsActive);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsActive == expectedUserLoginResponseList[countExpectedUserLogins].IsActive
		//					, "userLoginResponseList.list[countUserLogins].IsActive == expectedUserLoginResponseList[countExpectedUserLogins].IsActive");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsLocked);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsLocked == expectedUserLoginResponseList[countExpectedUserLogins].IsLocked
		//					, "userLoginResponseList.list[countUserLogins].IsLocked == expectedUserLoginResponseList[countExpectedUserLogins].IsLocked");
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsTainted == expectedUserLoginResponseList[countExpectedUserLogins].IsTainted
		//					, "userLoginResponseList.list[countUserLogins].IsTainted == expectedUserLoginResponseList[countExpectedUserLogins].IsTainted");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsPending);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsPending == expectedUserLoginResponseList[countExpectedUserLogins].IsPending
		//					, "userLoginResponseList.list[countUserLogins].IsPending == expectedUserLoginResponseList[countExpectedUserLogins].IsPending");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsExpired);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsExpired == expectedUserLoginResponseList[countExpectedUserLogins].IsExpired
		//					, "userLoginResponseList.list[countUserLogins].IsExpired == expectedUserLoginResponseList[countExpectedUserLogins].IsExpired");
		//				Assert.True(userLoginResponseList.list[countUserLogins].PasswordModifiedDate == expectedUserLoginResponseList[countExpectedUserLogins].PasswordModifiedDate
		//					, "userLoginResponseList.list[countUserLogins].PasswordModifiedDate == expectedUserLoginResponseList[countExpectedUserLogins].PasswordModifiedDate");
		//				Assert.True(userLoginResponseList.list[countUserLogins].FromDate == expectedUserLoginResponseList[countExpectedUserLogins].FromDate
		//					, "userLoginResponseList.list[countUserLogins].FromDate == expectedUserLoginResponseList[countExpectedUserLogins].FromDate");
		//				Assert.True(userLoginResponseList.list[countUserLogins].ThruDate == expectedUserLoginResponseList[countExpectedUserLogins].ThruDate
		//					, "userLoginResponseList.list[countUserLogins].ThruDate == expectedUserLoginResponseList[countExpectedUserLogins].ThruDate");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].StatusSetDate);
		//				Assert.True(userLoginResponseList.list[countUserLogins].StatusSetDate == expectedUserLoginResponseList[countExpectedUserLogins].StatusSetDate
		//					, "userLoginResponseList.list[countUserLogins].StatusSetDate == expectedUserLoginResponseList[countExpectedUserLogins].StatusSetDate");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].LastLogin);
		//				Assert.True(userLoginResponseList.list[countUserLogins].LastLogin == expectedUserLoginResponseList[countExpectedUserLogins].LastLogin
		//					, "userLoginResponseList.list[countUserLogins].LastLogin == expectedUserLoginResponseList[countExpectedUserLogins].LastLogin");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsSuperUser);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsSuperUser == expectedUserLoginResponseList[countExpectedUserLogins].IsSuperUser
		//					, "userLoginResponseList.list[countUserLogins].IsSuperUser == expectedUserLoginResponseList[countExpectedUserLogins].IsSuperUser");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].Status);
		//				Assert.True(userLoginResponseList.list[countUserLogins].Status == expectedUserLoginResponseList[countExpectedUserLogins].Status
		//					, "userLoginResponseList.list[countUserLogins].Status == expectedUserLoginResponseList[countExpectedUserLogins].Status");
		//				Assert.True(userLoginResponseList.list[countUserLogins].Password == expectedUserLoginResponseList[countExpectedUserLogins].Password
		//					, "userLoginResponseList.list[countUserLogins].Password == expectedUserLoginResponseList[countExpectedUserLogins].Password");
		//				Assert.NotNull(userLoginResponseList.list[countUserLogins].IsInternalAdmin);
		//				Assert.True(userLoginResponseList.list[countUserLogins].IsInternalAdmin == expectedUserLoginResponseList[countExpectedUserLogins].IsInternalAdmin
		//					, "userLoginResponseList.list[countUserLogins].IsInternalAdmin == expectedUserLoginResponseList[countExpectedUserLogins].IsInternalAdmin");

		//				string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//					, expectedUserLoginResponseList[countExpectedUserLogins].LoginName, "P@ssw0rd");
		//				Assert.NotNull(verifyLockedUser);
		//				if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//				{
		//					Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//				}
		//				else
		//				{
		//					Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//				}
		//				XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//			}
		//		}
		//	}
		//	Assert.NotNull(userLoginResponseList.Status.Success);
		//	Assert.True(userLoginResponseList.Status.Success, "userLoginResponseList.Status.Success");
		//	Assert.NotNull(userLoginResponseList.Status.ErrorCode);
		//	Assert.True(userLoginResponseList.Status.ErrorCode.Length < 1
		//		, "userLoginResponseList.ErrorCode.Length < 1");
		//	Assert.NotNull(userLoginResponseList.Status.ErrorMsg);
		//	Assert.True(userLoginResponseList.Status.ErrorMsg.Length < 1
		//		, "userLoginResponseList.ErrorMessage.Length < 1");
		//}

		//[Fact, Trait("", "Negative Case")]
		//public void PatchUserLoginsActivateUserWithoutUpdateType()
		//{
		//	// Set up Payload
		//	payload = reusable.DoPatchUserLoginsPayload();
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?userLoginStatusType=Active&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(ResponseString.Contains("Internal System Error. Please contact RealPage support with error reference Id - ")
		//		, "ResponseString.Contains(\"Internal System Error.Please contact RealPage support with error reference Id - \")");
		//}

		//[Fact, Trait("", "Negative Case")]
		//public void PatchUserLoginsDisableUserWithoutUpdateType()
		//{
		//	// Set up Payload
		//	payload = reusable.DoPatchUserLoginsPayload();
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/", "userlogins?userLoginStatusType=Active&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(ResponseString.Contains("Internal System Error. Please contact RealPage support with error reference Id - ")
		//		, "ResponseString.Contains(\"Internal System Error.Please contact RealPage support with error reference Id - \")");
		//}
		
		////[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsActivateUserWithoutOrganizationRealPageId()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=Active&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		////[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsDisableUserWithoutOrganizationRealPageId()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=Disabled&updateType=Batch");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}
		
		////[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsActivateUserWithUserLoginStatusTypeOnly()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=Active&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		////[Fact, Trait("", "Data-Driven")]
		//public void PatchUserLoginsDisableUserWithUserLoginStatusTypeOnly()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	// Execute API for Payload
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

		//	// Extract API's JSON Payload to be used as Expected Response
		//	
		//	UserLogin expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);

		//	List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	// Get and Add 2nd GET UserLogin API's JSON Response to Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);
		//	EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
		//	ResponseString = GetHttpWebResponseString(response);
		//	expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
		//	expectedUserLoginResponseList.Add(expectedUserLoginResponse);

		//	payload = JsonConvert.SerializeObject(expectedUserLoginResponseList);
		//	XunitTestOutPut.WriteLine("Payload:\n" + payload);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogins"].Replace("userlogins/"
		//		, "userlogins?organizationRealPageId=&userLoginStatusType=Disabled&updateType=");

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Patch + " at " + EndPointUrl);
		//	response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Patch, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	ResponseString = GetHttpWebResponseString(response);
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
		//	List<RepositoryResponse> userLoginResponseList = JsonConvert.DeserializeObject<List<RepositoryResponse>>(ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	for (int countUserLogins = 0; countUserLogins < userLoginResponseList.Count; countUserLogins++)
		//	{
		//		Assert.NotNull(userLoginResponseList[countUserLogins].Id);
		//		Assert.True(userLoginResponseList[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId
		//			, "userLoginResponse[countUserLogins].Id == expectedUserLoginResponseList[countUserLogins].UserId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].RealPageId);
		//		Assert.True(userLoginResponseList[countUserLogins].RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId
		//			, "userLoginResponse.RealPageId == expectedUserLoginResponseList[countUserLogins].RealPageId");
		//		Assert.NotNull(userLoginResponseList[countUserLogins].ErrorMessage);
		//		Assert.True(userLoginResponseList[countUserLogins].ErrorMessage.Length < 1
		//			, "userLoginResponseList[countUserLogins].ErrorMessage.Length < 1");

		//		string verifyLockedUser = GetClientToken(Properties["identityClientUrl"]
		//			, expectedUserLoginResponseList[countUserLogins].LoginName
		//			, expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail") ? "" : "P@ssw0rd");
		//		Assert.NotNull(verifyLockedUser);
		//		if (expectedUserLoginResponseList[countUserLogins].LoginName.Contains("gmail"))
		//		{
		//			Assert.True(verifyLockedUser.Contains("invalid_grant"), "verifyLockedUser.Contains(\"invalid_grant\")");
		//		}
		//		else
		//		{
		//			Assert.True(verifyLockedUser.Contains("User account is locked."), "verifyLockedUser.Contains(\"User account is locked.\")");
		//		}
		//		XunitTestOutPut.WriteLine("\n\nLog-in Attempt Error:\n" + verifyLockedUser);
		//	}
		//}

		//[Fact, Trait("", "Happy Path")]
		//public void PostUserLoginStatusActivateUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Active&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(Convert.ToBoolean(ResponseString), "Convert.ToBoolean(ResponseString)");
			
		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PostUserLoginStatusAccountCreationPendUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=AccountCreationPending&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(Convert.ToBoolean(ResponseString), "Convert.ToBoolean(ResponseString)");

		//	// Reactivate User after Testing
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Active&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Reactivate User by calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PostUserLoginStatusLockUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Locked&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(Convert.ToBoolean(ResponseString), "Convert.ToBoolean(ResponseString)");

		//	// Reactivate User after Testing
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Active&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Reactivate User by calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PostUserLoginStatusDisableUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Disabled&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(Convert.ToBoolean(ResponseString), "Convert.ToBoolean(ResponseString)");

		//	// Reactivate User after Testing
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Active&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Reactivate User by calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PostUserLoginStatusUnlockUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Unlocked&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(Convert.ToBoolean(ResponseString), "Convert.ToBoolean(ResponseString)");

		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PostUserLoginStatusAccountCreationExpireUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=AccountCreationExpired&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(Convert.ToBoolean(ResponseString), "Convert.ToBoolean(ResponseString)");

		//	// Reactivate User after Testing
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Active&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Reactivate User by calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//}

		//[Fact, Trait("", "Data-Driven")]
		//public void PostUserLoginStatusAccountCreationSucceedUser()
		//{
		//	// Set up Payload
		//	payload = reusable.DoGetUserLoginUser(userLoginsUser);

		//	//Set up the API URL
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=AccountCreationSuccessful&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

		//	//Execute API
		//	XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//	// Extract API's JSON Response
		//	
		//	XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

		//	// Assert
		//	Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
		//	Assert.NotNull(ResponseString);
		//	Assert.True(Convert.ToBoolean(ResponseString), "Convert.ToBoolean(ResponseString)");

		//	// Reactivate User after Testing
		//	EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=Active&realPageId="
		//		+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
		//	XunitTestOutPut.WriteLine("Reactivate User by calling " + HttpVerb.Post + " at " + EndPointUrl);
		//	GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

		//}

		//[Fact, Trait("", "Data-Driven")]
		public void PostUserLoginStatusWithoutStatusTypeName()
		{
			// Set up Payload
			payload = reusable.DoGetUserLoginUser(userLoginsUser);

			//Set up the API URL
			EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=&realPageId="
				+ JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(Convert.ToBoolean(ResponseString), "Convert.ToBoolean(ResponseString)");
			
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginStatusWithoutRealPageId()
		{
			//Set up the API URL
			EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=AccountCreationSuccessful&realPageId=";

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("Invalid parameter: realPageId"), "ResponseString.Contains(\"Invalid parameter: realPageId\")");
			
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostUserLoginStatusWithoutStatusTypeNameAndRealPageId()
		{
			//Set up the API URL
			EndPointUrl = HostUrl + Properties["UserLogin"] + "/Status?statusTypeName=&realPageId=";

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("Invalid parameter: realPageId"), "ResponseString.Contains(\"Invalid parameter: realPageId\")");

		}
	}
}
