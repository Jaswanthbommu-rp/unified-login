using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;

namespace GreenBook.Tests
{
	public class RoleType : TestController
	{
		public RoleType(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
/*
			// Expected Test Result
			GetRoleTypeResponsePath = DataPath + "GetRoleTypeResponse.json";
			GetRoleTypeResponse = jsonManager.LoadJsonAsString(GetRoleTypeResponsePath);
			expectedRoleType = JsonConvert.DeserializeObject<ObjectOutput<List<
				RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType>, ErrorData>>(GetRoleTypeResponse);
*/
    }

		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string GetRoleTypeResponsePath, GetRoleTypeResponse;
		private ObjectOutput<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType>
				, ErrorData> expectedRoleType;

		// RoleType=/api/roletypes?roleTypeName=

		[Fact, Trait("", "Happy Path")]
		public void GetRoleType()
		{
            // Set up the API URL
            EndPointUrl = HostUrl + Properties["RoleType"] + WebUtility.UrlEncode("User Role"); ;
            //EndPointUrl = HostUrl + Properties["RoleType"] + "SuperUser";
            
            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectOutput<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType>
				, ErrorData> roleType = JsonConvert.DeserializeObject<ObjectOutput<List<
					RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType>, ErrorData>>(ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			
            foreach (var roletype in roleType.obj)
            {
                Assert.True(roletype.Name != null);
                Assert.True(roletype.ParentPartyRoleTypeId > 0);
                Assert.True(roletype.PartyRoleTypeId > 0);
            }
        }

		//[Fact, Trait("", "Data-Driven")]
		public void GetRoleTypeWithValidRoleTypeName()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["RoleType"] + "User Role";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectOutput<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType>
				, ErrorData> roleType = JsonConvert.DeserializeObject<ObjectOutput<List<
					RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType>, ErrorData>>(ResponseString);

			int parentPartyRoleTypeId = 0;

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			for (int countRoleTypes = 0; countRoleTypes < roleType.obj.Count; countRoleTypes++)
			{
				parentPartyRoleTypeId = expectedRoleType.obj[countRoleTypes].Name == "User Role" && parentPartyRoleTypeId <= 0 ?
					expectedRoleType.obj[countRoleTypes].ParentPartyRoleTypeId : parentPartyRoleTypeId;
				if (roleType.obj[countRoleTypes].PartyRoleTypeId == expectedRoleType.obj[countRoleTypes].PartyRoleTypeId
					&& roleType.obj[countRoleTypes].ParentPartyRoleTypeId == parentPartyRoleTypeId)
				{
					Assert.NotNull(roleType.obj[countRoleTypes].PartyRoleTypeId);
					Assert.True(roleType.obj[countRoleTypes].PartyRoleTypeId == expectedRoleType.obj[countRoleTypes].PartyRoleTypeId
						, "roleType.obj[countRoleTypes].PartyRoleTypeId != expectedRoleType.obj[countRoleTypes].PartyRoleTypeId");
					Assert.NotNull(roleType.obj[countRoleTypes].ParentPartyRoleTypeId);
					Assert.True(roleType.obj[countRoleTypes].ParentPartyRoleTypeId == expectedRoleType.obj[countRoleTypes].ParentPartyRoleTypeId
						, "roleType.obj[countRoleTypes].ParentPartyRoleTypeId != expectedRoleType.obj[countRoleTypes].ParentPartyRoleTypeId");
					Assert.NotNull(roleType.obj[countRoleTypes].Name);
					Assert.True(roleType.obj[countRoleTypes].Name == expectedRoleType.obj[countRoleTypes].Name
						, "roleType.obj[countRoleTypes].Name != expectedRoleType.obj[countRoleTypes].Name");
				}
			}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetRoleTypeWithInvalidRoleTypeName()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["RoleType"] + "invalidRoleTypeName";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ObjectOutput<List<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType>
				, ErrorData> roleType = JsonConvert.DeserializeObject<ObjectOutput<List<
					RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType>, ErrorData>>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			for (int countRoleTypes = 0; countRoleTypes < roleType.obj.Count; countRoleTypes++)
			{
				if (roleType.obj[countRoleTypes].PartyRoleTypeId == expectedRoleType.obj[countRoleTypes].PartyRoleTypeId)
				{
					Assert.NotNull(roleType.obj[countRoleTypes].PartyRoleTypeId);
					Assert.True(string.IsNullOrEmpty(roleType.obj[countRoleTypes].PartyRoleTypeId.ToString())
						, "!string.IsNullOrEmpty(roleType.obj[countRoleTypes].PartyRoleTypeId.ToString())");
					Assert.NotNull(roleType.obj[countRoleTypes].ParentPartyRoleTypeId);
					Assert.True(string.IsNullOrEmpty(roleType.obj[countRoleTypes].ParentPartyRoleTypeId.ToString())
						, "!string.IsNullOrEmpty(roleType.obj[countRoleTypes].ParentPartyRoleTypeId.ToString())");
					Assert.NotNull(roleType.obj[countRoleTypes].Name);
					Assert.True(string.IsNullOrEmpty(roleType.obj[countRoleTypes].Name), "!string.IsNullOrEmpty(roleType.obj[countRoleTypes].Name)");
				}
			}
		}
	}
}
