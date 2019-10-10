using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Data;

namespace GreenBook.Tests
{
    public class TelecommunicationNumber : TestController
    {
		public TelecommunicationNumber(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
			telecommunicationNumberUsername = CurrentlyLoggedInUser;
		}
		private string payload = "";
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string realPageId, telecommunicationNumberUsername;

		// TelecommunicationNumber =/api/Persons/{realPageId}/TelecommunicationNumber

		[Fact, Trait("", "Happy Path")]
		public void GetTelecommunicationNumber()
		{

            // Set up the API URL
            realPageId = reusable.GetRealPageId(telecommunicationNumberUsername);
			EndPointUrl = HostUrl + Properties["TelecommunicationNumber"].Replace("{realPageId}",realPageId);

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
            ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber, IErrorData> telecommunicationNumber 
                = JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber, IErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Extract Expected JSON Response
            RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber expectedcounttelecommunicationNumber 
                = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber>(reusable.DoGetTelecommunicationNumberPayload());
            XunitTestOutPut.WriteLine("Expected JSON Response:\n" + JsonConvert.SerializeObject(expectedcounttelecommunicationNumber));

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            for (int counttelecommunicationNumber = 0; counttelecommunicationNumber < telecommunicationNumber.list.Count; counttelecommunicationNumber++)
            {
                Assert.True(telecommunicationNumber.list[0].PartyContactMechanismId > 0);
                Assert.True(telecommunicationNumber.list[counttelecommunicationNumber].ContactMechanismId > 0);
                Assert.NotNull(telecommunicationNumber.list[counttelecommunicationNumber].CountryCode);
                Assert.NotNull(telecommunicationNumber.list[counttelecommunicationNumber].AreaCode);
                Assert.NotNull(telecommunicationNumber.list[counttelecommunicationNumber].PhoneNumber);
                Assert.NotNull(telecommunicationNumber.list[counttelecommunicationNumber].contactMechanismUsageType);
                Assert.True(telecommunicationNumber.list[counttelecommunicationNumber].contactMechanismUsageType.ContactMechanismUsageTypeId > 0);
                Assert.True(telecommunicationNumber.list[counttelecommunicationNumber].contactMechanismUsageType.ParentContactMechanismUsageTypeId > 0);
                Assert.NotNull(telecommunicationNumber.list[counttelecommunicationNumber].contactMechanismUsageType.Name);
            }
        }

		[Fact, Trait("", "Happy Path")]
		public void PostTelecommunicationNumber()
		{
			// Set up Payload
			payload = reusable.DoPostPutTelecommunicationNumberPayload(telecommunicationNumberUsername, HttpVerb.Post);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            realPageId = reusable.GetRealPageId(telecommunicationNumberUsername);
			EndPointUrl = HostUrl + Properties["TelecommunicationNumber"].Replace("{realPageId}", realPageId);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber.TelecommunicationNumberOutputResult
				telecommunicationNumberOutput = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber.TelecommunicationNumberOutputResult>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(telecommunicationNumberOutput.ContactMechanismId);
			Assert.True(telecommunicationNumberOutput.ContactMechanismId > 1, "electronicAddressOutput.ContactMechanismId > 1");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PostTelecommunicationNumberInvalidRealPageId()
		{
			// Set up Payload
			payload = reusable.DoPostPutTelecommunicationNumberPayload(telecommunicationNumberUsername, HttpVerb.Post);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(telecommunicationNumberUsername)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["TelecommunicationNumber"].Replace("{realPageId}", "invalidRealPageId");

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
		public void PutTelecommunicationNumber()
		{
			// Set up Payload
			payload = reusable.DoPostPutTelecommunicationNumberPayload(telecommunicationNumberUsername);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            realPageId = reusable.GetRealPageId(telecommunicationNumberUsername);
            EndPointUrl = HostUrl + Properties["TelecommunicationNumber"].Replace("{realPageId}", realPageId);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

			// Extract API's JSON Response
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber.TelecommunicationNumberOutputResult
				telecommunicationNumberOutput = JsonConvert.DeserializeObject<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber.TelecommunicationNumberOutputResult>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(telecommunicationNumberOutput.ContactMechanismId);
			Assert.True(telecommunicationNumberOutput.ContactMechanismId
				== JsonConvert.DeserializeObject<LinkTelecommunicationNumber>(payload).PartyContactMechanism.ContactMechanismId
				, "electronicAddressOutput.ContactMechanismId == JsonConvert.DeserializeObject<LinkElectronicAddress>(payload).PartyContactMechanism.ContactMechanismId");
		}

		//[Fact, Trait("", "Negative Case")]
		public void PutTelecommunicationNumberInvalidRealPageId()
		{
			// Set up Payload
			payload = reusable.DoPostPutTelecommunicationNumberPayload(telecommunicationNumberUsername);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(telecommunicationNumberUsername)).RealPageId.ToString();
			EndPointUrl = HostUrl + Properties["TelecommunicationNumber"].Replace("{realPageId}", "invalidRealPageId");

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
