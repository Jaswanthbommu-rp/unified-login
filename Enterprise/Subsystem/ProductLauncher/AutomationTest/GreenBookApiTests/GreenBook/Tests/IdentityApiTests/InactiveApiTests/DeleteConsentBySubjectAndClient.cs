using System.Net;
using Xunit;
using Xunit.Abstractions;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using System.Data;

namespace GreenBook.Tests
{
	public class DeleteConsentBySubjectAndClient : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable tokenDetails = new DataTable();

		public DeleteConsentBySubjectAndClient(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// DeleteConsentBySubjectAndClient=/api/IdentityConfig/DeleteConsentBySubjectAndClient

		//[Fact, Trait("", "Happy Path")]
		public void PostDeleteConsentBySubjectAndClient()
		{
			// Set up the API URL
			tokenDetails = reusable.DoSelectTokens();
			EndPointUrl = HostIdentityUrl + Properties["DeleteConsentBySubjectAndClient"]
				+ "?subjectCode=" + tokenDetails.Rows[0]["SubjectCode"] + "&clientCode=" + tokenDetails.Rows[0]["ClientCode"] + "&tokenType=" + tokenDetails.Rows[0]["TokenType"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.NoContent == ResponseHttpStatusCode, "HttpStatusCode.NoContent == ResponseHttpStatusCode");
		}
	}
}
