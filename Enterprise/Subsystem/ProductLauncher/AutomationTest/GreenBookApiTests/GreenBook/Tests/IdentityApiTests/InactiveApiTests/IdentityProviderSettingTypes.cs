using System.Net;
using Xunit;
using Xunit.Abstractions;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using System.Data;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using Newtonsoft.Json;

namespace GreenBook.Tests
{
	public class IdentityProviderSettingTypes : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		string payload;

		public IdentityProviderSettingTypes(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// IdentityProviderSettingTypes=/api/identityprovider/settingtypes

		//[Fact, Trait("", "Happy Path")]
		public void PostIdentityProviderSettingTypes()
		{
			// Set up Payload
			payload = reusable.DoPostIdentityProviderSettingTypesPayload();
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderSettingTypes"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			IdentityProviderSettingType identityProviderSettingResponse = JsonConvert.DeserializeObject<IdentityProviderSettingType>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(identityProviderSettingResponse.IdentityProviderSettingTypeId);
			Assert.True(identityProviderSettingResponse.IdentityProviderSettingTypeId > 0, "identityProviderSettingResponse.IdentityProviderSettingTypeId > 0");
		}
	}
}
