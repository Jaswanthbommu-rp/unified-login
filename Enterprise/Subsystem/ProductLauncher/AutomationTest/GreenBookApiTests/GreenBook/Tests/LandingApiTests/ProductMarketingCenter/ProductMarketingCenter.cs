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

namespace GreenBook.Tests.LandingApiTests.ProductMarketingCenter
{
	public class ProductMarketingCenter : TestController
	{
		public ProductMarketingCenter(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
		    string[] userData = Properties["ProductMarketingUser"].Split('|');
		    _accessToken = GetClientToken(Properties["identityClientUrl"], userData[0], userData[1]);
            this.XunitTestOutPut = _xUnitTestOutput;

			personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();

			newUsername = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
		}

		private string payload = "", personaId, userPersonaId, rightId, newUsername, expProductProperty, expProductRole;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private Tuple<string, string, string, bool, bool> dataSet;

		// ProductMarketingCenter=/api/products/marketingcenter

		//[Theory]
		//[Trait("Data-Driven", "ProductMarketingPropertiesAdd-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductMarketingCenterPropertiesforAdd(string testCaseId, string userTypeId, string userType)
		{
			// Set up the API URL
			switch (userTypeId)
			{
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("9", userTypeId, newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
					break;
				default:
					dataSet = reusable.createUserWithPropertyAndRole("9", userTypeId, newUsername, HttpVerb.Post);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductProperty = dataSet.Item2;

			EndPointUrl = $"{HostUrl}{Properties["ProductMarketingCenter"]}/properties?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            // Deserialization for Response Object
			ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            // Deserialization for Record Object
            List<ProductProperty> productProperties = JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(resProductProperties.Records));
            // TODO : Deserialization for Additonal Object
            
            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//Assert.NotNull(productProperties);
            
            foreach (ProductProperty productProperty in productProperties)
			{
				Assert.NotNull(productProperty);

				Assert.NotNull(productProperty.ID);
				Assert.NotNull(productProperty.Name);
				Assert.NotNull(productProperty.State);
				Assert.NotNull(productProperty.IsAssigned);

				//Additional Asserts
				if (productProperty.ID == expProductProperty || userTypeId == "402")
				{
					Assert.True(productProperty.IsAssigned);
				}
				else
				{
					Assert.False(productProperty.IsAssigned);
				}
			}
			Assert.NotNull(resProductProperties.RowsPerPage);
			Assert.NotNull(resProductProperties.SkipRows);
			Assert.NotNull(resProductProperties.CurrentPage);
			Assert.NotNull(resProductProperties.TotalPages);
			Assert.NotNull(resProductProperties.TotalRows);
			Assert.Null(resProductProperties.Additional);
			Assert.NotNull(resProductProperties.IsError);
			Assert.NotNull(resProductProperties.ErrorReason);

			// Additional Asserts
			Assert.True(productProperties.Count == resProductProperties.TotalRows);
		}


		//[Theory]
		//[Trait("Data-Driven", "ProductMarketingCenterPropertiesEdit-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TestCaseID", "401", "Regular User with Email")]
		[InlineData("TestCaseID", "404", "Regular User No Email")]
		public void GetProductMarketingCenterPropertiesforEdit(string testCaseId, string userTypeId, string userType)
		{
			switch (userTypeId)
			{
				case "401":
					dataSet = reusable.createUserWithPropertyAndRole("9", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put);
					break;
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("9", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductProperty = dataSet.Item2;

			// Set up the API URL
			EndPointUrl =
				$"{HostUrl}{Properties["ProductMarketingCenter"]}/properties?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			// Deserialization for Response Object
			ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			// Deserialization for Record Object
			List<ProductProperty> productProperties =
				JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(resProductProperties.Records));
			// TODO : Deserialization for Additonal Object

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//Assert.NotNull(productProperties);

			if (productProperties != null)
			{
				foreach (ProductProperty productProperty in productProperties)
				{
					Assert.NotNull(productProperty);

					Assert.NotNull(productProperty.ID);
					Assert.NotNull(productProperty.Name);
					Assert.NotNull(productProperty.State);
					Assert.NotNull(productProperty.IsAssigned);

					//Additional Asserts
					if (productProperty.ID == expProductProperty)
						Assert.True(productProperty.IsAssigned);
					//else
					//	Assert.False(productProperty.IsAssigned);
				}
				Assert.NotNull(resProductProperties.RowsPerPage);
				Assert.NotNull(resProductProperties.SkipRows);
				Assert.NotNull(resProductProperties.CurrentPage);
				Assert.NotNull(resProductProperties.TotalPages);
				Assert.NotNull(resProductProperties.TotalRows);
				Assert.Null(resProductProperties.Additional);
				Assert.NotNull(resProductProperties.IsError);
				Assert.NotNull(resProductProperties.ErrorReason);
			}
		}

	    //[Theory]
	    //[Trait("Data-Driven", "ProductMarketingCenterPropertiesEdit-Negative")]
	    //[InlineData("TestCaseID", "userTypeId", "userType")]
	    [InlineData("TestCaseID", "401", "Regular User with Email")]
	    [InlineData("TestCaseID", "404", "Regular User No Email")]
	    [InlineData("TestCaseID", "402", "System Administrator")]
	    public void GetProductMarketingCenterPropertiesforEditNegative(string testCaseId, string userTypeId, string userType)
		{
			userPersonaId = reusable.createUserWithoutPropertyAndRole("9", userTypeId);

			// Set up the API URL
			EndPointUrl =
	            $"{HostUrl}{Properties["ProductMarketingCenter"]}/properties?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

	        // Execute API
	        XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
	        GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

	        // Extract API's JSON Response
	        XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
	        // Deserialization for Response Object
	        ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
	        // Deserialization for Record Object
	        List<ProductProperty> productProperties =
	            JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(resProductProperties.Records));
	        // TODO : Deserialization for Additonal Object

	        // Assert
	        Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(productProperties);

			foreach (ProductProperty productProperty in productProperties)
	        {
				Assert.NotNull(productProperty);
				Assert.NotNull(productProperty.ID);
				Assert.NotNull(productProperty.Name);
				Assert.NotNull(productProperty.State);
				Assert.NotNull(productProperty.IsAssigned);

				//Additional Asserts
				Assert.False(productProperty.IsAssigned);
			}
			Assert.NotNull(resProductProperties.RowsPerPage);
			Assert.NotNull(resProductProperties.SkipRows);
			Assert.NotNull(resProductProperties.CurrentPage);
			Assert.NotNull(resProductProperties.TotalPages);
			Assert.NotNull(resProductProperties.TotalRows);
			Assert.Null(resProductProperties.Additional);
			Assert.NotNull(resProductProperties.IsError);
			Assert.NotNull(resProductProperties.ErrorReason);
		}

		//[Theory]
		//[Trait("Data-Driven", "ProductMarketingRolesAdd-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductMarketingCenterRolesforAdd(string testCaseId, string userTypeId, string userType)
        {
			// Set up the API URL
			switch (userTypeId)
			{
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("9", userTypeId, newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
					break;
				default:
					dataSet = reusable.createUserWithPropertyAndRole("9", userTypeId, newUsername, HttpVerb.Post);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductRole = dataSet.Item3;

			EndPointUrl = $"{HostUrl}{Properties["ProductMarketingCenter"]}/roles?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            // Deserialization for Response Object
            ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            // Deserialization for Record Object
            List<ProductRole> productRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));
            
            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(productRoles);

			foreach (ProductRole productRole in productRoles)
            {
				Assert.NotNull(productRole);
				Assert.NotNull(productRole.ID);
				Assert.NotNull(productRole.Name);
				Assert.NotNull(productRole.IsAssigned);

				//Additional Asserts
				if (productRole.ID == expProductRole || (userTypeId == "402" && productRole.Name == "Corporate Operations"))
				{
					Assert.True(productRole.IsAssigned);
				}
				else
				{
					Assert.False(productRole.IsAssigned);
				}
			}
			Assert.NotNull(resProductRoles.RowsPerPage);
			Assert.NotNull(resProductRoles.SkipRows);
			Assert.NotNull(resProductRoles.CurrentPage);
			Assert.NotNull(resProductRoles.TotalPages);
			Assert.NotNull(resProductRoles.TotalRows);
			Assert.Null(resProductRoles.Additional);
			Assert.NotNull(resProductRoles.IsError);
			Assert.NotNull(resProductRoles.ErrorReason);

			//Additional Asserts
			Assert.True(productRoles.Count == resProductRoles.TotalRows);
		}

		//[Theory]
		//[Trait("Data-Driven", "ProductOnesiteRolesEdit-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User No Email")]
		[InlineData("TC_003", "401", "Regular User with Email", true)]
		[InlineData("TC_004", "404", "Regular User without Email", true)]
		public void GetProductMarketingCenterRolesforEdit(string testCaseId, string userTypeId, string userType, bool disablePropertyRole = false)
		{
			switch (userTypeId)
			{
				case "401":
					dataSet = reusable.createUserWithPropertyAndRole("9", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put, false, disablePropertyRole);
					break;
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("9", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put, false, disablePropertyRole);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductRole = dataSet.Item3;

			// Set up the API URL
			EndPointUrl = $"{HostUrl}{Properties["ProductMarketingCenter"]}/roles?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			// Deserialization for Response Object
			ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			// Deserialization for Record Object
			List<ProductRole> productRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));
			// TODO : Deserialization for Additonal Object

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			//Assert.NotNull(productRoles);

			if (productRoles != null)
			{
				foreach (ProductRole productRole in productRoles)
				{
					Assert.NotNull(productRole);
					Assert.NotNull(productRole.ID);
					Assert.NotNull(productRole.Name);
					Assert.NotNull(productRole.IsAssigned);

					// Additional Asserts
					if (productRole.ID == expProductRole && disablePropertyRole == false)
						Assert.True(productRole.IsAssigned, "productRole.IsAssigned is false.");
					else
						Assert.False(productRole.IsAssigned, "productRole.IsAssigned is true.");
				}
				Assert.NotNull(resProductRoles.RowsPerPage);
				Assert.NotNull(resProductRoles.SkipRows);
				Assert.NotNull(resProductRoles.CurrentPage);
				Assert.NotNull(resProductRoles.TotalPages);
				Assert.NotNull(resProductRoles.TotalRows);
				Assert.Null(resProductRoles.Additional);
				Assert.NotNull(resProductRoles.IsError);
				Assert.NotNull(resProductRoles.ErrorReason);

				//Additional Asserts
				Assert.True(productRoles.Count == resProductRoles.TotalRows);
			}
		}
		
