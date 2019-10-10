using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Data;

namespace GreenBook.Tests
{
	public class ContactMechanism : TestController
	{
		public ContactMechanism(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
            //realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString();
            realPageId = reusable.GetRealPageId(CurrentlyLoggedInUser);
        }
		
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string realPageId = "";
		private DataTable expectedContactMechanism = new DataTable();

		// ContactMechanism=/api/persons/{realPageId}/contactmechanism

		[Fact, Trait("", "Happy Path")]
		public void GetContactMechanism()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ContactMechanism"].Replace("{realPageId}", realPageId);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectListOutput<CommonAddress, IErrorData> contactMechanism
				= JsonConvert.DeserializeObject<ObjectListOutput<CommonAddress, IErrorData>>(ResponseString);

			// Extract Expected JSON Response
			expectedContactMechanism = reusable.DoListContactMechanismsForPerson(realPageId);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			for (int countContactMechanism = 0; countContactMechanism < contactMechanism.list.Count; countContactMechanism++)
			{
				Assert.NotNull(contactMechanism.list[countContactMechanism].PartyContactMechanismId);
				Assert.True(contactMechanism.list[countContactMechanism].PartyContactMechanismId
					== int.Parse(expectedContactMechanism.Rows[countContactMechanism]["PartyContactMechanismId"].ToString())
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].PartyContactMechanismId" +
					"== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"PartyContactMechanismId\"].ToString())");
				Assert.NotNull(contactMechanism.list[countContactMechanism].ContactMechanismId);
				Assert.True(contactMechanism.list[countContactMechanism].ContactMechanismId
					== (expectedContactMechanism.Rows[countContactMechanism]["ContactMechanismId"].ToString().Length <= 0?
					0 :	int.Parse(expectedContactMechanism.Rows[countContactMechanism]["ContactMechanismId"].ToString()))
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismId" +
					"== (expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ContactMechanismId\"].ToString().Length <= 0 ?" +
					"0 : int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ContactMechanismId\"].ToString()))");
				Assert.NotNull(contactMechanism.list[countContactMechanism].AddressString);
				Assert.True(contactMechanism.list[countContactMechanism].AddressString
					== expectedContactMechanism.Rows[countContactMechanism]["AddressString"].ToString()
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].AddressString" +
					"== expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"AddressString\"].ToString()");
				Assert.NotNull(contactMechanism.list[countContactMechanism].AddressType);
				Assert.True(contactMechanism.list[countContactMechanism].AddressType
					== expectedContactMechanism.Rows[countContactMechanism]["AddressType"].ToString()
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].AddressType" +
					"== expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"AddressType\"].ToString()");
				Assert.NotNull(contactMechanism.list[countContactMechanism].contactMechanismUsageType.ContactMechanismUsageTypeId);
				Assert.True(contactMechanism.list[countContactMechanism].contactMechanismUsageType.ContactMechanismUsageTypeId
					== int.Parse(expectedContactMechanism.Rows[countContactMechanism]["ContactMechanismUsageTypeId"].ToString())
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismUsageTypeId" +
					"== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanism][\"ContactMechanismUsageTypeId\"].ToString())");
				Assert.NotNull(contactMechanism.list[countContactMechanism].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
				Assert.True(contactMechanism.list[countContactMechanism].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					== int.Parse(expectedContactMechanism.Rows[countContactMechanism]["ParentContactMechanismUsageTypeId"].ToString())
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ParentContactMechanismUsageTypeId" +
					"== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanism][\"ParentContactMechanismUsageTypeId\"].ToString())");
				Assert.NotNull(contactMechanism.list[countContactMechanism].contactMechanismUsageType.Name);
				Assert.True(contactMechanism.list[countContactMechanism].contactMechanismUsageType.Name
					== expectedContactMechanism.Rows[countContactMechanism]["Name"].ToString()
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].Name" +
					"== expectedContactMechanismUsageType.Rows[countContactMechanism][\"Name\"].ToString()");
			}
		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetContactMechanismWithContactMechanismUsageTypeName()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ContactMechanismUsageTypes"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			ObjectListOutput<ContactMechanismUsageType, IErrorData> ContactMechanismUsageTypes
				= JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(ResponseString);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ContactMechanism"].Replace("{realPageId}", realPageId)
				+ "?ContactMechanismUsageTypeName=" + System.Uri.EscapeDataString(ContactMechanismUsageTypes.list[0].Name);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectListOutput<CommonAddress, IErrorData> contactMechanism
				= JsonConvert.DeserializeObject<ObjectListOutput<CommonAddress, IErrorData>>(ResponseString);

			// Extract Expected JSON Response
			expectedContactMechanism = reusable.DoListContactMechanismsForPerson(realPageId);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			for (int countContactMechanism = 0; countContactMechanism < 1; countContactMechanism++)
			{
				Assert.NotNull(contactMechanism.list[countContactMechanism].PartyContactMechanismId);
				Assert.True(contactMechanism.list[countContactMechanism].PartyContactMechanismId
					== int.Parse(expectedContactMechanism.Rows[countContactMechanism]["PartyContactMechanismId"].ToString())
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].PartyContactMechanismId" +
					"== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"PartyContactMechanismId\"].ToString())");
				Assert.NotNull(contactMechanism.list[countContactMechanism].ContactMechanismId);
				Assert.True(contactMechanism.list[countContactMechanism].ContactMechanismId
					== (expectedContactMechanism.Rows[countContactMechanism]["ContactMechanismId"].ToString().Length <= 0 ?
					0 : int.Parse(expectedContactMechanism.Rows[countContactMechanism]["ContactMechanismId"].ToString()))
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismId" +
					"== (expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ContactMechanismId\"].ToString().Length <= 0 ?" +
					"0 : int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ContactMechanismId\"].ToString()))");
				Assert.NotNull(contactMechanism.list[countContactMechanism].AddressString);
				Assert.True(contactMechanism.list[countContactMechanism].AddressString
					== expectedContactMechanism.Rows[countContactMechanism]["AddressString"].ToString()
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].AddressString" +
					"== expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"AddressString\"].ToString()");
				Assert.NotNull(contactMechanism.list[countContactMechanism].AddressType);
				Assert.True(contactMechanism.list[countContactMechanism].AddressType
					== expectedContactMechanism.Rows[countContactMechanism]["AddressType"].ToString()
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].AddressType" +
					"== expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"AddressType\"].ToString()");
				//Assert.NotNull(contactMechanism.list[countContactMechanism].contactMechanismUsageType.ContactMechanismUsageTypeId);
				//Assert.True(contactMechanism.list[countContactMechanism].contactMechanismUsageType.ContactMechanismUsageTypeId
				//	== int.Parse(expectedContactMechanism.Rows[countContactMechanism]["ContactMechanismUsageTypeId"].ToString())
				//	, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismUsageTypeId" +
				//	"== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanism][\"ContactMechanismUsageTypeId\"].ToString())");
				//Assert.NotNull(contactMechanism.list[countContactMechanism].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
				//Assert.True(contactMechanism.list[countContactMechanism].contactMechanismUsageType.ParentContactMechanismUsageTypeId
				//	== int.Parse(expectedContactMechanism.Rows[countContactMechanism]["ParentContactMechanismUsageTypeId"].ToString())
				//	, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ParentContactMechanismUsageTypeId" +
				//	"== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanism][\"ParentContactMechanismUsageTypeId\"].ToString())");
				//Assert.NotNull(contactMechanism.list[countContactMechanism].contactMechanismUsageType.Name);
				//Assert.True(contactMechanism.list[countContactMechanism].contactMechanismUsageType.Name
				//	== expectedContactMechanism.Rows[countContactMechanism]["Name"].ToString()
				//	, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].Name" +
				//	"== expectedContactMechanismUsageType.Rows[countContactMechanism][\"Name\"].ToString()");
			}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetContactMechanismInvalidRealPageId()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ContactMechanism"].Replace("{realPageId}", "invalidRealPageId");

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
	}
}
