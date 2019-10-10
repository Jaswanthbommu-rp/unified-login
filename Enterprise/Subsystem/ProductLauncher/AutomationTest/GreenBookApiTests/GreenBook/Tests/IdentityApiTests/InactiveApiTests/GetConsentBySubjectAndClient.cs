using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Data;
using System;

namespace GreenBook.Tests
{
	public class GetConsentBySubjectAndClient : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable consentDetails = new DataTable();

		public GetConsentBySubjectAndClient(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetConsentBySubjectAndClient=/api/IdentityConfig/GetConsentBySubjectAndClient

		//[Fact, Trait("", "Happy Path")]
		public void GetGetConsentBySubjectAndClient()
		{
			// Set up the API URL
			consentDetails = reusable.DoSelectConsents();
			EndPointUrl = HostIdentityUrl + Properties["GetConsentBySubjectAndClient"]
				+ "?subjectCode=" + consentDetails.Rows[0]["SubjectCode"] + "&clientCode=" + consentDetails.Rows[0]["ClientCode"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			Consent consentResponse = JsonConvert.DeserializeObject<Consent>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(consentResponse.SubjectCode);
			Assert.True(consentResponse.SubjectCode == consentDetails.Rows[0]["SubjectCode"].ToString(), "consentResponse.SubjectCode == consentDetails.Rows[0][\"SubjectCode\"].ToString()");
			Assert.NotNull(consentResponse.ClientCode);
			Assert.True(consentResponse.ClientCode == consentDetails.Rows[0]["ClientCode"].ToString(), "consentResponse.ClientCode == consentDetails.Rows[0][\"ClientCode\"].ToString()");
			Assert.NotNull(consentResponse.Scopes);
			Assert.True(consentResponse.Scopes == consentDetails.Rows[0]["Scopes"].ToString(), "consentResponse.Scopes == consentDetails.Rows[0][\"Scopes\"].ToString()");
		}
	}
}