	    //[Theory]
	    //[Trait("Data-Driven", "ProductMarketingCenterRolesEdit-Negative")]
	    //[InlineData("TestCaseID", "userTypeId", "userType")]
	    [InlineData("TestCaseID", "401", "Regular User with Email")]
	    [InlineData("TestCaseID", "404", "Regular User No Email")]
	    [InlineData("TestCaseID", "402", "System Administrator")]
	    public void GetProductMarketingCenterRolesforEditNegative(string testCaseId, string userTypeId, string userType)
	    {
	        string userPersonaId = reusable.createUserWithoutPropertyAndRole("9", userTypeId);

	        // Set up the API URL
	        EndPointUrl = $"{HostUrl}{Properties["ProductMarketingCenter"]}/roles?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

	        // Execute API
	        XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
	        GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

	        // Extract API's JSON Response
	        XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
	        // Deserialization for Response Object
	        ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
	        // Deserialization for Record Object
	        List<ProductRole> productRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));
	        // TODO : Deserialization for Additonal Object

	        // Assert
	        Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
	        //Assert.NotNull(productRoles);
			
	        foreach (ProductRole productRole in productRoles)
			{
				Assert.NotNull(productRole);
				Assert.NotNull(productRole.ID);
				Assert.NotNull(productRole.Name);
				Assert.NotNull(productRole.IsAssigned);

				// Additional Asserts
				Assert.False(productRole.IsAssigned);
			}
			Assert.NotNull(resProductRoles.RowsPerPage);
			Assert.NotNull(resProductRoles.SkipRows);
			Assert.NotNull(resProductRoles.CurrentPage);
			Assert.NotNull(resProductRoles.TotalPages);
			Assert.NotNull(resProductRoles.TotalRows);
			Assert.Null(resProductRoles.Additional);
			Assert.NotNull(resProductRoles.IsError);
			Assert.NotNull(resProductRoles.ErrorReason);

