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
	public class GetAllScopeSecrets : TestController
	{
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		DataTable scopeSecrets = new DataTable();
		int countScopeSecrets = 0;
		DateTimeOffset? scopeSecretExpiration = null;

		public GetAllScopeSecrets(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;
		}

		// GetAllScopeSecrets=/api/IdentityConfig/GetAllScopeSecrets

		//[Fact, Trait("", "Happy Path")]
		public void GetGetAllScopeSecrets()
		{
			// Set up the API URL
			scopeSecrets = reusable.DoSelectScopeSecrets();
			EndPointUrl = HostIdentityUrl + Properties["GetAllScopeSecrets"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			
			IList<ScopeSecret> scopeSecretResponse = JsonConvert.DeserializeObject<IList<ScopeSecret>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");

			foreach (ScopeSecret scopeSecret in scopeSecretResponse)
			{
				Assert.NotNull(scopeSecret.Id);
				Assert.True(scopeSecret.Id == int.Parse(scopeSecrets.Rows[countScopeSecrets]["ScopeSecretId"].ToString()), "scopeSecret.Id == int.Parse(scopeSecrets.Rows[countScopeSecrets][\"ScopeSecretId\"].ToString())");
				Assert.NotNull(scopeSecret.Description);
				Assert.True(scopeSecret.Description == scopeSecrets.Rows[countScopeSecrets]["Description"].ToString(), "scopeSecret.Description == scopeSecrets.Rows[countScopeSecrets][\"Description\"].ToString()");

				if (scopeSecrets.Rows[countScopeSecrets]["Expiration"].ToString().Length > 0)
				{
					scopeSecretExpiration = Convert.ToDateTime(scopeSecrets.Rows[countScopeSecrets]["Expiration"].ToString());
				}

				Assert.True(scopeSecret.Expiration == scopeSecretExpiration, "scopeSecret.Expiration == scopeSecretExpiration");
				Assert.NotNull(scopeSecret.Type);
				Assert.True(scopeSecret.Type == scopeSecrets.Rows[countScopeSecrets]["Type"].ToString(), "scopeSecret.Type == scopeSecrets.Rows[countScopeSecrets][\"Type\"].ToString()");
				Assert.NotNull(scopeSecret.Value);
				Assert.True(scopeSecret.Value == scopeSecrets.Rows[countScopeSecrets]["Value"].ToString(), "scopeSecret.Value == scopeSecrets.Rows[countScopeSecrets][\"Value\"].ToString()");
				Assert.NotNull(scopeSecret.ScopeId);
				Assert.True(scopeSecret.ScopeId == int.Parse(scopeSecrets.Rows[countScopeSecrets]["ScopeId"].ToString()), "scopeSecret.ScopeId == int.Parse(scopeSecrets.Rows[countScopeSecrets][\"ScopeId\"].ToString())");

				if (scopeSecret.Scope != null)
				{

				}

				countScopeSecrets++;
			}
			countScopeSecrets = 0;
		}
	}
}
