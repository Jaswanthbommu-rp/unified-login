using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using System.Data;
using GreenBook.Attributes;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using System.Collections.Generic;
using System.Linq;

namespace GreenBook.Tests.LandingApiTests.ProductOneSite
{
	public class ProductLearningPortal : TestController
	{
        public ProductLearningPortal(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
		    userData = Properties["ProductLearningPortalUser"].Split('|');
		    _accessToken = GetClientToken(Properties["identityClientUrl"], userData[0], userData[1]);
            this.XunitTestOutPut = _xUnitTestOutput;
		}
		private string[] userData;
		private string payload = "";
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;

	    [Fact, Trait("", "Happy Path")]
	    public void GetProductsLearningPortal()
	    {
	        // Set up the API URL
	        EndPointUrl = $"{HostUrl}{Properties["ProductLearningPortal"]}";

	        // Execute API
	        XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
	        GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

	        // Extract API's JSON Response
	        XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
	        // Deserialization for Response Object
	        ObjectOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductLearningPortal, ErrorData> resProductProperties = JsonConvert.DeserializeObject<ObjectOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductLearningPortal, ErrorData>>(ResponseString);

	        Assert.NotNull(resProductProperties.obj.Url);
            Assert.NotNull(resProductProperties.Status.Success);
	        Assert.NotNull(resProductProperties.Status.ErrorCode);
	        Assert.NotNull(resProductProperties.Status.ErrorMsg);
        }

		//[Theory]
		//[Trait("Data-Driven", "ProductLearningPortal")]
		//[InlineData("TestCaseID", "userName", "createUser", "returnUrl")]
		[InlineData("TC_001", "$userName", "Yes", "Yes")]
		[InlineData("TC_002", "$userName", "No", "Yes")]
		[InlineData("TC_003", "$userName", "", "Yes")]
		[InlineData("TC_004", "", "Yes", "Yes")]
		[InlineData("TC_005", "", "No", "Yes")]
		[InlineData("TC_006", "", "", "Yes")]
		[InlineData("TC_007", "Invalid", "", "No")]
		[InlineData("TC_008", "Invalid", "Yes", "No")]
		[InlineData("TC_009", "Invalid", "No", "No")]
		//[InlineData("TC_010", "Valid-NewUser", "No", "No")] // TODO: Not applicable for CF RES.
		//[InlineData("TC_011", "Valid-NewUser", "Yes", "Yes")]
		public void GetProductsLearningPortalDatadriven(string testCaseId, string userName, string createUser, string returnUrl)
		{
			if (userName == "$userName")
			{
				userName = userData[0];
			}
			else if (userName == "Valid-NewUser")
			{
				userName = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLogins(JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona(Convert.ToInt64(reusable.createUserWithPropertyAndRole("19", "401", Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com", HttpVerb.Post).Item1))).RealPageId)).LoginName;
			}

			// Set up the API URL
			EndPointUrl = $"{HostUrl}{Properties["ProductLearningPortal"]}?userName={userName}&createUser={createUser}";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			// Deserialization for Response Object
			ObjectOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductLearningPortal, ErrorData> resProductProperties = JsonConvert.DeserializeObject<ObjectOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ProductLearningPortal, ErrorData>>(ResponseString);

			if (ResponseHttpStatusCode == HttpStatusCode.OK)
			{
				Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
				if (createUser.Length > 0)
				{
					if (userName.ToLower() != "invalid")
					{
						Assert.NotNull(resProductProperties.obj.Url);
						Assert.NotNull(resProductProperties.Status.Success);
						Assert.NotNull(resProductProperties.Status.ErrorCode);
						Assert.NotNull(resProductProperties.Status.ErrorMsg);

						if (returnUrl == "Yes")
						{
							Assert.NotEqual(resProductProperties.obj.Url, "null");
							Assert.Equal(resProductProperties.Status.Success, true);
							Assert.Equal(resProductProperties.Status.ErrorCode, "");
							Assert.Equal(resProductProperties.Status.ErrorMsg, "");
						}
						else
						{
							Assert.Equal(resProductProperties.obj.Url, "null");
							Assert.Equal(resProductProperties.Status.Success, false);
							Assert.Equal(resProductProperties.Status.ErrorCode, "");
							Assert.Equal(resProductProperties.Status.ErrorMsg, "");
						}
					}
				}
			}
			else if (ResponseHttpStatusCode == HttpStatusCode.BadRequest)
			{
				Assert.Equal(HttpStatusCode.BadRequest, ResponseHttpStatusCode);
			}
		}
    }
}

 



