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

namespace GreenBook.Tests
{
    public class ElectronicAddress : TestController
	{

        public ElectronicAddress(ITestOutputHelper _xUnitTestOutput)
        {
            reusable = new TestUtilities(this);
            this.XunitTestOutPut = _xUnitTestOutput;
            realPageId = reusable.GetRealPageId(CurrentlyLoggedInUser);
        }

        /*
		public ElectronicAddress(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			dbManager = new DatabaseController(DbConnString);

			EndPointUrl = HostUrl + Properties["RoleType"] + WebUtility.UrlEncode("User Role");
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			roleType = JsonConvert.DeserializeObject<ObjectListOutput<
				RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData>>(ResponseString);

			electronicAddressUser = Guid.NewGuid().ToString().Remove(7);
			payload = reusable.DoPostNewUserPayload(electronicAddressUser, "AutoFirstName", "AutoLastName", null, "regular user (no email)");
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + HostUrl + Properties["User"] + "?userType=" + roleType.list[2].PartyRoleTypeId
				+ " to get the NewUser ActivityToken.\n\nPayload:\n" + payload + "\n");

			EndPointUrl = HostUrl + Properties["User"];
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
			CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(ResponseString);

			_accessToken = GetClientToken(Properties["identityClientUrl"], electronicAddressUser, "P@ssw0rd");

			Thread.Sleep(3000);
			EndPointUrl = HostUrl + Properties["ProfileDetails"];
			XunitTestOutPut.WriteLine("\nCalling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			var profileDetailsResponse = JsonConvert.DeserializeObject<ObjectOutput<ProfileDetailTestModel, ErrorData>>(ResponseString);

			EndPointUrl = HostUrl + Properties["User"] + "/Validate-token?enterpriseUserName=" + WebUtility.UrlEncode(electronicAddressUser)
					+ "&verificationToken=" + profileDetailsResponse.obj.VerificationActivityToken;
			XunitTestOutPut.WriteLine("\nCalling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            //realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(electronicAddressUser)).RealPageId.ToString();
            realPageId = reusable.GetRealPageId(electronicAddressUser);
		}
*/
        private string payload = "";
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string realPageId = "", electronicAddressUser;
		DatabaseController dbManager;
		private ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData> roleType;

		// ElectronicAddress=/api/persons/{realPageId}/electronicaddress

		[Fact, Trait("", "Happy Path")]
		public void GetElectronicAddress()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", realPageId);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress, IErrorData> electronicAddress
				= JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress, IErrorData>>(ResponseString);

