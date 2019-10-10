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
	public class IdentityProviderSettings : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable identityProviderSettingTypes = new DataTable();
		string payload;

		public IdentityProviderSettings(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// IdentityProviderSettings=/api/identityprovider/settings

		//[Fact, Trait("", "Happy Path")]
		public void PostIdentityProviderSettings()
		{
			// Set up Payload
			payload = reusable.DoPostIdentityProviderSettingsPayload();
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			identityProviderSettingTypes = reusable.DoSelectTokens();
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderSettings"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			IdentityProviderSetting identityProviderSettingResponse = JsonConvert.DeserializeObject<IdentityProviderSetting>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.NoContent == ResponseHttpStatusCode, "HttpStatusCode.NoContent == ResponseHttpStatusCode");
		}
	}
}
