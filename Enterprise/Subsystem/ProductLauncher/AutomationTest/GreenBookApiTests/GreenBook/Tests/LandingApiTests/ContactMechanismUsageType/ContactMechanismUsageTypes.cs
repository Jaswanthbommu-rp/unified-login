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
	public class ContactMechanismUsageTypes : TestController
	{
		public ContactMechanismUsageTypes(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		private string payload = "";
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private DataTable expectedContactMechanismUsageType = new DataTable();

		// ContactMechanismUsageTypes = /api/ContactMechanismUsageTypes

		[Fact, Trait("", "Happy Path")]
		public void GetContactMechanismUsageTypes()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ContactMechanismUsageTypes"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			ObjectListOutput<ContactMechanismUsageType, IErrorData> ContactMechanismUsageTypes
				= JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Extract Expected JSON Response
			expectedContactMechanismUsageType = reusable.DoSelectContactMechanismUsageType();

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			for (int countContactMechanismUsageType = 0; countContactMechanismUsageType < ContactMechanismUsageTypes.list.Count; countContactMechanismUsageType++)
			{
				Assert.NotNull(ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismUsageTypeId);
				Assert.True(ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismUsageTypeId
					== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType]["ContactMechanismUsageTypeId"].ToString())
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismUsageTypeId" +
					"== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ContactMechanismUsageTypeId\"].ToString())");
				Assert.NotNull(ContactMechanismUsageTypes.list[countContactMechanismUsageType].ParentContactMechanismUsageTypeId);
				Assert.True(ContactMechanismUsageTypes.list[countContactMechanismUsageType].ParentContactMechanismUsageTypeId
					== (expectedContactMechanismUsageType.Rows[countContactMechanismUsageType]["ParentContactMechanismUsageTypeId"].ToString().Length <= 0?
					0 :	int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType]["ParentContactMechanismUsageTypeId"].ToString()))
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ParentContactMechanismUsageTypeId" +
					"== (expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ParentContactMechanismUsageTypeId\"].ToString().Length <= 0 ?" +
					"0 : int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ParentContactMechanismUsageTypeId\"].ToString()))");
				Assert.NotNull(ContactMechanismUsageTypes.list[countContactMechanismUsageType].Name);
				Assert.True(ContactMechanismUsageTypes.list[countContactMechanismUsageType].Name
					== expectedContactMechanismUsageType.Rows[countContactMechanismUsageType]["Name"].ToString()
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].Name" +
					"== expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"Name\"].ToString()");
			}
		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetContactMechanismUsageTypesWithContactMechanismUsageTypeName()
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
			EndPointUrl = HostUrl + Properties["ContactMechanismUsageTypes"] + "?ContactMechanismUsageTypeName=" + ContactMechanismUsageTypes.list[0].Name;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract Expected JSON Response
			expectedContactMechanismUsageType = reusable.DoSelectContactMechanismUsageType(ContactMechanismUsageTypes.list[0].Name);

			// Extract API's JSON Response
			
			ContactMechanismUsageTypes
				= JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			for (int countContactMechanismUsageType = 0; countContactMechanismUsageType < ContactMechanismUsageTypes.list.Count; countContactMechanismUsageType++)
			{
				Assert.NotNull(ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismUsageTypeId);
				Assert.True(ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismUsageTypeId
					== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType]["ContactMechanismUsageTypeId"].ToString())
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ContactMechanismUsageTypeId" +
					"== int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ContactMechanismUsageTypeId\"].ToString())");
				Assert.NotNull(ContactMechanismUsageTypes.list[countContactMechanismUsageType].ParentContactMechanismUsageTypeId);
				Assert.True(ContactMechanismUsageTypes.list[countContactMechanismUsageType].ParentContactMechanismUsageTypeId
					== (expectedContactMechanismUsageType.Rows[countContactMechanismUsageType]["ParentContactMechanismUsageTypeId"].ToString().Length <= 0 ?
					0 : int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType]["ParentContactMechanismUsageTypeId"].ToString()))
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].ParentContactMechanismUsageTypeId" +
					"== (expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ParentContactMechanismUsageTypeId\"].ToString().Length <= 0 ?" +
					"0 : int.Parse(expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"ParentContactMechanismUsageTypeId\"].ToString()))");
				Assert.NotNull(ContactMechanismUsageTypes.list[countContactMechanismUsageType].Name);
				Assert.True(ContactMechanismUsageTypes.list[countContactMechanismUsageType].Name
					== expectedContactMechanismUsageType.Rows[countContactMechanismUsageType]["Name"].ToString()
					, "ContactMechanismUsageTypes.list[countContactMechanismUsageType].Name" +
					"== expectedContactMechanismUsageType.Rows[countContactMechanismUsageType][\"Name\"].ToString()");
			}
		}		
	}
}
