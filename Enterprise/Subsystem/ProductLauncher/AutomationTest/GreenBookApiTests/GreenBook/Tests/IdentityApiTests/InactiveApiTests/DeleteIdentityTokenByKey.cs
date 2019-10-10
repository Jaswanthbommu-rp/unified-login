using System.Net;
using Xunit;
using Xunit.Abstractions;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using System.Data;

namespace GreenBook.Tests
{
	public class DeleteIdentityTokenByKey : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable tokenDetails = new DataTable();

		public DeleteIdentityTokenByKey(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// DeleteIdentityTokenByKey=/api/IdentityConfig/DeleteIdentityTokenByKey

		//[Fact, Trait("", "Happy Path")]
		public void GetDeleteIdentityTokenByKey()
		{
			// Set up the API URL
			tokenDetails = reusable.DoSelectTokens();
			EndPointUrl = HostIdentityUrl + Properties["DeleteIdentityTokenByKey"]
				+ "?tokenKey=" + tokenDetails.Rows[0]["TokenKey"] + "&tokenType=" + tokenDetails.Rows[0]["TokenType"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.NoContent == ResponseHttpStatusCode, "HttpStatusCode.NoContent == ResponseHttpStatusCode");
		}
	}
}
