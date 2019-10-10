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
using System.Linq;

namespace GreenBook.Tests.LandingApiTests.ProductOneSite
{
	public class ProductOneSite : TestController
	{
		public ProductOneSite(ITestOutputHelper _xUnitTestOutput)
		{
			this.XunitTestOutPut = _xUnitTestOutput;

			reusable = new TestUtilities(this);

			string[] userData = Properties["ProductOnesiteUser"].Split('|');
			_accessToken = GetClientToken(Properties["identityClientUrl"], userData[0], userData[1]);
			personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();

			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/rights?editorPersonaId=" + personaId;
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
			rightId = JsonConvert.DeserializeObject<List<ProductRight>>(JsonConvert.SerializeObject(JsonConvert.DeserializeObject<ListResponse>(ResponseString).Records)).First().ID.ToString();

			newUsername = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
		}
		private string payload = "", personaId, userPersonaId, rightId, newUsername, expProductProperty, expProductRole;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private Tuple<string, string, string, bool, bool> dataSet;

		// ProductOneSite=/api/products/onesite

		//[Theory]
		//[Trait("Data-Driven", "ProductOnesitePropertiesAdd-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		[InlineData("TC_004", "401", "Regular User with Email", false)]
		[InlineData("TC_005", "404", "Regular User without Email", false)]
		[InlineData("TC_006", "402", "System Administrator", false)]
		public void GetProductOneSiteUserPropertiesforAdd(string testCaseId, string userTypeId, string userType, bool assignedOnly = true)
		{
			switch (userTypeId)
			{
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId, newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
					break;
				default:
					dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId, newUsername, HttpVerb.Post);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductProperty = dataSet.Item2;

			// Set up the API URL
			EndPointUrl = $"{HostUrl}{Properties["ProductOneSite"]}/user/properties?userPersonaId={userPersonaId}&assignedOnly={assignedOnly}&editorPersonaId={personaId}";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			// Deserialization for Response Object
			ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			// Deserialization for Record Object
			List<ProductProperty> productProperties = JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(resProductProperties.Records));

			// Assert
			Assert.Equal(HttpStatusCode.OK, ResponseHttpStatusCode);
			Assert.NotNull(resProductProperties.Records);

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
			Assert.NotNull(resProductProperties.Additional);
			Assert.NotNull(resProductProperties.IsError);
			Assert.NotNull(resProductProperties.ErrorReason);

			// Additional Asserts
			Assert.True(productProperties.Count == resProductProperties.TotalRows);
		}

		//[Theory]
		//[Trait("Data-Driven", "ProductOnesitePropertiesEdit-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly", "disablePropertyRole")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "401", "Regular User with Email", false)]
		[InlineData("TC_004", "404", "Regular User without Email", false)]
		[InlineData("TC_005", "401", "Regular User with Email", false, true)]
		[InlineData("TC_006", "404", "Regular User without Email", false, true)]
		[InlineData("TC_007", "401", "Regular User with Email", true, true)]
		[InlineData("TC_008", "404", "Regular User without Email", true, true)]
		public void GetProductOneSiteUserPropertiesforEdit(string testCaseId, string userTypeId, string userType, bool assignedOnly = true, bool disablePropertyRole = false)
		{
			switch (userTypeId)
			{
				case "401":
					dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put, false, disablePropertyRole);
					break;
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put, false, disablePropertyRole);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductProperty = dataSet.Item2;

