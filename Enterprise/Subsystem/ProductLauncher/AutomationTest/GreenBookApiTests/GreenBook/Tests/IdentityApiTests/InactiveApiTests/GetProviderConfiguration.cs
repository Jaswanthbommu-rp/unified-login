using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Data;
using System;
using System.Collections.Generic;

namespace GreenBook.Tests
{
	public class GetProviderConfiguration : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;

		public GetProviderConfiguration(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetProviderConfiguration=/api/IdentityConfig/GetProviderConfiguration

		/*Disabling this test since the GET GetProviderConfiguration has already been deprecated 
		 and replaced by GET /api/identityprovider/configuration/{providerName}.*/
		//[Fact, Trait("", "Happy Path")]
		public void GetGetProviderConfiguration()
		{
			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["GetProviderConfiguration"] + "?providerEnum=5";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			IList<ProviderConfiguration> providerConfigurationResponse = JsonConvert.DeserializeObject<IList<ProviderConfiguration>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			foreach (ProviderConfiguration providerConfiguration in providerConfigurationResponse)
			{
				Assert.NotNull(providerConfiguration.ProviderPortfolioId);
				Assert.True(providerConfiguration.ProviderPortfolioId > 0);
			}
		}
	}
}
