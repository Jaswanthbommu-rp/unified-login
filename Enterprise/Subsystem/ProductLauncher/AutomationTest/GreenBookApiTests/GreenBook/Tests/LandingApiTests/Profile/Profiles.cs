using System.Net;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Data;
using System;
using GreenBook.Models;

namespace GreenBook.Tests
{
    public class Profiles : TestController
    {
        private string payload;
        JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
        private string realPageId = "";
        private DataTable expectedContactMechanism = new DataTable();
        private DataTable expectedUserLogin = new DataTable();

        public Profiles(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
            
        }

		// Profiles=/api/Profiles/
		
		//[Fact, Trait("", "Happy Path")]
		public void PutProfiles()
		{
			// Set up Payload
			payload = reusable.DoPutProfilesPayload();
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Extract Expected JSON Response
			Profile expectedProfile = JsonConvert.DeserializeObject<Profile>(payload);

			//Set up the API URL
			EndPointUrl = HostUrl + Properties["Profiles"] + (expectedProfile.RealPageId);

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			ObjectOutput<Profile, IErrorData> profileResponse = JsonConvert.DeserializeObject<ObjectOutput<Profile, IErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");


            Assert.NotNull(profileResponse.obj.TelecommunicationNumber);

            int countTelecommunicationNumber = 0;
            
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].PartyContactMechanismId
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].PartyContactMechanismId
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].PartyContactMechanismId "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].PartyContactMechanismId");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].ContactMechanismId
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].ContactMechanismId
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].ContactMechanismId "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].ContactMechanismId");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].CountryCode
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].CountryCode
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].CountryCode "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].CountryCode");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].AreaCode
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].AreaCode
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].AreaCode "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].AreaCode");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].PhoneNumber
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].PhoneNumber
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].PhoneNumber "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].PhoneNumber");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ContactMechanismUsageTypeId
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ContactMechanismUsageTypeId
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ContactMechanismUsageTypeId "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ContactMechanismUsageTypeId");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ParentContactMechanismUsageTypeId
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ParentContactMechanismUsageTypeId
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ParentContactMechanismUsageTypeId "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.Name
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.Name
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.Name "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.Name");
          

            Assert.True(profileResponse.obj.PartyRole.PartyRoleId == expectedProfile.PartyRole.PartyRoleId
                , "profileResponse.obj.PartyRole.PartyRoleId == expectedProfile.PartyRole.PartyRoleId");
            Assert.True(profileResponse.obj.PartyRole.PartyId == expectedProfile.PartyRole.PartyId
                , "profileResponse.obj.PartyRole.PartyId == expectedProfile.PartyRole.PartyId");
            Assert.True(profileResponse.obj.PartyRole.RoleTypeId == expectedProfile.PartyRole.RoleTypeId
                , "profileResponse.obj.PartyRole.RoleTypeId == expectedProfile.PartyRole.RoleTypeId");
            Assert.True(profileResponse.obj.PartyRole.Name == expectedProfile.PartyRole.Name
                , "profileResponse.obj.PartyRole.Name == expectedProfile.PartyRole.Name");
            Assert.NotNull(profileResponse.obj.PartyId);
			Assert.True(profileResponse.obj.PartyId == expectedProfile.PartyId
				, "profileResponse.obj.PartyId == expectedProfile.PartyId");
			Assert.NotNull(profileResponse.obj.RealPageId);
			Assert.True(profileResponse.obj.RealPageId == expectedProfile.RealPageId
				, "profileResponse.obj.RealPageId == expectedProfile.RealPageId");
			Assert.NotNull(profileResponse.obj.FirstName);
			Assert.True(profileResponse.obj.FirstName == expectedProfile.FirstName
				, "profileResponse.obj.FirstName == expectedProfile.FirstName");
			Assert.NotNull(profileResponse.obj.LastName);
			Assert.True(profileResponse.obj.LastName == expectedProfile.LastName
				, "profileResponse.obj.LastName == expectedProfile.LastName");
			Assert.NotNull(profileResponse.obj.MiddleName);
			Assert.True(profileResponse.obj.MiddleName == expectedProfile.MiddleName
				, "profileResponse.obj.MiddleName == expectedProfile.MiddleName");
			Assert.True(profileResponse.obj.Suffix == expectedProfile.Suffix
				, "profileResponse.obj.Suffix == expectedProfile.Suffix");
			Assert.True(profileResponse.obj.Title == expectedProfile.Title
				, "profileResponse.obj.Title == expectedProfile.Title");
			Assert.NotNull(profileResponse.obj.PreferredContactMethodId);
			Assert.True(profileResponse.obj.PreferredContactMethodId == expectedProfile.PreferredContactMethodId
				, "profileResponse.obj.PreferredContactMethodId == expectedProfile.PreferredContactMethodId");
			Assert.NotNull(profileResponse.Status.Success);
			Assert.True(profileResponse.Status.Success, "profileResponse.Status.Success");
			Assert.NotNull(profileResponse.Status.ErrorCode);
			Assert.True(profileResponse.Status.ErrorCode == "", "profileResponse.Status.ErrorCode == \"\"");
			Assert.NotNull(profileResponse.Status.ErrorMsg);
			Assert.True(profileResponse.Status.ErrorMsg == "", "profileResponse.Status.ErrorMsg == \"\"");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PutProfilesInvalidRealPageId()
		{
			// Set up Payload
			payload = reusable.DoPutProfilesPayload();
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			//Set up the API URL
			EndPointUrl = HostUrl + Properties["Profiles"] + "invalidRealPageId";

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PutProfilesNullTelecommunicationNumberOnPayload()
		{
			// Set up Payload
			payload = reusable.DoPutProfilesPayload();
			XunitTestOutPut.WriteLine("Payload:\n" + payload);
			Profile expectedProfile = JsonConvert.DeserializeObject<Profile>(payload);
			expectedProfile.TelecommunicationNumber = null;
			payload = JsonConvert.SerializeObject(expectedProfile);

			//Set up the API URL
			EndPointUrl = HostUrl + Properties["Profiles"] + expectedProfile.RealPageId;

			//Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("Internal System Error. Please contact RealPage support with error reference Id - ")
				, "ResponseString.Contains(\"Internal System Error. Please contact RealPage support with error reference Id - \")");
		}

        [Fact, Trait("", "Happy Path")]
        public void GetProfile()
        {
            //_accessToken = GetClientToken(Properties["identityClientUrl"], Properties["enterpriseUsername6"], "P@ssw0rd");
            //realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(Properties["enterpriseUsername6"])).RealPageId.ToString();

            var personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId;
            var realpageId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona(personaId)).RealPageId;

            // Extract Expected JSON Response
            Profile expectedProfile = JsonConvert.DeserializeObject<Profile>(reusable.DoGetProfile(realpageId.ToString()));
            XunitTestOutPut.WriteLine("Expected JSON Response:\n" + JsonConvert.SerializeObject(expectedProfile));

            // Execute API
            EndPointUrl = HostUrl + Properties["Profiles"] + realpageId.ToString();
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response			
            ObjectOutput<Profile, IErrorData> profileResponse = JsonConvert.DeserializeObject<ObjectOutput<Profile, IErrorData>>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

            //Assert UserLogin
            Assert.NotNull(profileResponse.obj.userLogin);

            Assert.NotNull(profileResponse.obj.userLogin.UserId);
            Assert.True(profileResponse.obj.userLogin.UserId == expectedProfile.userLogin.UserId, "profileResponse.obj.userLogin.UserId == expectedProfile.userLogin.UserId");
            Assert.NotNull(profileResponse.obj.userLogin.PartyId);
            Assert.True(profileResponse.obj.userLogin.PartyId == expectedProfile.userLogin.PartyId, "profileResponse.obj.userLogin.PartyId == expectedProfile.userLogin.PartyId");
            Assert.NotNull(profileResponse.obj.userLogin.RealPageId);
            Assert.True(profileResponse.obj.userLogin.RealPageId == expectedProfile.userLogin.RealPageId, "profileResponse.obj.userLogin.RealPageId == expectedProfile.userLogin.RealPageId");
            Assert.NotNull(profileResponse.obj.userLogin.LoginName);
            Assert.True(profileResponse.obj.userLogin.LoginName == expectedProfile.userLogin.LoginName, "profileResponse.obj.userLogin.LoginName == expectedProfile.userLogin.LoginName");
            if (profileResponse.obj.userLogin.LoginNameType != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.LoginNameType);
                //Assert.True(profileResponse.obj.userLogin.LoginNameType == expectedProfile.userLogin.LoginNameType, "profileResponse.obj.userLogin.LoginNameType == expectedProfile.userLogin.LoginNameType");
            }
            Assert.NotNull(profileResponse.obj.userLogin.IsActive);
            Assert.True(profileResponse.obj.userLogin.IsActive == expectedProfile.userLogin.IsActive, "profileResponse.obj.userLogin.IsActive == expectedProfile.userLogin.IsActive");
            Assert.NotNull(profileResponse.obj.userLogin.IsLocked);
            Assert.True(profileResponse.obj.userLogin.IsLocked == expectedProfile.userLogin.IsLocked, "profileResponse.obj.userLogin.IsLocked == expectedProfile.userLogin.IsLocked");
            Assert.NotNull(profileResponse.obj.userLogin.IsPending);
            Assert.True(profileResponse.obj.userLogin.IsPending == expectedProfile.userLogin.IsPending, "profileResponse.obj.userLogin.IsPending == expectedProfile.userLogin.IsPending");
            //Assert.NotNull(profileResponse.obj.userLogin.IsTainted);
            Assert.NotNull(profileResponse.obj.userLogin.IsExpired);
            Assert.True(profileResponse.obj.userLogin.IsExpired == expectedProfile.userLogin.IsExpired, "profileResponse.obj.userLogin.IsExpired == expectedProfile.userLogin.IsExpired");
            Assert.NotNull(profileResponse.obj.userLogin.PasswordModifiedDate);
            Assert.True(profileResponse.obj.userLogin.PasswordModifiedDate == expectedProfile.userLogin.PasswordModifiedDate, "profileResponse.obj.userLogin.PasswordModifiedDate == expectedProfile.userLogin.PasswordModifiedDate");
            Assert.NotNull(profileResponse.obj.userLogin.FromDate);
            Assert.True(profileResponse.obj.userLogin.FromDate == expectedProfile.userLogin.FromDate, "profileResponse.obj.userLogin.FromDate == expectedProfile.userLogin.FromDate");
            if (profileResponse.obj.userLogin.ThruDate != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.ThruDate);
                Assert.True(profileResponse.obj.userLogin.ThruDate == expectedProfile.userLogin.ThruDate, "profileResponse.obj.userLogin.ThruDate == expectedProfile.userLogin.ThruDate");
            }
            if (profileResponse.obj.userLogin.LastLogin != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.LastLogin);
                Assert.True(profileResponse.obj.userLogin.LastLogin == expectedProfile.userLogin.LastLogin, "profileResponse.obj.userLogin.LastLogin == expectedProfile.userLogin.LastLogin");
            }
            Assert.NotNull(profileResponse.obj.userLogin.IsSuperUser);
            Assert.True(profileResponse.obj.userLogin.IsSuperUser == expectedProfile.userLogin.IsSuperUser, "profileResponse.obj.userLogin.IsSuperUser == expectedProfile.userLogin.IsSuperUser");
            Assert.NotNull(profileResponse.obj.userLogin.Status);
            Assert.True(profileResponse.obj.userLogin.Status == expectedProfile.userLogin.Status, "profileResponse.obj.userLogin.Status == expectedProfile.userLogin.Status");
            if (profileResponse.obj.userLogin.Password != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.Password);
                Assert.True(profileResponse.obj.userLogin.Password == expectedProfile.userLogin.Password, "profileResponse.obj.userLogin.Password == expectedProfile.userLogin.Password");
            }
            if (profileResponse.obj.userLogin.UserRoleType != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.UserRoleType);
                Assert.True(profileResponse.obj.userLogin.UserRoleType == expectedProfile.userLogin.UserRoleType, "profileResponse.obj.userLogin.UserRoleType == expectedProfile.userLogin.UserRoleType");
            }
            Assert.NotNull(profileResponse.obj.userLogin.Is3rdPartyIDP);
            Assert.True(profileResponse.obj.userLogin.Is3rdPartyIDP == expectedProfile.userLogin.Is3rdPartyIDP, "profileResponse.obj.userLogin.Is3rdPartyIDP == expectedProfile.userLogin.Is3rdPartyIDP");

            //TelecommunicationNumber
            int countTelecommunicationNumber = 0;

            Assert.NotNull(profileResponse.obj.TelecommunicationNumber);
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].PartyContactMechanismId
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].PartyContactMechanismId
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].PartyContactMechanismId "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].PartyContactMechanismId");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].ContactMechanismId
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].ContactMechanismId
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].ContactMechanismId "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].ContactMechanismId");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].CountryCode
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].CountryCode
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].CountryCode "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].CountryCode");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].AreaCode
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].AreaCode
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].AreaCode "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].AreaCode");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].PhoneNumber
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].PhoneNumber
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].PhoneNumber "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].PhoneNumber");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ContactMechanismUsageTypeId
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ContactMechanismUsageTypeId
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ContactMechanismUsageTypeId "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ContactMechanismUsageTypeId");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ParentContactMechanismUsageTypeId
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ParentContactMechanismUsageTypeId
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ParentContactMechanismUsageTypeId "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
                Assert.True(profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.Name
                    == expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.Name
                    , "profileResponse.obj.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.Name "
                    + "== expectedProfile.TelecommunicationNumber[countTelecommunicationNumber].contactMechanismUsageType.Name");
          
            Assert.NotNull(profileResponse.obj.PartyRole.PartyRoleId);
            Assert.True(profileResponse.obj.PartyRole.PartyRoleId == expectedProfile.PartyRole.PartyRoleId
                , "profileResponse.obj.PartyRole.PartyRoleId == expectedProfile.PartyRole.PartyRoleId");
            Assert.NotNull(profileResponse.obj.PartyRole.PartyId);
            Assert.True(profileResponse.obj.PartyRole.PartyId == expectedProfile.PartyRole.PartyId
                , "profileResponse.obj.PartyRole.PartyId == expectedProfile.PartyRole.PartyId");
            Assert.NotNull(profileResponse.obj.PartyRole.RoleTypeId);
            Assert.True(profileResponse.obj.PartyRole.RoleTypeId == expectedProfile.PartyRole.RoleTypeId
                , "profileResponse.obj.PartyRole.RoleTypeId == expectedProfile.PartyRole.RoleTypeId");
            Assert.NotNull(profileResponse.obj.PartyRole.Name);
            Assert.True(profileResponse.obj.PartyRole.Name == expectedProfile.PartyRole.Name
                , "profileResponse.obj.PartyRole.Name == expectedProfile.PartyRole.Name");
            Assert.NotNull(profileResponse.obj.PartyId);
            Assert.True(profileResponse.obj.PartyId == expectedProfile.PartyId
                , "profileResponse.obj.PartyId == expectedProfile.PartyId");
            Assert.NotNull(profileResponse.obj.RealPageId);
            Assert.True(profileResponse.obj.RealPageId == expectedProfile.RealPageId
                , "profileResponse.obj.RealPageId == expectedProfile.RealPageId");
            Assert.NotNull(profileResponse.obj.FirstName);
            Assert.True(profileResponse.obj.FirstName == expectedProfile.FirstName
                , "profileResponse.obj.FirstName == expectedProfile.FirstName");
            Assert.NotNull(profileResponse.obj.LastName);
            Assert.True(profileResponse.obj.LastName == expectedProfile.LastName
                , "profileResponse.obj.LastName == expectedProfile.LastName");
            Assert.NotNull(profileResponse.obj.MiddleName);
            Assert.True(profileResponse.obj.MiddleName == expectedProfile.MiddleName
                , "profileResponse.obj.MiddleName == expectedProfile.MiddleName");
            if (profileResponse.obj.Suffix != null)
            { 
                Assert.True(profileResponse.obj.Suffix == expectedProfile.Suffix
                    , "profileResponse.obj.Suffix == expectedProfile.Suffix");
            }
            Assert.NotNull(profileResponse.obj.Title);
            Assert.True(profileResponse.obj.Title == expectedProfile.Title
                , "profileResponse.obj.Title == expectedProfile.Title");
            Assert.NotNull(profileResponse.obj.PreferredContactMethodId);
                    
            Assert.NotNull(profileResponse.Status.Success);
            Assert.True(profileResponse.Status.Success, "profileResponse.Status.Success");
            Assert.NotNull(profileResponse.Status.ErrorCode);
            Assert.True(profileResponse.Status.ErrorCode == "", "profileResponse.Status.ErrorCode == \"\"");
            Assert.NotNull(profileResponse.Status.ErrorMsg);
            Assert.True(profileResponse.Status.ErrorMsg == "", "profileResponse.Status.ErrorMsg == \"\"");
        }

        //[Fact, Trait("", "Negative Case")]
        public void GetProfilesInvalidRealPageId()
        {
            // Set up Payload
            payload = reusable.DoPutProfilesPayload();
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            //Set up the API URL
            EndPointUrl = HostUrl + Properties["Profiles"] + "invalidRealPageId";

            //Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: payload);

            // Extract API's JSON Response
            
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
            Assert.NotNull(ResponseString);
            Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
        }

        //[Fact, Trait("", "Happy Path")]

        //[Theory]
        //[Trait("Data-Driven", "Happy Path")]
        [InlineData("GetProfileDetails")]
        public void GetProfileDetailsHappyPaths(string testCase)
        {
            _accessToken = GetClientToken(Properties["identityClientUrl"], Properties["enterpriseUsername6"], "P@ssw0rd");
            realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(Properties["enterpriseUsername6"])).RealPageId.ToString();            

            // Extract Expected JSON Response
            ProfileDetailTestModel expectedProfileDetail = JsonConvert.DeserializeObject<ProfileDetailTestModel>(reusable.DoGetProfileDetailsPayload(realPageId));
            XunitTestOutPut.WriteLine("Expected JSON Response:\n" + JsonConvert.SerializeObject(expectedProfileDetail));

            // Execute API
            EndPointUrl = HostUrl + Properties["ProfileDetails"];
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response            
            ObjectOutput<ProfileDetailTestModel, IErrorData> profileResponse = JsonConvert.DeserializeObject<ObjectOutput<ProfileDetailTestModel, IErrorData>>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

            if (profileResponse.obj.Avatar != null)
            {
                Assert.NotNull(profileResponse.obj.Avatar);
            }

            //Assert UserLogin
            Assert.NotNull(profileResponse.obj.userLogin);

            Assert.NotNull(profileResponse.obj.userLogin.UserId);
            Assert.True(profileResponse.obj.userLogin.UserId == expectedProfileDetail.userLogin.UserId, "profileResponse.obj.userLogin.UserId == expectedProfileDetail.userLogin.UserId");
            Assert.NotNull(profileResponse.obj.userLogin.PartyId);
            Assert.True(profileResponse.obj.userLogin.PartyId == expectedProfileDetail.userLogin.PartyId, "profileResponse.obj.userLogin.PartyId == expectedProfileDetail.userLogin.PartyId");
            Assert.NotNull(profileResponse.obj.userLogin.RealPageId);
            Assert.True(profileResponse.obj.userLogin.RealPageId == expectedProfileDetail.userLogin.RealPageId, "profileResponse.obj.userLogin.RealPageId == expectedProfileDetail.userLogin.RealPageId");
            Assert.NotNull(profileResponse.obj.userLogin.LoginName);
            Assert.True(profileResponse.obj.userLogin.LoginName == expectedProfileDetail.userLogin.LoginName, "profileResponse.obj.userLogin.LoginName == expectedProfileDetail.userLogin.LoginName");
            if (profileResponse.obj.userLogin.LoginNameType != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.LoginNameType);
                Assert.True(profileResponse.obj.userLogin.LoginNameType == expectedProfileDetail.userLogin.LoginNameType, "profileResponse.obj.userLogin.LoginNameType == expectedProfileDetail.userLogin.LoginNameType");
            }

            Assert.NotNull(profileResponse.obj.userLogin.IsActive);
            Assert.True(profileResponse.obj.userLogin.IsActive == expectedProfileDetail.userLogin.IsActive, "profileResponse.obj.userLogin.IsActive == expectedProfileDetail.userLogin.IsActive");
            Assert.NotNull(profileResponse.obj.userLogin.IsLocked);
            Assert.True(profileResponse.obj.userLogin.IsLocked == expectedProfileDetail.userLogin.IsLocked, "profileResponse.obj.userLogin.IsLocked == expectedProfileDetail.userLogin.IsLocked");
            Assert.NotNull(profileResponse.obj.userLogin.IsPending);
            Assert.True(profileResponse.obj.userLogin.IsPending == expectedProfileDetail.userLogin.IsPending, "profileResponse.obj.userLogin.IsPending == expectedProfileDetail.userLogin.IsPending");
            //Assert.NotNull(profileResponse.obj.userLogin.IsTainted);
            Assert.NotNull(profileResponse.obj.userLogin.IsExpired);
            Assert.True(profileResponse.obj.userLogin.IsExpired == expectedProfileDetail.userLogin.IsExpired, "profileResponse.obj.userLogin.IsExpired == expectedProfileDetail.userLogin.IsExpired");
            Assert.NotNull(profileResponse.obj.userLogin.PasswordModifiedDate);
            Assert.True(profileResponse.obj.userLogin.PasswordModifiedDate == expectedProfileDetail.userLogin.PasswordModifiedDate, "profileResponse.obj.userLogin.PasswordModifiedDate == expectedProfileDetail.userLogin.PasswordModifiedDate");
            Assert.NotNull(profileResponse.obj.userLogin.FromDate);
            Assert.True(profileResponse.obj.userLogin.FromDate == expectedProfileDetail.userLogin.FromDate, "profileResponse.obj.userLogin.FromDate == expectedProfileDetail.userLogin.FromDate");
            if (profileResponse.obj.userLogin.ThruDate != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.ThruDate);
                Assert.True(profileResponse.obj.userLogin.ThruDate == expectedProfileDetail.userLogin.ThruDate, "profileResponse.obj.userLogin.ThruDate == expectedProfileDetail.userLogin.ThruDate");
            }
            if (profileResponse.obj.userLogin.LastLogin != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.LastLogin);            
                Assert.True(profileResponse.obj.userLogin.LastLogin == expectedProfileDetail.userLogin.LastLogin, "profileResponse.obj.userLogin.LastLogin == expectedProfileDetail.userLogin.LastLogin");
            }
            Assert.NotNull(profileResponse.obj.userLogin.IsSuperUser);
            Assert.True(profileResponse.obj.userLogin.IsSuperUser == expectedProfileDetail.userLogin.IsSuperUser, "profileResponse.obj.userLogin.IsSuperUser == expectedProfileDetail.userLogin.IsSuperUser");
            Assert.NotNull(profileResponse.obj.userLogin.Status);
            Assert.True(profileResponse.obj.userLogin.Status == expectedProfileDetail.userLogin.Status, "profileResponse.obj.userLogin.Status == expectedProfileDetail.userLogin.Status");
            if (profileResponse.obj.userLogin.Password != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.Password);
                Assert.True(profileResponse.obj.userLogin.Password == expectedProfileDetail.userLogin.Password, "profileResponse.obj.userLogin.Password == expectedProfileDetail.userLogin.Password");
            }
            if (profileResponse.obj.userLogin.UserRoleType != null)
            {
                Assert.NotNull(profileResponse.obj.userLogin.UserRoleType);
                Assert.True(profileResponse.obj.userLogin.UserRoleType == expectedProfileDetail.userLogin.UserRoleType, "profileResponse.obj.userLogin.UserRoleType == expectedProfileDetail.userLogin.UserRoleType");
            }
            Assert.NotNull(profileResponse.obj.userLogin.Is3rdPartyIDP);
            Assert.True(profileResponse.obj.userLogin.Is3rdPartyIDP == expectedProfileDetail.userLogin.Is3rdPartyIDP, "profileResponse.obj.userLogin.Is3rdPartyIDP == expectedProfileDetail.userLogin.Is3rdPartyIDP");

            //AssertOrganization
            if (profileResponse.obj.organization != null)
            {
                for (int countOrganization = 0; countOrganization < profileResponse.obj.organization.Count; countOrganization++)
                {
                    Assert.NotNull(profileResponse.obj.organization[0].RealPageId);
                    Assert.True(profileResponse.obj.organization[0].RealPageId == expectedProfileDetail.organization[0].RealPageId, 
                        "profileResponse.obj.organization[0].RealPageId == expectedProfileDetail.organization[0].RealPageId");
                    Assert.NotNull(profileResponse.obj.organization[0].PartyId);
                    Assert.True(profileResponse.obj.organization[0].PartyId == expectedProfileDetail.organization[0].PartyId, 
                        "profileResponse.obj.organization[0].PartyId == expectedProfileDetail.organization[0].PartyId");
                    Assert.NotNull(profileResponse.obj.organization[0].BooksMasterId);
                    Assert.True(profileResponse.obj.organization[0].BooksMasterId == expectedProfileDetail.organization[0].BooksMasterId, 
                        "profileResponse.obj.organization[0].BooksMasterId == expectedProfileDetail.organization[0].BooksMasterId");
                    Assert.NotNull(profileResponse.obj.organization[0].Name);
                    Assert.True(profileResponse.obj.organization[0].Name == expectedProfileDetail.organization[0].Name, 
                        "profileResponse.obj.organization[0].Name == expectedProfileDetail.organization[0].Name");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.PartyRelationshipId);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.PartyRelationshipId == expectedProfileDetail.organization[0].partyRelationship.PartyRelationshipId, 
                        "profileResponse.obj.organization[0].partyRelationship.PartyRelationshipId == expectedProfileDetail.organization[0].partyRelationship.PartyRelationshipId");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.PartyIdFrom);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.PartyIdFrom == expectedProfileDetail.organization[0].partyRelationship.PartyIdFrom, 
                        "profileResponse.obj.organization[0].partyRelationship.PartyIdFrom == expectedProfileDetail.organization[0].partyRelationship.PartyIdFrom");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.RealPageIdFrom);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.RealPageIdFrom == expectedProfileDetail.organization[0].partyRelationship.RealPageIdFrom, 
                        "profileResponse.obj.organization[0].partyRelationship.RealPageIdFrom == expectedProfileDetail.organization[0].partyRelationship.RealPageIdFrom");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.PartyIdTo);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.PartyIdTo == expectedProfileDetail.organization[0].partyRelationship.PartyIdTo, 
                        "profileResponse.obj.organization[0].partyRelationship.PartyIdTo == expectedProfileDetail.organization[0].partyRelationship.PartyIdTo");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.RealPageIdTo);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.RealPageIdTo == expectedProfileDetail.organization[0].partyRelationship.RealPageIdTo, 
                        "profileResponse.obj.organization[0].partyRelationship.RealPageIdTo == expectedProfileDetail.organization[0].partyRelationship.RealPageIdTo");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.RoleTypeIdFrom);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.RoleTypeIdFrom == expectedProfileDetail.organization[0].partyRelationship.RoleTypeIdFrom, 
                        "profileResponse.obj.organization[0].partyRelationship.RoleTypeIdFrom == expectedProfileDetail.organization[0].partyRelationship.RoleTypeIdFrom");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.roleTypeFrom.PartyRoleTypeId);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.roleTypeFrom.PartyRoleTypeId == expectedProfileDetail.organization[0].partyRelationship.roleTypeFrom.PartyRoleTypeId,
                        "profileResponse.obj.organization[0].partyRelationship.roleTypeFrom.PartyRoleTypeId == expectedProfileDetail.organization[0].partyRelationship.roleTypeFrom.PartyRoleTypeId");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId == expectedProfileDetail.organization[0].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId,
                        "profileResponse.obj.organization[0].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId == expectedProfileDetail.organization[0].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.roleTypeFrom.Name);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.roleTypeFrom.Name == expectedProfileDetail.organization[0].partyRelationship.roleTypeFrom.Name,
                        "profileResponse.obj.organization[0].partyRelationship.roleTypeFrom.Name == expectedProfileDetail.organization[0].partyRelationship.roleTypeFrom.Name");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.RoleTypeIdTo);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.RoleTypeIdTo == expectedProfileDetail.organization[0].partyRelationship.RoleTypeIdTo,
                        "profileResponse.obj.organization[0].partyRelationship.RoleTypeIdTo == expectedProfileDetail.organization[0].partyRelationship.RoleTypeIdTo");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.roleTypeTo.PartyRoleTypeId);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.roleTypeTo.PartyRoleTypeId == expectedProfileDetail.organization[0].partyRelationship.roleTypeTo.PartyRoleTypeId,
                        "profileResponse.obj.organization[0].partyRelationship.roleTypeTo.PartyRoleTypeId == expectedProfileDetail.organization[0].partyRelationship.roleTypeTo.PartyRoleTypeId");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.roleTypeTo.ParentPartyRoleTypeId);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.roleTypeTo.ParentPartyRoleTypeId == expectedProfileDetail.organization[0].partyRelationship.roleTypeTo.ParentPartyRoleTypeId,
                        "profileResponse.obj.organization[0].partyRelationship.roleTypeTo.ParentPartyRoleTypeId == expectedProfileDetail.organization[0].partyRelationship.roleTypeTo.ParentPartyRoleTypeId");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.roleTypeTo.Name);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.roleTypeTo.Name == expectedProfileDetail.organization[0].partyRelationship.roleTypeTo.Name,
                        "profileResponse.obj.organization[0].partyRelationship.roleTypeTo.Name == expectedProfileDetail.organization[0].partyRelationship.roleTypeTo.Name");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.PartyRelationshipTypeId);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.PartyRelationshipTypeId == expectedProfileDetail.organization[0].partyRelationship.PartyRelationshipTypeId,
                        "profileResponse.obj.organization[0].partyRelationship.PartyRelationshipTypeId == expectedProfileDetail.organization[0].partyRelationship.PartyRelationshipTypeId");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.RelationshipTypeId);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.RelationshipTypeId == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.RelationshipTypeId,
                        "profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.RelationshipTypeId == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.RelationshipTypeId");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom,
                        "profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidTo);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidTo == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidTo,
                        "profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidTo == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.RoleTypeIdValidTo");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.Name);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.Name == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.Name,
                        "profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.Name == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.Name");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.Description);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.Description == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.Description,
                        "profileResponse.obj.organization[0].partyRelationship.partyRelationshipType.Description == expectedProfileDetail.organization[0].partyRelationship.partyRelationshipType.Description");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.FromDate);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.FromDate == expectedProfileDetail.organization[0].partyRelationship.FromDate,
                        "profileResponse.obj.organization[0].partyRelationship.FromDate == expectedProfileDetail.organization[0].partyRelationship.FromDate");
                    Assert.NotNull(profileResponse.obj.organization[0].partyRelationship.ThruDate);
                    Assert.True(profileResponse.obj.organization[0].partyRelationship.ThruDate == expectedProfileDetail.organization[0].partyRelationship.ThruDate,
                        "profileResponse.obj.organization[0].partyRelationship.ThruDate == expectedProfileDetail.organization[0].partyRelationship.ThruDate");
                }
            }

            //AssertContactMechanism            
            for (int countContactMechanism = 0; countContactMechanism < profileResponse.obj.contactMechanism.Count; countContactMechanism++)
            {
                Assert.NotNull(profileResponse.obj.contactMechanism[countContactMechanism].PartyContactMechanismId);
                Assert.True(profileResponse.obj.contactMechanism[countContactMechanism].PartyContactMechanismId == expectedProfileDetail.contactMechanism[countContactMechanism].PartyContactMechanismId,
                "profileResponse.obj.contactMechanism[countContactMechanism].PartyContactMechanismId == expectedProfileDetail.contactMechanism[countContactMechanism].PartyContactMechanismId");
                Assert.NotNull(profileResponse.obj.contactMechanism[countContactMechanism].ContactMechanismId);
                Assert.True(profileResponse.obj.contactMechanism[countContactMechanism].ContactMechanismId == expectedProfileDetail.contactMechanism[countContactMechanism].ContactMechanismId,
                "profileResponse.obj.contactMechanism[countContactMechanism].ContactMechanismId == expectedProfileDetail.contactMechanism[countContactMechanism].ContactMechanismId");
                Assert.NotNull(profileResponse.obj.contactMechanism[countContactMechanism].AddressString);
                Assert.True(profileResponse.obj.contactMechanism[countContactMechanism].AddressString == expectedProfileDetail.contactMechanism[countContactMechanism].AddressString,
                "profileResponse.obj.contactMechanism[countContactMechanism].AddressString == expectedProfileDetail.contactMechanism[countContactMechanism].AddressString");
                Assert.NotNull(profileResponse.obj.contactMechanism[countContactMechanism].AddressType);
                Assert.True(profileResponse.obj.contactMechanism[countContactMechanism].AddressType == expectedProfileDetail.contactMechanism[countContactMechanism].AddressType,
                "profileResponse.obj.contactMechanism[countContactMechanism].AddressType == expectedProfileDetail.contactMechanism[countContactMechanism].AddressType");
                Assert.NotNull(profileResponse.obj.contactMechanism[countContactMechanism].contactMechanismUsageType.ContactMechanismUsageTypeId);
                Assert.True(profileResponse.obj.contactMechanism[countContactMechanism].contactMechanismUsageType.ContactMechanismUsageTypeId == expectedProfileDetail.contactMechanism[countContactMechanism].contactMechanismUsageType.ContactMechanismUsageTypeId,
                "profileResponse.obj.contactMechanism[countContactMechanism].contactMechanismUsageType.ContactMechanismUsageTypeId == expectedProfileDetail.contactMechanism[countContactMechanism].contactMechanismUsageType.ContactMechanismUsageTypeId");
                Assert.NotNull(profileResponse.obj.contactMechanism[countContactMechanism].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
                Assert.True(profileResponse.obj.contactMechanism[countContactMechanism].contactMechanismUsageType.ParentContactMechanismUsageTypeId == expectedProfileDetail.contactMechanism[countContactMechanism].contactMechanismUsageType.ParentContactMechanismUsageTypeId,
                "profileResponse.obj.contactMechanism[countContactMechanism].contactMechanismUsageType.ParentContactMechanismUsageTypeId == expectedProfileDetail.contactMechanism[countContactMechanism].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
                Assert.NotNull(profileResponse.obj.contactMechanism[countContactMechanism].contactMechanismUsageType.Name);
                Assert.True(profileResponse.obj.contactMechanism[countContactMechanism].contactMechanismUsageType.Name == expectedProfileDetail.contactMechanism[countContactMechanism].contactMechanismUsageType.Name,
                "profileResponse.obj.contactMechanism[countContactMechanism].contactMechanismUsageType.Name == expectedProfileDetail.contactMechanism[countContactMechanism].contactMechanismUsageType.Name");
            }

            //AssertSummary Count
            Assert.NotNull(profileResponse.obj.SummaryCount);
            Assert.NotNull(profileResponse.obj.SummaryCount.TotalAssignedProperties);
            Assert.NotNull(profileResponse.obj.SummaryCount.TotalAssignedProducts);
            Assert.NotNull(profileResponse.obj.SummaryCount.TotalAssignedRoles);

            //AssertAssigned Products
            Assert.NotNull(profileResponse.obj.AssignedProducts);

            //AssertTelecommunicationNumber
            Assert.NotNull(profileResponse.obj.TelecommunicationNumber);

            //AssertProfileDetail
            if (profileResponse.obj.Password != null)
            {
                Assert.NotNull(profileResponse.obj.Password);
                Assert.True(profileResponse.obj.Password == expectedProfileDetail.Password, "profileResponse.obj.Password == expectedProfileDetail.Password");
            }
            if (profileResponse.obj.NotificationEmail != null)
            {
                Assert.NotNull(profileResponse.obj.NotificationEmail);
                Assert.True(profileResponse.obj.NotificationEmail == expectedProfileDetail.NotificationEmail, "profileResponse.obj.NotificationEmail == expectedProfileDetail.NotificationEmail");
            }
            //Assert.NotNull(profileResponse.obj.AuthenticationType);
            //Assert.True(profileResponse.obj.AuthenticationType == expectedProfileDetail.AuthenticationType, "profileResponse.obj.AuthenticationType == expectedProfileDetail.AuthenticationType");
            //Assert.NotNull(profileResponse.obj.UserTypeId);
            //Assert.True(profileResponse.obj.UserTypeId == expectedProfileDetail.UserTypeId, "profileResponse.obj.UserTypeId == expectedProfileDetail.UserTypeId");
            Assert.NotNull(profileResponse.obj.PartyId);
            Assert.True(profileResponse.obj.PartyId == expectedProfileDetail.PartyId, "profileResponse.obj.PartyId == expectedProfileDetail.PartyId");
            Assert.NotNull(profileResponse.obj.RealPageId);
            Assert.True(profileResponse.obj.RealPageId == expectedProfileDetail.RealPageId, "profileResponse.obj.RealPageId == expectedProfileDetail.RealPageId");
            Assert.NotNull(profileResponse.obj.FirstName);
            Assert.True(profileResponse.obj.FirstName == expectedProfileDetail.FirstName, "profileResponse.obj.FirstName == expectedProfileDetail.FirstName");
            Assert.NotNull(profileResponse.obj.MiddleName);
            Assert.True(profileResponse.obj.MiddleName == expectedProfileDetail.MiddleName, "profileResponse.obj.MiddleName == expectedProfileDetail.MiddleName");
            Assert.NotNull(profileResponse.obj.LastName);
            Assert.True(profileResponse.obj.LastName == expectedProfileDetail.LastName, "profileResponse.obj.LastName == expectedProfileDetail.LastName");
            if (profileResponse.obj.Suffix != null)
            {
                Assert.NotNull(profileResponse.obj.Suffix);
                Assert.True(profileResponse.obj.Suffix == expectedProfileDetail.Suffix, "profileResponse.obj.Suffix == expectedProfileDetail.Suffix");
            }
            if (profileResponse.obj.Title != null)
            {
                Assert.NotNull(profileResponse.obj.Title);
                Assert.True(profileResponse.obj.Title == expectedProfileDetail.Title, "profileResponse.obj.Title == expectedProfileDetail.Title");
            }
            //Assert.NotNull(profileResponse.obj.PreferredContactMethodId);
            //Assert.True(profileResponse.obj.PreferredContactMethodId == expectedProfileDetail.PreferredContactMethodId, "profileResponse.obj.PreferredContactMethodId == expectedProfileDetail.PreferredContactMethodId");

        }

    }

}