			if (electronicAddress.list.Count <= 0)
			{
				// Set up Payload
				payload = reusable.DoPostPutElectronicAddressPayload(HttpVerb.Post);
				XunitTestOutPut.WriteLine("Payload:\n" + payload);

				//Execute POST API
				EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", realPageId);
				XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

				// Reexecute GET API
				XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

				// Reextract API's JSON Response
				electronicAddress
				= JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress, IErrorData>>(ResponseString);
			}
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Extract Expected JSON Response
			EndPointUrl = HostUrl + Properties["ContactMechanism"].Replace("{realPageId}", realPageId);

			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			ObjectListOutput<CommonAddress, IErrorData> expectedElectronicAddressDetails
				= JsonConvert.DeserializeObject<ObjectListOutput<CommonAddress, IErrorData>>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			for (int countElectronicAddress = 0; countElectronicAddress < electronicAddress.list.Count; countElectronicAddress++)
			{
				if (electronicAddress.list[countElectronicAddress].PartyContactMechanismId
					== expectedElectronicAddressDetails.list[countElectronicAddress].PartyContactMechanismId)
				{
					Assert.NotNull(electronicAddress.list[countElectronicAddress].PartyContactMechanismId);
					Assert.True(electronicAddress.list[countElectronicAddress].PartyContactMechanismId
						== expectedElectronicAddressDetails.list[countElectronicAddress].PartyContactMechanismId
						, "electronicAddress.list[countElectronicAddress].PartyContactMechanismId	== expectedElectronicAddressDetails.list[countElectronicAddress].PartyContactMechanismId");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].ContactMechanismId);
					Assert.True(electronicAddress.list[countElectronicAddress].ContactMechanismId
						== expectedElectronicAddressDetails.list[countElectronicAddress].ContactMechanismId
						, "electronicAddress.list[countElectronicAddress].ContactMechanismId == expectedElectronicAddressDetails.list[countElectronicAddress].ContactMechanismId");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].AddressString);
					Assert.True(electronicAddress.list[countElectronicAddress].AddressString
						== expectedElectronicAddressDetails.list[countElectronicAddress].AddressString
						, "electronicAddress.list[countElectronicAddress].AddressString == expectedElectronicAddressDetails.list[countElectronicAddress].AddressString");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].AddressType);
					Assert.True(electronicAddress.list[countElectronicAddress].AddressType
						== expectedElectronicAddressDetails.list[countElectronicAddress].AddressType
						, "electronicAddress.list[countElectronicAddress].AddressType == expectedElectronicAddressDetails.list[countElectronicAddress].AddressType");

					Assert.NotNull(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ContactMechanismUsageTypeId);
					Assert.True(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ContactMechanismUsageTypeId
						== expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.ContactMechanismUsageTypeId
						, "electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ContactMechanismUsageTypeId	== expectedElectronicAddressDetails.list[countElectronicAddress].ContactMechanismUsageTypeId");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
					Assert.True(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId
						== expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId
						, "electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId == expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.Name);
					Assert.True(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.Name
						== expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.Name
						, "electronicAddress.list[countElectronicAddress].contactMechanismUsageType.Name == expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.Name");
				}
			}
		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetElectronicAddressWithContactMechanismUsageTypeName()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", realPageId) + "?ContactMechanismUsageTypeName=" + Uri.EscapeDataString("Email Notification");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress, IErrorData> electronicAddress
				= JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress, IErrorData>>(ResponseString);

			if (electronicAddress.list.Count <= 0)
			{
				// Set up Payload
				payload = reusable.DoPostPutElectronicAddressPayload(HttpVerb.Post);
				XunitTestOutPut.WriteLine("Payload:\n" + payload);

				//Execute POST API
				EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", realPageId) + "?ContactMechanismUsageTypeName=" + Uri.EscapeDataString("Email Notification");
				XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

				// Reexecute GET API
				XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

				// Reextract API's JSON Response
				electronicAddress
				= JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress, IErrorData>>(ResponseString);
			}
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Extract Expected JSON Response
			EndPointUrl = HostUrl + Properties["ContactMechanism"].Replace("{realPageId}", realPageId);

			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			ObjectListOutput<CommonAddress, IErrorData> expectedElectronicAddressDetails
				= JsonConvert.DeserializeObject<ObjectListOutput<CommonAddress, IErrorData>>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			for (int countElectronicAddress = 0; countElectronicAddress < electronicAddress.list.Count; countElectronicAddress++)
			{
				if (electronicAddress.list[countElectronicAddress].PartyContactMechanismId
					== expectedElectronicAddressDetails.list[countElectronicAddress].PartyContactMechanismId)
				{
					Assert.NotNull(electronicAddress.list[countElectronicAddress].PartyContactMechanismId);
					Assert.True(electronicAddress.list[countElectronicAddress].PartyContactMechanismId
						== expectedElectronicAddressDetails.list[countElectronicAddress].PartyContactMechanismId
						, "electronicAddress.list[countElectronicAddress].PartyContactMechanismId	== expectedElectronicAddressDetails.list[countElectronicAddress].PartyContactMechanismId");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].ContactMechanismId);
					Assert.True(electronicAddress.list[countElectronicAddress].ContactMechanismId
						== expectedElectronicAddressDetails.list[countElectronicAddress].ContactMechanismId
						, "electronicAddress.list[countElectronicAddress].ContactMechanismId == expectedElectronicAddressDetails.list[countElectronicAddress].ContactMechanismId");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].AddressString);
					Assert.True(electronicAddress.list[countElectronicAddress].AddressString
						== expectedElectronicAddressDetails.list[countElectronicAddress].AddressString
						, "electronicAddress.list[countElectronicAddress].AddressString == expectedElectronicAddressDetails.list[countElectronicAddress].AddressString");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].AddressType);
					Assert.True(electronicAddress.list[countElectronicAddress].AddressType
						== expectedElectronicAddressDetails.list[countElectronicAddress].AddressType
						, "electronicAddress.list[countElectronicAddress].AddressType == expectedElectronicAddressDetails.list[countElectronicAddress].AddressType");

					Assert.NotNull(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ContactMechanismUsageTypeId);
					Assert.True(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ContactMechanismUsageTypeId
						== expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.ContactMechanismUsageTypeId
						, "electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ContactMechanismUsageTypeId	== expectedElectronicAddressDetails.list[countElectronicAddress].ContactMechanismUsageTypeId");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
					Assert.True(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId
						== expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId
						, "electronicAddress.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId == expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
					Assert.NotNull(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.Name);
					Assert.True(electronicAddress.list[countElectronicAddress].contactMechanismUsageType.Name
						== expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.Name
						, "electronicAddress.list[countElectronicAddress].contactMechanismUsageType.Name == expectedElectronicAddressDetails.list[countElectronicAddress].contactMechanismUsageType.Name");
				}
			}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetElectronicAddressInvalidRealPageId()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", "invalidRealPageId");

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
		public void PostElectronicAddress()
		{
			// Set up Payload
			payload = reusable.DoPostPutElectronicAddressPayload(HttpVerb.Post);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			//realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(electronicAddressUser)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", realPageId);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress.ElectronicAddressOutputResult 
            electronicAddressOutput = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress.ElectronicAddressOutputResult>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(electronicAddressOutput.ContactMechanismId);
			Assert.True(electronicAddressOutput.ContactMechanismId > 1, "electronicAddressOutput.ContactMechanismId > 1");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostElectronicAddressInvalidRealPageId()
		{
			// Set up Payload
			payload = reusable.DoPostPutElectronicAddressPayload(HttpVerb.Post);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(electronicAddressUser)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", "invalidRealPageId");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
		}

		[Fact, Trait("", "Happy Path")]
		public void PutElectronicAddress()
		{
			// Set up Payload
			payload = reusable.DoPostPutElectronicAddressPayload();
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			//realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(electronicAddressUser)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", realPageId);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress.ElectronicAddressOutputResult
				electronicAddressOutput = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.ElectronicAddress.ElectronicAddressOutputResult>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(electronicAddressOutput.ContactMechanismId);
			Assert.True(electronicAddressOutput.ContactMechanismId 
				== JsonConvert.DeserializeObject<LinkElectronicAddress>(payload).PartyContactMechanism.ContactMechanismId
				, "electronicAddressOutput.ContactMechanismId == JsonConvert.DeserializeObject<LinkElectronicAddress>(payload).PartyContactMechanism.ContactMechanismId");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PutElectronicAddressInvalidRealPageId()
		{
			// Set up Payload
			payload = reusable.DoPostPutElectronicAddressPayload();
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(electronicAddressUser)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", "invalidRealPageId");

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
		}
	}
}
