using System.Net;
using Xunit;
using Xunit.Abstractions;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using System.Data;

namespace GreenBook.Tests
{
	public class InsertIdentityToken : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable tokenDetails = new DataTable();
		string payload;

		public InsertIdentityToken(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// DeleteIdentityTokenByKey=/api/IdentityConfig/DeleteIdentityTokenByKey

		//[Fact, Trait("", "Happy Path")]
		public void PostInsertIdentityToken()
		{
			// Set up Payload
			payload = reusable.DoPostInsertIdentityTokenPayload();
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["InsertIdentityToken"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.NoContent == ResponseHttpStatusCode, "HttpStatusCode.NoContent == ResponseHttpStatusCode");
		}
	}
}