			//Additional Asserts
			Assert.True(productRoles.Count == resProductRoles.TotalRows);
		}


		//[Theory]
		//[Trait("Data-Driven", "ProductMarketingCenterProperties")]
		//[InlineData("TestCaseID", "editorPersonaId", "userPersonaId", "assignedOnly", "ExpResponse", "ExpResponseRecords")]
		[InlineData("TC_001", "$editorPersonaId", "0", "OK", "Yes")]
		[InlineData("TC_002", "0", "0", "OK", "No")]
		[InlineData("TC_003", "0", "0", "OK", "No")]
		[InlineData("TC_004", "$editorPersonaId", "Invalid", "BadRequest", "No")]
		public void GetProductMarketingCenterPropertiesDataDriven(string testCaseId, string editorPersonaId, string userPersonaId, string expResponse, string expObject)
		{
			XunitTestOutPut.WriteLine("Executing TestCase : " + testCaseId);

			if (editorPersonaId == "$editorPersonaId")
			{
				editorPersonaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();
			}

			EndPointUrl =
				$"{HostUrl}{Properties["ProductMarketingCenter"]}/properties?userPersonaId={userPersonaId}&editorPersonaId={editorPersonaId}";

			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Validation for Query String Parameters
			switch (expResponse)
			{
				case "OK":
				Assert.True(ResponseHttpStatusCode == HttpStatusCode.OK);

				// Deserialization for Response Object
				ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
				// Deserialization for Record Object
				List<ProductProperty> productProperties = JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(resProductProperties.Records));

				switch (expObject)
				{
					case "Yes":
						Assert.True(productProperties != null);
						Assert.True(resProductProperties.TotalRows > 0);
						Assert.True(productProperties.Count == resProductProperties.TotalRows);

						foreach (ProductProperty productProperty in productProperties)
						{
							Assert.NotNull(productProperty);

							Assert.NotNull(productProperty.ID);
							Assert.NotNull(productProperty.Name);
							Assert.NotNull(productProperty.State);
							Assert.NotNull(productProperty.IsAssigned);

							//Additional Asserts
							if (editorPersonaId == personaId )
								Assert.False(productProperty.IsAssigned);
							else
								Assert.True(!productProperty.IsAssigned);
						}
						Assert.NotNull(resProductProperties.RowsPerPage);
						Assert.NotNull(resProductProperties.SkipRows);
						Assert.NotNull(resProductProperties.CurrentPage);
						Assert.NotNull(resProductProperties.TotalPages);
						Assert.NotNull(resProductProperties.TotalRows);
						Assert.Null(resProductProperties.Additional);
						Assert.NotNull(resProductProperties.IsError);
						Assert.NotNull(resProductProperties.ErrorReason);

						// Additional Asserts
						Assert.True(productProperties.Count == resProductProperties.TotalRows);
						break;
					default:
						Assert.True(productProperties == null);
						Assert.True(resProductProperties.TotalRows == 0);
						break;
				}
					break;
				case "BadRequest":
					Assert.True(ResponseHttpStatusCode == HttpStatusCode.BadRequest);
					break;
				case "NotFound":
					Assert.True(ResponseHttpStatusCode == HttpStatusCode.NotFound);
					break;
				case "Forbidden":
					Assert.True(ResponseHttpStatusCode == HttpStatusCode.Forbidden);
					break;
				case "InternalServerError":
					Assert.True(ResponseHttpStatusCode == HttpStatusCode.InternalServerError);
					break;
				default:
					Assert.True("Test Data Issue" == "Please Add the Appropriate expected response and execute");
					break;
			}
        }

        //[Theory]
        //[Trait("Data-Driven", "ProductMarketingCenterRoles")]
        //[InlineData("TestCaseID", "editorPersonaId", "userPersonaId", "ExpResponse", "ExpResponseRecords")]
        [InlineData("TC_001", "$editorPersonaId", "0", "OK", "Yes")]
        [InlineData("TC_002", "0", "0", "OK", "No")]
        [InlineData("TC_003", "Invalid", "$userPersonaId", "BadRequest", "No")]
        [InlineData("TC_004", "$editorPersonaId", "Invalid", "BadRequest", "No")]
        [InlineData("TC_005", "Invalid", "Invalid", "BadRequest", "No")]
        public void GetProductMarketingCenterRolesDataDriven(string testCaseId, string editorPersonaId, string userPersonaId, string expResponse, string expObject)
        {
            XunitTestOutPut.WriteLine("Executing TestCase : " + testCaseId);
			
			if (editorPersonaId == "$editorPersonaId")
			{
				editorPersonaId = personaId;
			}

            EndPointUrl = $"{HostUrl}{Properties["ProductMarketingCenter"]}/roles?userPersonaId={userPersonaId}&editorPersonaId={editorPersonaId}";

			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
            // Validation for Query String Parameters
            if (expResponse == "OK")
            {
                Assert.True(ResponseHttpStatusCode == HttpStatusCode.OK);

                // Deserialization for Response Object
                ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
                // Deserialization for Record Object
                List<ProductRole> productRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));

                if (expObject == "Yes")
                {
					Assert.True(productRoles != null);
					Assert.True(resProductRoles.TotalRows > 0);
					Assert.True(productRoles.Count == resProductRoles.TotalRows);

					foreach (ProductRole productRole in productRoles)
					{
						Assert.NotNull(productRole);

						Assert.NotNull(productRole);
						Assert.NotNull(productRole.ID);
						Assert.NotNull(productRole.Name);
						Assert.NotNull(productRole.IsAssigned);

						//Additional Asserts
						if (editorPersonaId == "$editorPersonaId")
							Assert.True(productRole.IsAssigned);
						else
							Assert.True(!productRole.IsAssigned);
					}
					Assert.NotNull(resProductRoles.RowsPerPage);
					Assert.NotNull(resProductRoles.SkipRows);
					Assert.NotNull(resProductRoles.CurrentPage);
					Assert.NotNull(resProductRoles.TotalPages);
					Assert.NotNull(resProductRoles.TotalRows);
					Assert.Null(resProductRoles.Additional);
					Assert.NotNull(resProductRoles.IsError);
					Assert.NotNull(resProductRoles.ErrorReason);

					// Additional Asserts
					Assert.True(productRoles.Count == resProductRoles.TotalRows);
				}
				else
				{
					Assert.True(productRoles == null);
					Assert.True(resProductRoles.TotalRows == 0);
				}
            }

            else if (expResponse == "BadRequest")
                Assert.True(ResponseHttpStatusCode == HttpStatusCode.BadRequest);
            else if (expResponse == "NotFound")
                Assert.True(ResponseHttpStatusCode == HttpStatusCode.NotFound);
            else if (expResponse == "Forbidden")
                Assert.True(ResponseHttpStatusCode == HttpStatusCode.Forbidden);
            else if (expResponse == "InternalServerError")
                Assert.True(ResponseHttpStatusCode == HttpStatusCode.InternalServerError);
            else
                Assert.True("Test Data Issue" == "Please Add the Appropriate expected response and execute");
        }
    }
}

 