			// Set up the API URL
			EndPointUrl =
				$"{HostUrl}{Properties["ProductOneSite"]}/user/properties?userPersonaId={userPersonaId}&assignedOnly={assignedOnly}&editorPersonaId={personaId}";

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
				if (productProperty.ID == expProductProperty && disablePropertyRole == false)
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
		//[Trait("Data-Driven", "ProductOnesitePropertiesEdit-Negative")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User No Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductOneSiteUserPropertiesforEditNegative(string testCaseId, string userTypeId, string userType)
		{
			userPersonaId = reusable.createUserWithoutPropertyAndRole("1", userTypeId);

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
		//[Trait("Data-Driven", "ProductOnesiteRolesAdd-HappyPath")]
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		[InlineData("TC_004", "401", "Regular User with Email", false)]
		[InlineData("TC_005", "404", "Regular User without Email", false)]
		[InlineData("TC_006", "402", "System Administrator", false)]
		public void GetProductOneSiteUserRolesforAdd(string testCaseId, string userTypeId, string userType, bool assignedOnly = true)
		{
			// Set up the API URL
			switch (userTypeId)
			{
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId, newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
					break;
				default:
					dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId, newUsername, HttpVerb.Post);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductRole = dataSet.Item3;

			EndPointUrl = $"{HostUrl}{Properties["ProductOneSite"]}/user/roles?userPersonaId={userPersonaId}&assignedOnly={assignedOnly}&editorPersonaId={personaId}";

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
				if (productRole.ID == expProductRole)
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
		//[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly", "disablePropertyRole")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User without Email")]
		[InlineData("TC_003", "401", "Regular User with Email", false)]
		[InlineData("TC_004", "404", "Regular User without Email", false)]
		[InlineData("TC_005", "401", "Regular User with Email", false, true)]
		[InlineData("TC_006", "404", "Regular User without Email", false, true)]
		[InlineData("TC_007", "401", "Regular User with Email", true, true)]
		[InlineData("TC_008", "404", "Regular User without Email", true, true)]
		public void GetProductOneSiteUserRolesforEdit(string testCaseId, string userTypeId, string userType, bool assignedOnly = true, bool disablePropertyRole = false)
		{
			switch (userTypeId)
			{
				case "401":
					dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put, false, disablePropertyRole);
					break;
				case "404":
					dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put, false, disablePropertyRole);
					break;
			}
			userPersonaId = dataSet.Item1;
			expProductRole = dataSet.Item3;

			// Set up the API URL
			EndPointUrl =
				$"{HostUrl}{Properties["ProductOneSite"]}/user/roles?userPersonaId={userPersonaId}&assignedOnly={assignedOnly}&editorPersonaId={personaId}";

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

		//[Theory]
		//[Trait("Data-Driven", "ProductOnesiteRolesEdit-Negative")]
		//[InlineData("TestCaseID", "userTypeId", "userType")]
		[InlineData("TC_001", "401", "Regular User with Email")]
		[InlineData("TC_002", "404", "Regular User No Email")]
		[InlineData("TC_003", "402", "System Administrator")]
		public void GetProductOneSiteUserRolesforEditNegative(string testCaseId, string userTypeId, string userType)
		{
			userPersonaId = reusable.createUserWithoutPropertyAndRole("1", userTypeId);

			// Set up the API URL
			EndPointUrl =
				$"{HostUrl}{Properties["ProductOneSite"]}/user/roles?userPersonaId={userPersonaId}&assignedOnly=false&editorPersonaId={personaId}";

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
		//[Trait("Data-Driven", "ProductOnesiteProperties")]
		//[InlineData("TestCaseID", "editorPersonaId", "userPersonaId", "assignedOnly", "ExpResponse", "ExpResponseRecords")]
		[InlineData("TC_001", "$editorPersonaId", "0", "true", "OK", "Yes")]
		[InlineData("TC_002", "$editorPersonaId", "0", "false", "OK", "Yes")]
		[InlineData("TC_003", "0", "$userPersonaId", "true", "BadRequest", "No")]
		[InlineData("TC_004", "0", "$userPersonaId", "false", "BadRequest", "No")]
		[InlineData("TC_005", "0", "0", "true", "BadRequest", "No")]
		[InlineData("TC_006", "0", "0", "false", "BadRequest", "No")]
		[InlineData("TC_007", "Invalid", "$userPersonaId", "true", "BadRequest", "No")]
		[InlineData("TC_008", "$editorPersonaId", "Invalid Type", "false", "BadRequest", "No")]
		public void GetProductOneSiteUserPropertiesDataDriven(string testCaseId, string editorPersonaId, string userPersonaId, string assignedOnly, string expResponse, string expObject)
		{
			XunitTestOutPut.WriteLine("Executing TestCase : " + testCaseId);

			if (editorPersonaId == "$editorPersonaId")
			{
				editorPersonaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();
			}

			EndPointUrl =
				$"{HostUrl}{Properties["ProductOneSite"]}/user/properties?userPersonaId={userPersonaId}&assignedOnly={assignedOnly}&editorPersonaId={editorPersonaId}";
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
		//[Trait("Data-Driven", "ProductOnesiteRoles")]
		//[InlineData("TestCaseID", "editorPersonaId", "userPersonaId", "assignedOnly", "ExpResponse", "ExpResponseRecords")]
		[InlineData("TC_001", "$editorPersonaId", "0", "true", "OK", "Yes")]
		[InlineData("TC_002", "$editorPersonaId", "0", "false", "OK", "Yes")]
		[InlineData("TC_003", "0", "$userPersonaId", "true", "BadRequest", "No")]
		[InlineData("TC_004", "0", "$userPersonaId", "false", "BadRequest", "No")]
		[InlineData("TC_005", "0", "0", "true", "BadRequest", "No")]
		[InlineData("TC_006", "0", "0", "false", "BadRequest", "No")]
		[InlineData("TC_007", "Invalid", "$userPersonaId", "true", "BadRequest", "No")]
		[InlineData("TC_008", "$editorPersonaId", "Invalid", "false", "BadRequest", "No")]
		public void GetProductOneSiteUserRolesDataDriven(string testCaseId, string editorPersonaId, string userPersonaId, string assignedOnly, string expResponse, string expObject)
		{
			XunitTestOutPut.WriteLine("Executing TestCase : " + testCaseId);

			if (editorPersonaId == "$editorPersonaId")
			{
				editorPersonaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();
			}

			EndPointUrl = $"{HostUrl}{Properties["ProductOneSite"]}/user/roles?userPersonaId={userPersonaId}&assignedOnly={assignedOnly}&editorPersonaId={editorPersonaId}";
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

							// Additional Asserts
							Assert.True(productRoles.Count == resProductRoles.TotalRows);
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

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetProductOneSiteRole")]
		public void GetProductOneSiteRoleHappyPaths(string testCase)
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/role?editorPersonaId=" + personaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ListResponse listResponseProductOneSiteRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			List<ProductRole> listProductOneSiteRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(listResponseProductOneSiteRoles.Records));

			// Extract Expected JSON Response
			string GetProductOneSiteRoleResponsePath = DataPath + "GetProductOneSiteRoleResponse.json";
			string GetProductOneSiteRoleResponse = jsonManager.LoadJsonAsString(GetProductOneSiteRoleResponsePath);
			ListResponse expectedListResponseProductOneSiteRoles = JsonConvert.DeserializeObject<ListResponse>(GetProductOneSiteRoleResponse);
			List<ProductRole> expectedListProductOneSiteRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(expectedListResponseProductOneSiteRoles.Records));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			for (int countListProductOneSiteRoles = 0; countListProductOneSiteRoles < listProductOneSiteRoles.Count; countListProductOneSiteRoles++)
			{
				if (listProductOneSiteRoles[countListProductOneSiteRoles].ID ==
					expectedListProductOneSiteRoles[countListProductOneSiteRoles].ID)
				{
					Assert.NotNull(listProductOneSiteRoles[countListProductOneSiteRoles].ID);
					Assert.True(listProductOneSiteRoles[countListProductOneSiteRoles].ID ==
						expectedListProductOneSiteRoles[countListProductOneSiteRoles].ID
						, "listProductOneSiteRoles[countListProductOneSiteRoles].ID == "
						+ "expectedListProductOneSiteRoles[countListProductOneSiteRoles].ID");
					Assert.NotNull(listProductOneSiteRoles[countListProductOneSiteRoles].Name);
					Assert.True(listProductOneSiteRoles[countListProductOneSiteRoles].Name ==
						expectedListProductOneSiteRoles[countListProductOneSiteRoles].Name
						, "listProductOneSiteRoles[countListProductOneSiteRoles].Name == "
						+ "expectedListProductOneSiteRoles[countListProductOneSiteRoles].Name");
					Assert.NotNull(listProductOneSiteRoles[countListProductOneSiteRoles].IsAssigned);
					Assert.True(listProductOneSiteRoles[countListProductOneSiteRoles].IsAssigned ==
						expectedListProductOneSiteRoles[countListProductOneSiteRoles].IsAssigned
						, "listProductOneSiteRoles[countListProductOneSiteRoles].IsAssigned == "
						+ "expectedListProductOneSiteRoles[countListProductOneSiteRoles].IsAssigned");
					Assert.NotNull(listProductOneSiteRoles[countListProductOneSiteRoles].Roletype);
					Assert.True(listProductOneSiteRoles[countListProductOneSiteRoles].Roletype ==
						expectedListProductOneSiteRoles[countListProductOneSiteRoles].Roletype
						, "listProductOneSiteRoles[countListProductOneSiteRoles].Roletype == "
						+ "expectedListProductOneSiteRoles[countListProductOneSiteRoles].Roletype");
					Assert.NotNull(listProductOneSiteRoles[countListProductOneSiteRoles].RightsAssigned);
					Assert.True(listProductOneSiteRoles[countListProductOneSiteRoles].RightsAssigned ==
						expectedListProductOneSiteRoles[countListProductOneSiteRoles].RightsAssigned
						, "listProductOneSiteRoles[countListProductOneSiteRoles].RightsAssigned == "
						+ "expectedListProductOneSiteRoles[countListProductOneSiteRoles].RightsAssigned");
				}
			}
			Assert.NotNull(listResponseProductOneSiteRoles.RowsPerPage);
			Assert.True(listResponseProductOneSiteRoles.RowsPerPage == expectedListResponseProductOneSiteRoles.RowsPerPage
				, "listResponseProductOneSiteRoles.RowsPerPage == expectedListResponseProductOneSiteRoles.RowsPerPage");
			Assert.NotNull(listResponseProductOneSiteRoles.SkipRows);
			Assert.True(listResponseProductOneSiteRoles.SkipRows == expectedListResponseProductOneSiteRoles.SkipRows
				, "listResponseProductOneSiteRoles.SkipRows == expectedListResponseProductOneSiteRoles.SkipRows");
			Assert.NotNull(listResponseProductOneSiteRoles.CurrentPage);
			Assert.True(listResponseProductOneSiteRoles.CurrentPage == expectedListResponseProductOneSiteRoles.CurrentPage
				, "listResponseProductOneSiteRoles.CurrentPage == expectedListResponseProductOneSiteRoles.CurrentPage");
			Assert.NotNull(listResponseProductOneSiteRoles.TotalPages);
			Assert.True(listResponseProductOneSiteRoles.TotalPages == expectedListResponseProductOneSiteRoles.TotalPages
				, "listResponseProductOneSiteRoles.TotalPages == expectedListResponseProductOneSiteRoles.TotalPages");
			Assert.NotNull(listResponseProductOneSiteRoles.TotalRows);
			Assert.True(listResponseProductOneSiteRoles.TotalRows == listResponseProductOneSiteRoles.Records.Count
				, "listResponseProductOneSiteRoles.TotalRows != listResponseProductOneSiteRoles.Records.Count");
			Assert.True(listResponseProductOneSiteRoles.Additional == expectedListResponseProductOneSiteRoles.Additional
				, "listResponseProductOneSiteRoles.Additional == expectedListResponseProductOneSiteRoles.Additional");
			Assert.NotNull(listResponseProductOneSiteRoles.IsError);
			Assert.True(listResponseProductOneSiteRoles.IsError == expectedListResponseProductOneSiteRoles.IsError
				, "listResponseProductOneSiteRoles.IsError == expectedListResponseProductOneSiteRoles.IsError");
			Assert.NotNull(listResponseProductOneSiteRoles.ErrorReason);
			Assert.True(listResponseProductOneSiteRoles.ErrorReason == expectedListResponseProductOneSiteRoles.ErrorReason
				, "listResponseProductOneSiteRoles.ErrorReason == expectedListResponseProductOneSiteRoles.ErrorReason");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetProductOneSiteRoleNoEditorPersonaId", "BadRequest", "The request is invalid.")]
		[InlineData("GetProductOneSiteRoleInvalidNumericEditorPersonaId", "InternalServerError", "Internal System Error. Please contact RealPage support with error reference Id - ", "0")]
		[InlineData("GetProductOneSiteRoleInvalidAlphabeticEditorPersonaId", "BadRequest", "The request is invalid.", "aBc")]
		public void GetProductOneSiteRoleNegativeCases(string testCase, string statusCode, string errorReason, string editorPersonaId = "")
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/role?editorPersonaId=" + editorPersonaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains(errorReason));
			if (statusCode == "InternalServerError")
			{
				Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
			}
			else if (statusCode == "BadRequest")
			{
				Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			}
		}

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetProductOneSiteRightRoles")]
		[InlineData("GetProductOneSiteRightRolesAssignedOnlyFalse", false)]
		public void GetProductOneSiteRightRolesHappyPaths(string testCase, bool assignedOnly = true)
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/right/roles?editorPersonaId="
				+ personaId + "&rightId=" + rightId + "&assignedOnly=" + assignedOnly;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ListResponse listResponseProductOneSiteRightRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			List<ProductRole> listProductOneSiteRightRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(listResponseProductOneSiteRightRoles.Records));

			// Extract Expected JSON Response
			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/role?editorPersonaId=" + personaId;
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl + " for the Expected Results.");
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\nExpected Results:\n" + ResponseString);
			ListResponse expectedListResponseProductOneSiteRightRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			List<ProductRole> expectedListProductOneSiteRightRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(expectedListResponseProductOneSiteRightRoles.Records));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			for (int countListProductOneSiteRoles = 0; countListProductOneSiteRoles < listProductOneSiteRightRoles.Count; countListProductOneSiteRoles++)
			{
				if (listProductOneSiteRightRoles[countListProductOneSiteRoles].ID ==
					expectedListProductOneSiteRightRoles[countListProductOneSiteRoles].ID)
				{
					Assert.NotNull(listProductOneSiteRightRoles[countListProductOneSiteRoles].ID);
					Assert.True(listProductOneSiteRightRoles[countListProductOneSiteRoles].ID ==
						expectedListProductOneSiteRightRoles[countListProductOneSiteRoles].ID
						, "listProductOneSiteRoles[countListProductOneSiteRoles].ID == "
						+ "expectedListProductOneSiteRoles[countListProductOneSiteRoles].ID");
					Assert.NotNull(listProductOneSiteRightRoles[countListProductOneSiteRoles].Name);
					Assert.True(listProductOneSiteRightRoles[countListProductOneSiteRoles].Name ==
						expectedListProductOneSiteRightRoles[countListProductOneSiteRoles].Name
						, "listProductOneSiteRoles[countListProductOneSiteRoles].Name == "
						+ "expectedListProductOneSiteRoles[countListProductOneSiteRoles].Name");
					Assert.NotNull(listProductOneSiteRightRoles[countListProductOneSiteRoles].IsAssigned);
					if (assignedOnly)
					{
						Assert.True(listProductOneSiteRightRoles[countListProductOneSiteRoles].IsAssigned
							, $"listProductOneSiteRoles[countListProductOneSiteRoles].IsAssigned is not \"{assignedOnly}\".");
					}
					else
					{
						Assert.True(listProductOneSiteRightRoles[countListProductOneSiteRoles].IsAssigned || !listProductOneSiteRightRoles[countListProductOneSiteRoles].IsAssigned
							, $"listProductOneSiteRoles[countListProductOneSiteRoles].IsAssigned is not \"{assignedOnly}\".");
					}
					Assert.NotNull(listProductOneSiteRightRoles[countListProductOneSiteRoles].Roletype);
					Assert.True(listProductOneSiteRightRoles[countListProductOneSiteRoles].Roletype ==
						expectedListProductOneSiteRightRoles[countListProductOneSiteRoles].Roletype
						, "listProductOneSiteRoles[countListProductOneSiteRoles].Roletype == "
						+ "expectedListProductOneSiteRoles[countListProductOneSiteRoles].Roletype");
					Assert.NotNull(listProductOneSiteRightRoles[countListProductOneSiteRoles].RightsAssigned);
					Assert.True(listProductOneSiteRightRoles[countListProductOneSiteRoles].RightsAssigned ==
						expectedListProductOneSiteRightRoles[countListProductOneSiteRoles].RightsAssigned
						, "listProductOneSiteRoles[countListProductOneSiteRoles].RightsAssigned == "
						+ "expectedListProductOneSiteRoles[countListProductOneSiteRoles].RightsAssigned");
				}
			}
			Assert.NotNull(listResponseProductOneSiteRightRoles.RowsPerPage);
			Assert.True(listResponseProductOneSiteRightRoles.RowsPerPage == expectedListResponseProductOneSiteRightRoles.RowsPerPage
				, "listResponseProductOneSiteRightRoles.RowsPerPage == expectedListResponseProductOneSiteRightRoles.RowsPerPage");
			Assert.NotNull(listResponseProductOneSiteRightRoles.SkipRows);
			Assert.True(listResponseProductOneSiteRightRoles.SkipRows == expectedListResponseProductOneSiteRightRoles.SkipRows
				, "listResponseProductOneSiteRightRoles.SkipRows == expectedListResponseProductOneSiteRightRoles.SkipRows");
			Assert.NotNull(listResponseProductOneSiteRightRoles.CurrentPage);
			Assert.True(listResponseProductOneSiteRightRoles.CurrentPage == expectedListResponseProductOneSiteRightRoles.CurrentPage
				, "listResponseProductOneSiteRightRoles.CurrentPage == expectedListResponseProductOneSiteRightRoles.CurrentPage");
			Assert.NotNull(listResponseProductOneSiteRightRoles.TotalPages);
			Assert.True(listResponseProductOneSiteRightRoles.TotalPages == expectedListResponseProductOneSiteRightRoles.TotalPages
				, "listResponseProductOneSiteRightRoles.TotalPages == expectedListResponseProductOneSiteRightRoles.TotalPages");
			Assert.NotNull(listResponseProductOneSiteRightRoles.TotalRows);
			Assert.True(listResponseProductOneSiteRightRoles.TotalRows == listResponseProductOneSiteRightRoles.Records.Count
				, "listResponseProductOneSiteRightRoles.TotalRows != listResponseProductOneSiteRightRoles.Records.Count");
			Assert.True(listResponseProductOneSiteRightRoles.Additional == expectedListResponseProductOneSiteRightRoles.Additional
				, "listResponseProductOneSiteRightRoles.Additional == expectedListResponseProductOneSiteRightRoles.Additional");
			Assert.NotNull(listResponseProductOneSiteRightRoles.IsError);
			Assert.True(listResponseProductOneSiteRightRoles.IsError == expectedListResponseProductOneSiteRightRoles.IsError
				, "listResponseProductOneSiteRightRoles.IsError == expectedListResponseProductOneSiteRightRoles.IsError");
			Assert.NotNull(listResponseProductOneSiteRightRoles.ErrorReason);
			Assert.True(listResponseProductOneSiteRightRoles.ErrorReason == expectedListResponseProductOneSiteRightRoles.ErrorReason
				, "listResponseProductOneSiteRightRoles.ErrorReason == expectedListResponseProductOneSiteRightRoles.ErrorReason");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetProductOneSiteRightRolesValidEditorPersonaIdNoRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "validEditorPersonaId", "", "")]
		[InlineData("GetProductOneSiteRightRolesValidEditorPersonaIdValidRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "validEditorPersonaId", "validRightId", "")]
		[InlineData("GetProductOneSiteRightRolesValidEditorPersonaIdValidRightIdInvalidAssignedOnly", "BadRequest", "The request is invalid.", "validEditorPersonaId", "validRightId", "invalidAssignedOnly")]
		[InlineData("GetProductOneSiteRightRolesValidEditorPersonaIdInvalidRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "validEditorPersonaId", "invalidRightId", "")]
		[InlineData("GetProductOneSiteRightRolesValidEditorPersonaIdInvalidRightIdTrueAssignedOnly", "BadRequest", "The request is invalid.", "validEditorPersonaId", "invalidRightId")]
		[InlineData("GetProductOneSiteRightRolesValidEditorPersonaIdInvalidRightIdFalseAssignedOnly", "BadRequest", "The request is invalid.", "validEditorPersonaId", "invalidRightId", "false")]
		[InlineData("GetProductOneSiteRightRolesValidEditorPersonaIdInvalidRightIdInvalidAssignedOnly", "BadRequest", "The request is invalid.", "validEditorPersonaId", "invalidRightId", "invalidAssignedOnly")]
		[InlineData("GetProductOneSiteRightRolesNoEditorPersonaIdNoRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "", "", "")]
		[InlineData("GetProductOneSiteRightRolesNoEditorPersonaIdValidRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "", "validRightId", "")]
		[InlineData("GetProductOneSiteRightRolesNoEditorPersonaIdValidRightIdTrueAssignedOnly", "BadRequest", "The request is invalid.", "", "validRightId")]
		[InlineData("GetProductOneSiteRightRolesNoEditorPersonaIdValidRightIdFalseAssignedOnly", "BadRequest", "The request is invalid.", "", "validRightId", "false")]
		[InlineData("GetProductOneSiteRightRolesNoEditorPersonaIdValidRightIdInvalidAssignedOnly", "BadRequest", "The request is invalid.", "", "validRightId", "invalidAssignedOnly")]
		[InlineData("GetProductOneSiteRightRolesNoEditorPersonaIdInvalidRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "", "invalidRightId", "")]
		[InlineData("GetProductOneSiteRightRolesNoEditorPersonaIdInvalidRightIdTrueAssignedOnly", "BadRequest", "The request is invalid.", "", "invalidRightId")]
		[InlineData("GetProductOneSiteRightRolesNoEditorPersonaIdInvalidRightIdFalseAssignedOnly", "BadRequest", "The request is invalid.", "", "invalidRightId", "false")]
		[InlineData("GetProductOneSiteRightRolesNoEditorPersonaIdInvalidRightIdInvalidAssignedOnly", "BadRequest", "The request is invalid.", "", "invalidRightId", "invalidAssignedOnly")]
		[InlineData("GetProductOneSiteRightRolesInvalidNumericEditorPersonaIdNoRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "0", "", "")]
		[InlineData("GetProductOneSiteRightRolesInvalidNumericEditorPersonaIdValidRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "0", "validRightId", "")]
		[InlineData("GetProductOneSiteRightRolesInvalidNumericEditorPersonaIdValidRightIdTrueAssignedOnly", "OK", "Invalid persona", "0", "validRightId")]
		[InlineData("GetProductOneSiteRightRolesInvalidNumericEditorPersonaIdValidRightIdFalseAssignedOnly", "OK", "Invalid persona", "0", "validRightId", "false")]
		[InlineData("GetProductOneSiteRightRolesInvalidNumericEditorPersonaIdValidRightIdInvalidAssignedOnly", "BadRequest", "The request is invalid.", "0", "validRightId", "invalidAssignedOnly")]
		[InlineData("GetProductOneSiteRightRolesInvalidNumericEditorPersonaIdInvalidRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "0", "invalidRightId", "")]
		[InlineData("GetProductOneSiteRightRolesInvalidNumericEditorPersonaIdInvalidRightIdTrueAssignedOnly", "BadRequest", "The request is invalid.", "0", "invalidRightId")]
		[InlineData("GetProductOneSiteRightRolesInvalidNumericEditorPersonaIdInvalidRightIdFalseAssignedOnly", "BadRequest", "The request is invalid.", "0", "invalidRightId", "false")]
		[InlineData("GetProductOneSiteRightRolesInvalidNumericEditorPersonaIdInvalidRightIdInvalidAssignedOnly", "BadRequest", "The request is invalid.", "0", "invalidRightId", "invalidAssignedOnly")]
		[InlineData("GetProductOneSiteRightRolesInvalidAlphabeticEditorPersonaId", "BadRequest", "The request is invalid.", "aBc", "", "")]
		[InlineData("GetProductOneSiteRightRolesInvalidAlphabeticEditorPersonaIdValidRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "aBc", "validRightId", "")]
		[InlineData("GetProductOneSiteRightRolesInvalidAlphabeticEditorPersonaIdValidRightIdTrueAssignedOnly", "BadRequest", "The request is invalid.", "aBc", "validRightId")]
		[InlineData("GetProductOneSiteRightRolesInvalidAlphabeticEditorPersonaIdValidRightIdFalseAssignedOnly", "BadRequest", "The request is invalid.", "aBc", "validRightId", "false")]
		[InlineData("GetProductOneSiteRightRolesInvalidAlphabeticEditorPersonaIdValidRightIdInvalidAssignedOnly", "BadRequest", "The request is invalid.", "aBc", "validRightId", "invalidAssignedOnly")]
		[InlineData("GetProductOneSiteRightRolesInvalidAlphabeticEditorPersonaIdInvalidRightIdNoAssignedOnly", "BadRequest", "The request is invalid.", "aBc", "invalidRightId", "")]
		[InlineData("GetProductOneSiteRightRolesInvalidAlphabeticEditorPersonaIdInvalidRightIdTrueAssignedOnly", "BadRequest", "The request is invalid.", "aBc", "invalidRightId")]
		[InlineData("GetProductOneSiteRightRolesInvalidAlphabeticEditorPersonaIdInvalidRightIdFalseAssignedOnly", "BadRequest", "The request is invalid.", "aBc", "invalidRightId", "false")]
		[InlineData("GetProductOneSiteRightRolesInvalidAlphabeticEditorPersonaIdInvalidRightIdInvalidAssignedOnly", "BadRequest", "The request is invalid.", "aBc", "invalidRightId", "invalidAssignedOnly")]
		public void GetProductOneSiteRightRolesNegativeCases(string testCase, string statusCode, string errorReason, string editorPersonaId = "validEditorPersonaId", string testRightId = "validRightId", string assignedOnly = "true")
		{
			// Set up the API URL
			editorPersonaId = editorPersonaId == "validEditorPersonaId" ? personaId : editorPersonaId;
			testRightId = testRightId == "validRightId" ? rightId : testRightId;
			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/right/roles?editorPersonaId="
				+ editorPersonaId + "&rightId=" + testRightId + "&assignedOnly=" + assignedOnly;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			if (statusCode == "OK")
			{
				ListResponse listResponseProductOneSiteRoleRights = JsonConvert.DeserializeObject<ListResponse>(ResponseString);

				Assert.StrictEqual(HttpStatusCode.OK, ResponseHttpStatusCode);
				Assert.Null(listResponseProductOneSiteRoleRights.Records);
				Assert.NotNull(listResponseProductOneSiteRoleRights.RowsPerPage);
				Assert.True(listResponseProductOneSiteRoleRights.RowsPerPage == 0, "listResponseProductOneSiteRights.RowsPerPage == 0");
				Assert.NotNull(listResponseProductOneSiteRoleRights.SkipRows);
				Assert.True(listResponseProductOneSiteRoleRights.SkipRows == 0, "listResponseProductOneSiteRights.SkipRows == 0");
				Assert.NotNull(listResponseProductOneSiteRoleRights.CurrentPage);
				Assert.True(listResponseProductOneSiteRoleRights.CurrentPage == 0, "listResponseProductOneSiteRights.CurrentPage == 0");
				Assert.NotNull(listResponseProductOneSiteRoleRights.TotalPages);
				Assert.True(listResponseProductOneSiteRoleRights.TotalPages == 1, "listResponseProductOneSiteRights.TotalPages == 1");
				Assert.NotNull(listResponseProductOneSiteRoleRights.TotalRows);
				Assert.True(listResponseProductOneSiteRoleRights.TotalRows == 0, "listResponseProductOneSiteRights.TotalRows == 0");
				Assert.NotNull(listResponseProductOneSiteRoleRights.IsError);
				Assert.True(listResponseProductOneSiteRoleRights.IsError, "listResponseProductOneSiteRights.IsError");
				Assert.NotNull(listResponseProductOneSiteRoleRights.ErrorReason);
				Assert.True(listResponseProductOneSiteRoleRights.ErrorReason == errorReason, $"listResponseProductOneSiteRights.ErrorReason is not \"{errorReason}\".");
			}
			else
			{
				Assert.NotNull(ResponseString);
				Assert.True(ResponseString.Contains(errorReason));
				if (statusCode == "InternalServerError")
				{
					Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
				}
				else if (statusCode == "BadRequest")
				{
					Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
				}
			}
		}

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetProductOneSiteRights")]
		public void GetProductOneSiteRightsHappyPaths(string testCase)
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/rights?editorPersonaId=" + personaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ListResponse listResponseProductOneSiteRights = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			List<ProductRight> listProductOneSiteRights = JsonConvert.DeserializeObject<List<ProductRight>>(JsonConvert.SerializeObject(listResponseProductOneSiteRights.Records));

			// Extract Expected JSON Response
			string GetProductOneSiteRightsResponsePath = DataPath + "GetProductOneSiteRightsResponse.json";
			string GetProductOneSiteRightsResponse = jsonManager.LoadJsonAsString(GetProductOneSiteRightsResponsePath);
			ListResponse expectedListResponseProductOneSiteRights = JsonConvert.DeserializeObject<ListResponse>(GetProductOneSiteRightsResponse);
			List<ProductRight> expectedListProductOneSiteRights = JsonConvert.DeserializeObject<List<ProductRight>>(JsonConvert.SerializeObject(listResponseProductOneSiteRights.Records));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			for (int countListProductOneSiteRights = 0; countListProductOneSiteRights < listProductOneSiteRights.Count; countListProductOneSiteRights++)
			{
				Assert.NotNull(listProductOneSiteRights[countListProductOneSiteRights].ID);
				Assert.True(listProductOneSiteRights[countListProductOneSiteRights].ID ==
					expectedListProductOneSiteRights[countListProductOneSiteRights].ID
					, "listProductOneSiteRights[countListProductOneSiteRights].ID == "
					+ "expectedListProductOneSiteRights[countListProductOneSiteRights].ID");
				Assert.NotNull(listProductOneSiteRights[countListProductOneSiteRights].Description);
				Assert.True(listProductOneSiteRights[countListProductOneSiteRights].Description ==
					expectedListProductOneSiteRights[countListProductOneSiteRights].Description
					, "listProductOneSiteRights[countListProductOneSiteRights].Description == "
					+ "expectedListProductOneSiteRights[countListProductOneSiteRights].Description");
				Assert.NotNull(listProductOneSiteRights[countListProductOneSiteRights].CenterName);
				Assert.True(listProductOneSiteRights[countListProductOneSiteRights].CenterName ==
					expectedListProductOneSiteRights[countListProductOneSiteRights].CenterName
					, "listProductOneSiteRights[countListProductOneSiteRights].CenterName == "
					+ "expectedListProductOneSiteRights[countListProductOneSiteRights].CenterName");
				Assert.NotNull(listProductOneSiteRights[countListProductOneSiteRights].Assigned);
				Assert.True(listProductOneSiteRights[countListProductOneSiteRights].Assigned ==
					expectedListProductOneSiteRights[countListProductOneSiteRights].Assigned
					, "listProductOneSiteRights[countListProductOneSiteRights].Assigned == "
					+ "expectedListProductOneSiteRights[countListProductOneSiteRights].Assigned");
				Assert.NotNull(listProductOneSiteRights[countListProductOneSiteRights].RolesAssigned);
				Assert.True(listProductOneSiteRights[countListProductOneSiteRights].RolesAssigned ==
					expectedListProductOneSiteRights[countListProductOneSiteRights].RolesAssigned
					, "listProductOneSiteRights[countListProductOneSiteRights].RolesAssigned == "
					+ "expectedListProductOneSiteRights[countListProductOneSiteRights].RolesAssigned");
			}
			Assert.NotNull(listResponseProductOneSiteRights.RowsPerPage);
			Assert.True(listResponseProductOneSiteRights.RowsPerPage == expectedListResponseProductOneSiteRights.RowsPerPage
				, "listResponseProductOneSiteRights.RowsPerPage == expectedListResponseProductOneSiteRights.RowsPerPage");
			Assert.NotNull(listResponseProductOneSiteRights.SkipRows);
			Assert.True(listResponseProductOneSiteRights.SkipRows == expectedListResponseProductOneSiteRights.SkipRows
				, "listResponseProductOneSiteRights.SkipRows == expectedListResponseProductOneSiteRights.SkipRows");
			Assert.NotNull(listResponseProductOneSiteRights.CurrentPage);
			Assert.True(listResponseProductOneSiteRights.CurrentPage == expectedListResponseProductOneSiteRights.CurrentPage
				, "listResponseProductOneSiteRights.CurrentPage == expectedListResponseProductOneSiteRights.CurrentPage");
			Assert.NotNull(listResponseProductOneSiteRights.TotalPages);
			Assert.True(listResponseProductOneSiteRights.TotalPages == expectedListResponseProductOneSiteRights.TotalPages
				, "listResponseProductOneSiteRights.TotalPages == expectedListResponseProductOneSiteRights.TotalPages");
			Assert.NotNull(listResponseProductOneSiteRights.TotalRows);
			Assert.True(listResponseProductOneSiteRights.TotalRows == listResponseProductOneSiteRights.Records.Count
				, "listResponseProductOneSiteRights.TotalRows != listResponseProductOneSiteRights.Records.Count");
			Assert.True(listResponseProductOneSiteRights.Additional == expectedListResponseProductOneSiteRights.Additional
				, "listResponseProductOneSiteRights.Additional == expectedListResponseProductOneSiteRights.Additional");
			Assert.NotNull(listResponseProductOneSiteRights.IsError);
			Assert.True(listResponseProductOneSiteRights.IsError == expectedListResponseProductOneSiteRights.IsError
				, "listResponseProductOneSiteRights.IsError == expectedListResponseProductOneSiteRights.IsError");
			Assert.NotNull(listResponseProductOneSiteRights.ErrorReason);
			Assert.True(listResponseProductOneSiteRights.ErrorReason == expectedListResponseProductOneSiteRights.ErrorReason
				, "listResponseProductOneSiteRights.ErrorReason == expectedListResponseProductOneSiteRights.ErrorReason");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetProductOneSiteRightsNoEditorPersonaId", "BadRequest", "The request is invalid.")]
		[InlineData("GetProductOneSiteRightsInvalidNumericEditorPersonaId", "OK", "Invalid persona", "0")]
		[InlineData("GetProductOneSiteRightsInvalidAlphabeticEditorPersonaId", "BadRequest", "The request is invalid.", "aBc")]
		public void GetProductOneSiteRightsNegativeCases(string testCase, string statusCode, string errorReason, string editorPersonaId = "")
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/rights?editorPersonaId=" + editorPersonaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			if (statusCode == "OK")
			{
				ListResponse listResponseProductOneSiteRights = JsonConvert.DeserializeObject<ListResponse>(ResponseString);

				Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
				Assert.Null(listResponseProductOneSiteRights.Records);
				Assert.NotNull(listResponseProductOneSiteRights.RowsPerPage);
				Assert.True(listResponseProductOneSiteRights.RowsPerPage == 0, "listResponseProductOneSiteRights.RowsPerPage == 0");
				Assert.NotNull(listResponseProductOneSiteRights.SkipRows);
				Assert.True(listResponseProductOneSiteRights.SkipRows == 0, "listResponseProductOneSiteRights.SkipRows == 0");
				Assert.NotNull(listResponseProductOneSiteRights.CurrentPage);
				Assert.True(listResponseProductOneSiteRights.CurrentPage == 0, "listResponseProductOneSiteRights.CurrentPage == 0");
				Assert.NotNull(listResponseProductOneSiteRights.TotalPages);
				Assert.True(listResponseProductOneSiteRights.TotalPages == 1, "listResponseProductOneSiteRights.TotalPages == 1");
				Assert.NotNull(listResponseProductOneSiteRights.TotalRows);
				Assert.True(listResponseProductOneSiteRights.TotalRows == 0, "listResponseProductOneSiteRights.TotalRows == 0");
				Assert.NotNull(listResponseProductOneSiteRights.IsError);
				Assert.True(listResponseProductOneSiteRights.IsError, "listResponseProductOneSiteRights.IsError");
				Assert.NotNull(listResponseProductOneSiteRights.ErrorReason);
				Assert.True(listResponseProductOneSiteRights.ErrorReason == errorReason, $"listResponseProductOneSiteRights.ErrorReason is not \"{errorReason}\".");
			}
			else
			{
				if (statusCode == "InternalServerError")
				{
					Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
				}
				else if (statusCode == "BadRequest")
				{
					Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
				}
				Assert.NotNull(ResponseString);
				Assert.True(ResponseString.Contains(errorReason));
			}
		}

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetProductOneSiteRightCenter")]
		public void GetProductOneSiteRightCenterHappyPaths(string testCase)
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/Right/Center?editorPersonaId=" + personaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ListResponse listResponseProductOneSiteRightCenter = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
			List<string> listProductOneSiteRightCenter = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(listResponseProductOneSiteRightCenter.Records));

			// Extract Expected JSON Response
			string GetProductOneSiteRightCenterResponsePath = DataPath + "GetProductOneSiteRightCenterResponse.json";
			string GetProductOneSiteRightCenterResponse = jsonManager.LoadJsonAsString(GetProductOneSiteRightCenterResponsePath);
			ListResponse expectedListResponseProductOneSiteRightCenter = JsonConvert.DeserializeObject<ListResponse>(GetProductOneSiteRightCenterResponse);
			List<string> expectedListProductOneSiteRightCenter = JsonConvert.DeserializeObject<List<string>>(JsonConvert.SerializeObject(listResponseProductOneSiteRightCenter.Records));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			for (int countListProductOneSiteRightCenter = 0; countListProductOneSiteRightCenter < listProductOneSiteRightCenter.Count; countListProductOneSiteRightCenter++)
			{
				Assert.NotNull(listProductOneSiteRightCenter[countListProductOneSiteRightCenter]);
				Assert.True(listProductOneSiteRightCenter[countListProductOneSiteRightCenter] ==
					expectedListProductOneSiteRightCenter[countListProductOneSiteRightCenter]
					, "listProductOneSiteRightCenter[countListProductOneSiteRightCenter] == "
					+ "expectedListProductOneSiteRightCenter[countListProductOneSiteRightCenter]");
			}
			Assert.NotNull(listResponseProductOneSiteRightCenter.RowsPerPage);
			Assert.True(listResponseProductOneSiteRightCenter.RowsPerPage == expectedListResponseProductOneSiteRightCenter.RowsPerPage
				, "listResponseProductOneSiteRightCenter.RowsPerPage == expectedListResponseProductOneSiteRightCenter.RowsPerPage");
			Assert.NotNull(listResponseProductOneSiteRightCenter.SkipRows);
			Assert.True(listResponseProductOneSiteRightCenter.SkipRows == expectedListResponseProductOneSiteRightCenter.SkipRows
				, "listResponseProductOneSiteRightCenter.SkipRows == expectedListResponseProductOneSiteRightCenter.SkipRows");
			Assert.NotNull(listResponseProductOneSiteRightCenter.CurrentPage);
			Assert.True(listResponseProductOneSiteRightCenter.CurrentPage == expectedListResponseProductOneSiteRightCenter.CurrentPage
				, "listResponseProductOneSiteRightCenter.CurrentPage == expectedListResponseProductOneSiteRightCenter.CurrentPage");
			Assert.NotNull(listResponseProductOneSiteRightCenter.TotalPages);
			Assert.True(listResponseProductOneSiteRightCenter.TotalPages == expectedListResponseProductOneSiteRightCenter.TotalPages
				, "listResponseProductOneSiteRightCenter.TotalPages == expectedListResponseProductOneSiteRightCenter.TotalPages");
			Assert.NotNull(listResponseProductOneSiteRightCenter.TotalRows);
			Assert.True(listResponseProductOneSiteRightCenter.TotalRows == listResponseProductOneSiteRightCenter.Records.Count
				, "listResponseProductOneSiteRightCenter.TotalRows != listResponseProductOneSiteRightCenter.Records.Count");
			Assert.True(listResponseProductOneSiteRightCenter.Additional == expectedListResponseProductOneSiteRightCenter.Additional
				, "listResponseProductOneSiteRightCenter.Additional == expectedListResponseProductOneSiteRightCenter.Additional");
			Assert.NotNull(listResponseProductOneSiteRightCenter.IsError);
			Assert.True(listResponseProductOneSiteRightCenter.IsError == expectedListResponseProductOneSiteRightCenter.IsError
				, "listResponseProductOneSiteRightCenter.IsError == expectedListResponseProductOneSiteRightCenter.IsError");
			Assert.NotNull(listResponseProductOneSiteRightCenter.ErrorReason);
			Assert.True(listResponseProductOneSiteRightCenter.ErrorReason == expectedListResponseProductOneSiteRightCenter.ErrorReason
				, "listResponseProductOneSiteRightCenter.ErrorReason == expectedListResponseProductOneSiteRightCenter.ErrorReason");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetProductOneSiteRightCenterNoEditorPersonaId", "BadRequest", "The request is invalid.")]
		[InlineData("GetProductOneSiteRightCenterInvalidNumericEditorPersonaId", "InternalServerError", "Internal System Error. Please contact RealPage support with error reference Id - ", "0")]
		[InlineData("GetProductOneSiteRightCenterInvalidAlphabeticEditorPersonaId", "BadRequest", "The request is invalid.", "aBc")]
		public void GetProductOneSiteRightCenterNegativeCases(string testCase, string statusCode, string errorReason, string editorPersonaId = "")
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["ProductOneSite"] + "/Right/Center?editorPersonaId=" + editorPersonaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			if (statusCode == "OK")
			{
				ListResponse listResponseProductOneSiteRightCenter = JsonConvert.DeserializeObject<ListResponse>(ResponseString);

				Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
				Assert.Null(listResponseProductOneSiteRightCenter.Records);
				Assert.NotNull(listResponseProductOneSiteRightCenter.RowsPerPage);
				Assert.True(listResponseProductOneSiteRightCenter.RowsPerPage == 0, "listResponseProductOneSiteRightCenter.RowsPerPage == 0");
				Assert.NotNull(listResponseProductOneSiteRightCenter.SkipRows);
				Assert.True(listResponseProductOneSiteRightCenter.SkipRows == 0, "listResponseProductOneSiteRightCenter.SkipRows == 0");
				Assert.NotNull(listResponseProductOneSiteRightCenter.CurrentPage);
				Assert.True(listResponseProductOneSiteRightCenter.CurrentPage == 0, "listResponseProductOneSiteRightCenter.CurrentPage == 0");
				Assert.NotNull(listResponseProductOneSiteRightCenter.TotalPages);
				Assert.True(listResponseProductOneSiteRightCenter.TotalPages == 1, "listResponseProductOneSiteRightCenter.TotalPages == 1");
				Assert.NotNull(listResponseProductOneSiteRightCenter.TotalRows);
				Assert.True(listResponseProductOneSiteRightCenter.TotalRows == 0, "listResponseProductOneSiteRightCenter.TotalRows == 0");
				Assert.NotNull(listResponseProductOneSiteRightCenter.IsError);
				Assert.True(listResponseProductOneSiteRightCenter.IsError, "listResponseProductOneSiteRightCenter.IsError");
				Assert.NotNull(listResponseProductOneSiteRightCenter.ErrorReason);
				Assert.True(listResponseProductOneSiteRightCenter.ErrorReason == errorReason, $"listResponseProductOneSiteRightCenter.ErrorReason is not \"{errorReason}\".");
			}
			else
			{
				if (statusCode == "InternalServerError")
				{
					Assert.True(HttpStatusCode.InternalServerError == ResponseHttpStatusCode, "HttpStatusCode.InternalServerError == ResponseHttpStatusCode");
				}
				else if (statusCode == "BadRequest")
				{
					Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
				}
				Assert.NotNull(ResponseString);
				Assert.True(ResponseString.Contains(errorReason));
			}
		}
    }
}

 



