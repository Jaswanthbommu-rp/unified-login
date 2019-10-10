using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using System.Collections.Generic;


namespace GreenBook.Tests.LandingApiTests.ProductOneSiteAccounting
{
    public class ProductOneSiteAccounting : TestController
    {
        public ProductOneSiteAccounting(ITestOutputHelper _xUnitTestOutput)
        {
            reusable = new TestUtilities(this);
            string[] userData = Properties["ProductOnesiteAccountingUser"].Split('|');
            _accessToken = GetClientToken(Properties["identityClientUrl"], userData[0], userData[1]);
            this.XunitTestOutPut = _xUnitTestOutput;
			
			personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();

			newUsername = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
		}
        private string payload = "", personaId, userPersonaId, expProductProperty, expProductRole, newUsername;
        JsonController jsonManager = new JsonController();
        TestUtilities reusable;
        private readonly ITestOutputHelper XunitTestOutPut;
		private Tuple<string, string, string, bool, bool> dataSet;

		// ProductOneSiteAccounting=/api/products/onesiteaccounting

		//[Theory]
		//[Trait("Data-Driven", "ProductOnesiteAccountingPropertiesAdd-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductOneSiteAccountingUserPropertiesforAdd(string testCaseId, string userTypeId, string userType)
		{
			switch (userTypeId)
			{
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("8", userTypeId, newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
					break;
				default:
					dataSet = reusable.createUserWithPropertyAndRole("8", userTypeId, newUsername, HttpVerb.Post);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductProperty = dataSet.Item2;

			// Set up the API URL
			EndPointUrl = $"{HostUrl}{Properties["ProductOneSiteAccounting"]}/user/properties?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            // Deserialization for Response Object
            ListResponse onesiteUserAccountingProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            // Deserialization for Record Object
            List<ProductProperty> productProperties = JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(onesiteUserAccountingProperties.Records));
            // TODO : Deserialization for Additonal Object

            // Assert
            Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
            Assert.NotNull(onesiteUserAccountingProperties.Records);

            foreach (ProductProperty productProperty in productProperties)
            {
                Assert.NotNull(productProperty);

                Assert.NotNull(productProperty.ID);
                Assert.NotNull(productProperty.Name);
                Assert.NotNull(productProperty.Street1);
                Assert.NotNull(productProperty.Street2);
                Assert.NotNull(productProperty.City);
                Assert.NotNull(productProperty.State);
                Assert.NotNull(productProperty.Zip);
                Assert.NotNull(productProperty.IsAssigned);

				//Additional Asserts
				if (productProperty.ID == expProductProperty)
				{
					Assert.True(productProperty.IsAssigned);
				}
				else
				{
					Assert.False(productProperty.IsAssigned);
				}
			}
            Assert.NotNull(onesiteUserAccountingProperties.RowsPerPage);
            Assert.NotNull(onesiteUserAccountingProperties.SkipRows);
            Assert.NotNull(onesiteUserAccountingProperties.CurrentPage);
            Assert.NotNull(onesiteUserAccountingProperties.TotalPages);
            Assert.NotNull(onesiteUserAccountingProperties.TotalRows);
            Assert.NotNull(onesiteUserAccountingProperties.Additional);
			if (userTypeId == "402")
			{
				Assert.True(onesiteUserAccountingProperties.Additional.ToString().Replace(" ", "").Contains("\"allProperties\":true"));
			}
            Assert.NotNull(onesiteUserAccountingProperties.IsError);
            Assert.NotNull(onesiteUserAccountingProperties.ErrorReason);

            // Additional Asserts
            Assert.True(productProperties.Count == onesiteUserAccountingProperties.TotalRows);
        }
		
        //[Theory]
        //[Trait("Data-Driven", "ProductOnesiteAccountingPropertiesEdit-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "401", "Regular User with Email", true)]
		[InlineData("TC_004", "404", "Regular User without Email", true)]
		public void GetProductOneSiteAccountingUserPropertiesforEdit(string testCaseId, string userTypeId,string userType, bool disablePropertyRole = false)
		{
			switch (userTypeId)
			{
				case "401":
					dataSet = reusable.createUserWithPropertyAndRole("8", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put, false, disablePropertyRole);
					break;
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("8", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put, false, disablePropertyRole);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductProperty = dataSet.Item2;

			// Set up the Current API URL
			EndPointUrl =
                $"{HostUrl}{Properties["ProductOneSiteAccounting"]}/user/properties?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            // Deserialization for Response Object
            ListResponse onesiteUserAccountingProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            // Deserialization for Record Object
            List<ProductProperty> productProperties =
                JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(onesiteUserAccountingProperties.Records));

            // Assert
            Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
			Assert.NotNull(onesiteUserAccountingProperties.Records);

			if (userTypeId != "402")
			{
				foreach (ProductProperty productProperty in productProperties)
				{
					Assert.NotNull(productProperty);

					Assert.NotNull(productProperty.ID);
					Assert.NotNull(productProperty.Name);
					Assert.NotNull(productProperty.Street1);
					Assert.NotNull(productProperty.Street2);
					Assert.NotNull(productProperty.City);
					Assert.NotNull(productProperty.State);
					Assert.NotNull(productProperty.Zip);
					Assert.NotNull(productProperty.IsAssigned);

					//Additional Asserts
					if (productProperty.ID == expProductProperty)
						Assert.True(productProperty.IsAssigned);
					else
						Assert.False(productProperty.IsAssigned);
				}
				Assert.NotNull(onesiteUserAccountingProperties.RowsPerPage);
				Assert.NotNull(onesiteUserAccountingProperties.SkipRows);
				Assert.NotNull(onesiteUserAccountingProperties.CurrentPage);
				Assert.NotNull(onesiteUserAccountingProperties.TotalPages);
				Assert.NotNull(onesiteUserAccountingProperties.TotalRows);
				Assert.NotNull(onesiteUserAccountingProperties.Additional);
				Assert.NotNull(onesiteUserAccountingProperties.IsError);
				Assert.NotNull(onesiteUserAccountingProperties.ErrorReason);
			}
		}
		
		//[Theory]
        //[Trait("Data-Driven", "ProductOnesiteAccountingPropertiesEdit-Negative")]
        //[InlineData("TestCaseID", "userTypeId", "userType")]
        [InlineData("TC_001", "401", "Regular User with Email")]
        [InlineData("TC_002", "404", "Regular User No Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductOneSiteAccountingUserPropertiesforEditNegative(string testCaseId, string userTypeId, string userType)
		{
			userPersonaId = reusable.createUserWithoutPropertyAndRole("8", userTypeId);

			// Set up the API URL
			EndPointUrl =
				$"{HostUrl}{Properties["ProductOneSite"]}/user/properties?userPersonaId={userPersonaId}&assignedOnly=false&editorPersonaId={personaId}";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			// Deserialization for Response Object
			ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			// Deserialization for Record Object
			List<ProductProperty> productProperties =
				JsonConvert.DeserializeObject<List<ProductProperty>>(
					JsonConvert.SerializeObject(resProductProperties.Records));

			// Assert
			Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
			Assert.NotNull(productProperties);

			foreach (ProductProperty productProperty in productProperties)
			{
				Assert.NotNull(productProperty);

				Assert.NotNull(productProperty.ID);
				Assert.NotNull(productProperty.Name);
				Assert.NotNull(productProperty.Street1);
				Assert.NotNull(productProperty.Street2);
				Assert.NotNull(productProperty.City);
				Assert.NotNull(productProperty.State);
				Assert.NotNull(productProperty.Zip);

				//Additional Asserts
				Assert.False(productProperty.IsAssigned);
			}
			Assert.NotNull(resProductProperties.RowsPerPage);
			Assert.NotNull(resProductProperties.SkipRows);
			Assert.NotNull(resProductProperties.CurrentPage);
			Assert.NotNull(resProductProperties.TotalPages);
			Assert.NotNull(resProductProperties.TotalRows);
			Assert.NotNull(resProductProperties.Additional);
			Assert.NotNull(resProductProperties.IsError);
			Assert.NotNull(resProductProperties.ErrorReason);
		}

		//[Theory]
		//[Trait("Data-Driven", "ProductOnesiteAccountingRolesAdd-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductOneSiteAccountingUserRolesforAdd(string testCaseId, string userTypeId, string userType)
        {
			// Set up the API URL
			switch (userTypeId)
			{
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("8", userTypeId, newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
					break;
				default:
					dataSet = reusable.createUserWithPropertyAndRole("8", userTypeId, newUsername, HttpVerb.Post);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductRole = dataSet.Item3;

			EndPointUrl = $"{HostUrl}{Properties["ProductOneSiteAccounting"]}/user/roles?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            // Deserialization for Response Object
            ListResponse onesiteUserRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            // Deserialization for Record Object
            List<ProductRole> productRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(onesiteUserRoles.Records));
            // TODO : Deserialization for Additonal Object

            // Assert
            Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
            Assert.NotNull(productRoles);

            foreach (ProductRole productRole in productRoles)
            {
                Assert.NotNull(productRole);
                Assert.NotNull(productRole.ID);
                Assert.NotNull(productRole.Name);
                Assert.NotNull(productRole.IsAssigned);

				//Additional Asserts
				if (productRole.ID == expProductRole || (userTypeId == "402" && productRole.Name == "Administrator"))
				{
					Assert.True(productRole.IsAssigned);
				}
				else
				{
					Assert.False(productRole.IsAssigned);
				}
			}
            Assert.NotNull(onesiteUserRoles.RowsPerPage);
            Assert.NotNull(onesiteUserRoles.SkipRows);
            Assert.NotNull(onesiteUserRoles.CurrentPage);
            Assert.NotNull(onesiteUserRoles.TotalPages);
            Assert.NotNull(onesiteUserRoles.TotalRows);
            Assert.Null(onesiteUserRoles.Additional);
            Assert.NotNull(onesiteUserRoles.IsError);
            Assert.NotNull(onesiteUserRoles.ErrorReason);

            //Additional Asserts
            Assert.True(productRoles.Count == onesiteUserRoles.TotalRows);
        }

		//[Theory]
		//[Trait("Data-Driven", "ProductOneSiteAccountingRolesEdit-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "401", "Regular User with Email", true)]
		[InlineData("TC_004", "404", "Regular User without Email", true)]
		public void GetProductOneSiteAccountingUserRolesforEdit(string testCaseId, string userTypeId, string userType, bool disablePropertyRole = false)
		{
			switch (userTypeId)
			{
				case "401":
					dataSet = reusable.createUserWithPropertyAndRole("8", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put, false, disablePropertyRole);
					break;
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("8", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put, false, disablePropertyRole);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductRole = dataSet.Item3;

			// Set up the Current API URL
			EndPointUrl =
                $"{HostUrl}{Properties["ProductOneSiteAccounting"]}/user/roles?editorPersonaId={personaId}&userPersonaId={userPersonaId}";

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            // Deserialization for Response Object
            ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            // Deserialization for Record Object
            List<ProductRole> productRoles =
                JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(productRoles);
			
            foreach (ProductRole productRole in productRoles)
            {
                Assert.NotNull(productRole);
                Assert.NotNull(productRole.ID);
                Assert.NotNull(productRole.Name);
                Assert.NotNull(productRole.IsAssigned);

				// Additional Asserts
				if (productRole.ID == expProductRole && disablePropertyRole == false)
					Assert.True(productRole.IsAssigned);
				else
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
        //[Trait("Data-Driven", "ProductOneSiteAccountingRolesEdit-Negative")]
        //[InlineData("TestCaseID", "userTypeId", "userType")]
        [InlineData("TestCaseID", "401", "Regular User with Email")]
        [InlineData("TestCaseID", "404", "Regular User No Email")]
        [InlineData("TestCaseID", "402", "System Administrator")]
        public void GetProductOneSiteAccountingUserRolesforEditNegative(string testCaseId, string userTypeId, string userType)
        {
            string userPersonaId = reusable.createUserWithoutPropertyAndRole("8", userTypeId);

            // Set up the Current API URL
            EndPointUrl =
                $"{HostUrl}{Properties["ProductOneSiteAccounting"]}/user/roles?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
            // Deserialization for Response Object
            ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            // Deserialization for Record Object
            List<ProductRole> productRoles =
                JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));

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
        //[Trait("Data-Driven", "ProductOnesiteAccountingProperties")]
		//[InlineData("TestCaseID", "editorPersonaId", "userPersonaId", "ExpResponse", "ExpResponseRecords")]
		[InlineData("TC_001", "$editorPersonaId", "0", "OK", "Yes")]
		[InlineData("TC_002", "0", "0", "OK", "No")]
		[InlineData("TC_003", "$editorPersonaId", "Invalid", "BadRequest", "No")]
		[InlineData("TC_004", "Invalid", "$userPersonaId", "BadRequest", "No")]
		[InlineData("TC_005", "Invalid", "Invalid", "BadRequest", "No")]
		public void GetProductOneSiteAccountingUserPropertiesDataDriven(string testCaseId, string editorPersonaId, string userPersonaId, string expResponse, string expObject)
        {
            XunitTestOutPut.WriteLine("Executing TestCase : " + testCaseId);
			
			if (editorPersonaId == "$editorPersonaId")
			{
				editorPersonaId = personaId;
			}

            EndPointUrl = $"{HostUrl}{Properties["ProductOneSiteAccounting"]}/user/properties?userPersonaId={userPersonaId}&editorPersonaId={editorPersonaId}";

			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
            // Validation for Query String Parameters
            if (expResponse == "OK")
            {
                Assert.True(ResponseHttpStatusCode == HttpStatusCode.OK);

                // Deserialization for Response Object
                ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
                // Deserialization for Record Object
                List<ProductProperty> productProperties = JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(resProductProperties.Records));

                if (expObject == "Yes")
                {
                    Assert.True(productProperties != null);
                    Assert.True(resProductProperties.TotalRows > 0);
                    Assert.True(productProperties.Count == resProductProperties.TotalRows);

                    foreach (ProductProperty productProperty in productProperties)
                    {
                        Assert.NotNull(productProperty);

                        Assert.NotNull(productProperty.ID);
                        Assert.NotNull(productProperty.Name);
                        Assert.NotNull(productProperty.Street1);
                        Assert.NotNull(productProperty.Street2);
                        Assert.NotNull(productProperty.City);
                        Assert.NotNull(productProperty.State);
                        Assert.NotNull(productProperty.Zip);
                        Assert.NotNull(productProperty.IsAssigned);

                        //Additional Asserts
                        if (editorPersonaId == "$editorPersonaId" && userPersonaId == "$userPersonaId")
                            Assert.True(productProperty.IsAssigned);
                        else
                            Assert.True(!productProperty.IsAssigned);
                    }
                    Assert.NotNull(resProductProperties.RowsPerPage);
                    Assert.NotNull(resProductProperties.SkipRows);
                    Assert.NotNull(resProductProperties.CurrentPage);
                    Assert.NotNull(resProductProperties.TotalPages);
                    Assert.NotNull(resProductProperties.TotalRows);
                    Assert.NotNull(resProductProperties.Additional);
                    Assert.NotNull(resProductProperties.IsError);
                    Assert.NotNull(resProductProperties.ErrorReason);

                    // Additional Asserts
                    Assert.True(productProperties.Count == resProductProperties.TotalRows);
                }
                else
                {
                    Assert.True(productProperties == null);
                    Assert.True(resProductProperties.TotalRows == 0);
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
		
        //[Theory]
        //[Trait("Data-Driven", "ProductOnesiteAccountingRoles")]
		//[InlineData("TestCaseID", "editorPersonaId", "userPersonaId", "ExpResponse", "ExpResponseRecords")]
		[InlineData("TC_001", "$editorPersonaId", "0", "OK", "Yes")]
		[InlineData("TC_002", "0", "0", "OK", "No")]
		[InlineData("TC_003", "$editorPersonaId", "Invalid", "BadRequest", "No")]
		[InlineData("TC_004", "Invalid", "$userPersonaId", "BadRequest", "No")]
		[InlineData("TC_005", "Invalid", "Invalid", "BadRequest", "No")]
		public void GetProductOneSiteAccountingUserRolesDataDriven(string testCaseId, string editorPersonaId, string userPersonaId, string expResponse, string expObject)
        {
            XunitTestOutPut.WriteLine("Executing TestCase : " + testCaseId);
			
			if (editorPersonaId == "$editorPersonaId")
			{
				editorPersonaId = personaId;
			}

            EndPointUrl = $"{HostUrl}{Properties["ProductOneSiteAccounting"]}/user/roles?userPersonaId={userPersonaId}&editorPersonaId={editorPersonaId}";

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
						Assert.NotNull(productRole.Description);
						Assert.NotNull(productRole.IsAssigned);

						//Additional Asserts
						if (editorPersonaId == "$editorPersonaId" && userPersonaId == "$userPersonaId")
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


