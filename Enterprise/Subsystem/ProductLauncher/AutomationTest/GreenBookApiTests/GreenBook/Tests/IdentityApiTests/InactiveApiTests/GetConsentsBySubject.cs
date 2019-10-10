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
	public class GetConsentsBySubject : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable consentDetails = new DataTable();
		int countConsent = 0;

		public GetConsentsBySubject(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetConsentsBySubject=/api/IdentityConfig/GetConsentsBySubject

		//[Fact, Trait("", "Happy Path")]
		public void GetGetConsentsBySubject()
		{
			// Set up the API URL
			consentDetails = reusable.DoSelectConsents();
			EndPointUrl = HostIdentityUrl + Properties["GetConsentsBySubject"]
				+ "?subjectCode=" + consentDetails.Rows[0]["SubjectCode"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			IList<Consent> consentResponse = JsonConvert.DeserializeObject<IList<Consent>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			if (consentResponse.Count > 0)
			{
				foreach (Consent consent in consentResponse)
				{
					Assert.NotNull(consent.SubjectCode);
					Assert.True(consent.SubjectCode == consentDetails.Rows[countConsent]["SubjectCode"].ToString(), "consent.SubjectCode == consentDetails.Rows[countConsent][\"SubjectCode\"].ToString()");
					Assert.NotNull(consent.ClientCode);
					Assert.True(consent.ClientCode == consentDetails.Rows[countConsent]["ClientCode"].ToString(), "consent.ClientCode == consentDetails.Rows[countConsent][\"ClientCode\"].ToString()");
					Assert.NotNull(consent.Scopes);
					Assert.True(consent.Scopes == consentDetails.Rows[countConsent]["Scopes"].ToString(), "consent.Scopes == consentDetails.Rows[countConsent][\"Scopes\"].ToString()");

					countConsent++;
				}
				countConsent = 0;
			}
		}
	}
}
