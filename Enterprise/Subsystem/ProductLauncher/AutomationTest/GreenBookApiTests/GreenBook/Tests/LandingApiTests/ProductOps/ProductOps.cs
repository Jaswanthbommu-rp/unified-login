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
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;

namespace GreenBook.Tests.LandingApiTests.productOps
{
	public class ProductOps : TestController
	{
		public ProductOps(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			string[] userData = Properties["ProductOpsUser"].Split('|');
			_accessToken = GetClientToken(Properties["identityClientUrl"], userData[0], userData[1]);

			this.XunitTestOutPut = _xUnitTestOutput;

			personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();
			newUsername = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
		}
		private string payload = "", personaId, userPersonaId, newUsername, expProductProperty, expProductRole;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private Tuple<string, string, string, bool, bool> dataSet;

		// ProductOneSite=/api/products/ops

		//[Theory]
		//[Trait("Data-Driven", "ProductOpsAssetsAdd-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		[InlineData("TC_004", "401", "Regular User with Email", false)]
		[InlineData("TC_005", "404", "Regular User without Email", false)]
		[InlineData("TC_006", "402", "System Administrator", false)]
		public void GetProductOpsAssetsforAdd(string testCaseId, string userTypeId, string userType, bool includeDisabled = true)
		{
			// Set up the API URL
			switch (userTypeId)
			{
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("13", userTypeId, newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
					break;
				default:
					dataSet = reusable.createUserWithPropertyAndRole("13", userTypeId, newUsername, HttpVerb.Post);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductProperty = dataSet.Item2;

			EndPointUrl = $"{HostUrl}{Properties["ProductOps"]}/assets?userPersonaId={userPersonaId}&includeDisabled={includeDisabled}&editorPersonaId={personaId}";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			// Deserialization for Response Object
			ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			// Deserialization for Record Object
			List<AssetGroup> productProperties = JsonConvert.DeserializeObject<List<AssetGroup>>(JsonConvert.SerializeObject(resProductProperties.Records));

			// Assert
			Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
			Assert.NotNull(resProductProperties.Records);

			foreach (AssetGroup productProperty in productProperties)
			{
				Assert.NotNull(productProperty);

				Assert.NotNull(productProperty.ID);
				Assert.NotNull(productProperty.Name);
				Assert.NotNull(productProperty.Status);
				Assert.NotNull(productProperty.GroupType);
				Assert.NotNull(productProperty.AssetID);
				Assert.NotNull(productProperty.IsAssigned);

				//Additional Asserts
				if (productProperty.ID == expProductProperty || (productProperty.Name == "[G] CF Real Estate Services" && userTypeId == "402"))
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
			Assert.NotNull(resProductProperties.Additional);
			if (userTypeId == "402")
			{
				Assert.True(resProductProperties.Additional.ToString().Contains("AssetGroups"), "resProductProperties.Additional is not \"AssetGroups\"");
			}
			Assert.NotNull(resProductProperties.IsError);
			Assert.NotNull(resProductProperties.ErrorReason);

			// Additional Asserts
			Assert.True(productProperties.Count == resProductProperties.TotalRows);
		}

		//[Theory]
		//[Trait("Data-Driven", "ProductOpsAssetsEdit-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "401", "Regular User with Email", false)]
		[InlineData("TC_004", "404", "Regular User without Email", false)]
		[InlineData("TC_005", "401", "Regular User with Email", false, true)]
		[InlineData("TC_006", "404", "Regular User without Email", false, true)]
		[InlineData("TC_007", "401", "Regular User with Email", true, true)]
		[InlineData("TC_008", "404", "Regular User without Email", true, true)]
		public void GetProductOpsAssetsforEdit(string testCaseId, string userTypeId, string userType, bool includeDisabled = true, bool disablePropertyRole = false)
		{
			switch (userTypeId)
			{
				case "401":
					dataSet = reusable.createUserWithPropertyAndRole("13", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put, false, disablePropertyRole);
					break;
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("13", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put, false, disablePropertyRole);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductProperty = dataSet.Item2;

			// Set up the API URL
			EndPointUrl = $"{HostUrl}{Properties["ProductOps"]}/assets?userPersonaId={userPersonaId}&includeDisabled={includeDisabled}&editorPersonaId={personaId}";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			// Deserialization for Response Object
			ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			// Deserialization for Record Object
			List<AssetGroup> productProperties = JsonConvert.DeserializeObject<List<AssetGroup>>(JsonConvert.SerializeObject(resProductProperties.Records));

			// Assert
			Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
			Assert.NotNull(productProperties);

			foreach (AssetGroup productProperty in productProperties)
			{
				Assert.NotNull(productProperty);

				Assert.NotNull(productProperty.ID);
				Assert.NotNull(productProperty.Name);
				Assert.NotNull(productProperty.Status);
				switch (includeDisabled)
				{
					case true:
						Assert.True(productProperty.Status == "active" || productProperty.Status == "inactive");
						break;
					default:
						Assert.True(productProperty.Status == "active");
						break;
				}

				Assert.NotNull(productProperty.GroupType);
				Assert.NotNull(productProperty.AssetID);
				Assert.NotNull(productProperty.IsAssigned);

				//Additional Asserts
				if (productProperty.ID == expProductProperty)
					Assert.True(productProperty.IsAssigned);
				else
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
		//[Trait("Data-Driven", "ProductOpsAssetsEdit-Negative")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User No Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductOpsAssetsforEditNegative(string testCaseId, string userTypeId, string userType)
		{
			userPersonaId = reusable.createUserWithoutPropertyAndRole("13", userTypeId);

			// Set up the API URL
			EndPointUrl = $"{HostUrl}{Properties["ProductOps"]}/assets?userPersonaId={userPersonaId}&includeDisabled=true&editorPersonaId={personaId}";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			// Deserialization for Response Object
			ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			// Deserialization for Record Object
			List<AssetGroup> productProperties =
				JsonConvert.DeserializeObject<List<AssetGroup>>(
					JsonConvert.SerializeObject(resProductProperties.Records));

			// Assert
			Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
			Assert.NotNull(productProperties);

			foreach (AssetGroup productProperty in productProperties)
			{
				Assert.NotNull(productProperty);

				Assert.NotNull(productProperty.ID);
				Assert.NotNull(productProperty.Name);
				Assert.NotNull(productProperty.Status);
				Assert.NotNull(productProperty.GroupType);
				Assert.NotNull(productProperty.AssetID);
				Assert.NotNull(productProperty.IsAssigned);

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
		//[Trait("Data-Driven", "ProductOpsRolesAdd-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductOpsRolesforAdd(string testCaseId, string userTypeId, string userType)
		{
			// Set up the API URL
			switch (userTypeId)
			{
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("13", userTypeId, newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
					break;
				default:
					dataSet = reusable.createUserWithPropertyAndRole("13", userTypeId, newUsername, HttpVerb.Post);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductRole = dataSet.Item3;

			EndPointUrl = $"{HostUrl}{Properties["ProductOps"]}/roles?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

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
			Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
			Assert.NotNull(productRoles);

			foreach (ProductRole productRole in productRoles)
			{
				Assert.NotNull(productRole);
				Assert.NotNull(productRole.ID);
				Assert.NotNull(productRole.Name);
				Assert.NotNull(productRole.IsAssigned);
				Assert.NotNull(productRole.Roletype);

				//Additional Asserts
				if (productRole.ID == expProductRole || (productRole.Name == "Marketplace Administrator" && userTypeId == "402"))
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
		//[Trait("Data-Driven", "ProductOpsRolesEdit-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User No Email")]
		[InlineData("TC_003", "401", "Regular User with Email", true)]
		[InlineData("TC_004", "404", "Regular User without Email", true)]
		public void GetProductOpsRolesforEdit(string testCaseId, string userTypeId, string userType, bool disablePropertyRole = false)
		{
			switch (userTypeId)
			{
				case "401":
					dataSet = reusable.createUserWithPropertyAndRole("13", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put, false, disablePropertyRole);
					break;
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("13", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put, false, disablePropertyRole);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductRole = dataSet.Item3;

			// Set up the API URL
			EndPointUrl = $"{HostUrl}{Properties["ProductOps"]}/roles?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

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
			Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
			Assert.NotNull(productRoles);

			foreach (ProductRole productRole in productRoles)
			{
				Assert.NotNull(productRole);
				Assert.NotNull(productRole.ID);
				Assert.NotNull(productRole.Name);
				Assert.NotNull(productRole.IsAssigned);
				Assert.NotNull(productRole.Roletype);

				// Additional Asserts
				if (productRole.ID == expProductRole)
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
		//[Trait("Data-Driven", "ProductOpsRolesEdit-Negative")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User No Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductOpsRolesforEditNegative(string testCaseId, string userTypeId, string userType)
		{
			userPersonaId = reusable.createUserWithoutPropertyAndRole("13", userTypeId);

			// Set up the API URL
			EndPointUrl = $"{HostUrl}{Properties["ProductOps"]}/roles?userPersonaId={userPersonaId}&editorPersonaId={personaId}";

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
				Assert.NotNull(productRole.Roletype);

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
		//[Trait("Data-Driven", "ProductOpsAssets")]
		//[InlineData("TestCaseID", "editorPersonaId", "userPersonaId", "assignedOnly", "ExpResponse", "ExpResponseRecords")]
		[InlineData("TC_001", "$editorPersonaId", "0", "true", "OK", "Yes")]
		[InlineData("TC_002", "$editorPersonaId", "0", "false", "OK", "Yes")]
		[InlineData("TC_003", "0", "0", "true", "OK", "No")]
		[InlineData("TC_004", "0", "0", "false", "OK", "No")]
		[InlineData("TC_005", "Invalid", "$userPersonaId", "true", "BadRequest", "No")]
		[InlineData("TC_006", "$editorPersonaId", "Invalid Type", "false", "BadRequest", "No")]
		public void GetProductOpsAssetsDataDriven(string testCaseId, string editorPersonaId, string userPersonaId, string assignedOnly, string expResponse, string expObject)
		{
			if (editorPersonaId == "$editorPersonaId")
			{
				editorPersonaId = personaId;
			}

			EndPointUrl = $"{HostUrl}{Properties["ProductOps"]}/assets?userPersonaId={userPersonaId}&includeDisabled={assignedOnly}&editorPersonaId={editorPersonaId}";

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
					List<AssetGroup> productProperties = JsonConvert.DeserializeObject<List<AssetGroup>>(JsonConvert.SerializeObject(resProductProperties.Records));

					switch (expObject)
					{
						case "Yes":
							Assert.True(productProperties != null);
							Assert.True(resProductProperties.TotalRows > 0);
							Assert.True(productProperties.Count == resProductProperties.TotalRows);

							foreach (AssetGroup productProperty in productProperties)
							{
								Assert.NotNull(productProperty);

								Assert.NotNull(productProperty.ID);
								Assert.NotNull(productProperty.Name);
								Assert.NotNull(productProperty.Status);
								Assert.NotNull(productProperty.GroupType);
								Assert.NotNull(productProperty.AssetID);
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
							if (userPersonaId == "0")
							{
								Assert.NotNull(resProductProperties.Additional);
								Assert.True(resProductProperties.Additional.ToString().Contains("AssetGroups"));
							}
							else
							{
								Assert.Null(resProductProperties.Additional);
							}
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
		//[Trait("Data-Driven", "ProductOpsRoles")]
		//[InlineData("TestCaseID", "editorPersonaId", "userPersonaId", "assignedOnly", "ExpResponse", "ExpResponseRecords")]
		[InlineData("TC_001", "$editorPersonaId", "0", "true", "OK", "Yes")]
		[InlineData("TC_002", "$editorPersonaId", "0", "false", "OK", "Yes")]
		[InlineData("TC_003", "0", "0", "true", "OK", "Yes")]
		[InlineData("TC_004", "0", "0", "false", "OK", "Yes")]
		[InlineData("TC_005", "Invalid", "$userPersonaId", "true", "BadRequest", "No")]
		[InlineData("TC_006", "$editorPersonaId", "Invalid", "false", "BadRequest", "No")]
		public void GetProductOpsRolesDataDriven(string testCaseId, string editorPersonaId, string userPersonaId, string assignedOnly, string expResponse, string expObject)
		{
			if (editorPersonaId == "$editorPersonaId")
			{
				editorPersonaId = personaId;
			}

			EndPointUrl = $"{HostUrl}{Properties["ProductOps"]}/roles?userPersonaId={userPersonaId}&includeDisabled={assignedOnly}&editorPersonaId={editorPersonaId}";

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
					ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
					// Deserialization for Record Object
					List<ProductRole> productRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));

					switch (expObject)
					{
						case "Yes":
							if (editorPersonaId == "0")
							{
								Assert.Null(productRoles);
							}
							else
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
									Assert.NotNull(productRole.Roletype);

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
							break;
						default:
							Assert.True(productRoles == null);
							Assert.True(resProductRoles.TotalRows == 0);
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
    }
}





