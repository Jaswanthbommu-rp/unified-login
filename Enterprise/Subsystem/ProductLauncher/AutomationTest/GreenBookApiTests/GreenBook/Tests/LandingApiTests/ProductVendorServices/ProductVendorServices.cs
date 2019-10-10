using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.VendorServices;
using System;
using System.Threading;

namespace GreenBook.Tests.LandingApiTests.ProductVendorServices
{
	public class ProductVendorServices : TestController
	{
		public ProductVendorServices(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			_accessToken = GetClientToken(Properties["identityClientUrl"], "james@test.com", "P@ssw0rd");
			personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();

			newUsername = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
		}
		private string personaId, expectedProductRole = "", expectedProductProperty = "", newUsername, userTypeId, userPersonaId;
		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private Tuple<string, string, string, bool, bool> userTuple = new Tuple<string, string, string, bool, bool>("", "", "", true, true);
		
		// ProductVendorServices=/api/products/vendorServices

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetProductVendorServicesProperties")]
		[InlineData("GetProductVendorServicesPropertiesPostNewUser")]
		[InlineData("GetProductVendorServicesPropertiesPostNewUserWithoutEmail", "Regular User (No Email)")]
		[InlineData("GetProductVendorServicesPropertiesPostNewUserSuperUser", "RealPage System Administrator")]
		[InlineData("GetProductVendorServicesPropertiesPutUserProfileDetail")]
		[InlineData("GetProductVendorServicesPropertiesPutUserProfileDetailWithoutEmail", "Regular User (No Email)")]
		public void GetProductVendorServicesPropertiesHappyPaths(string testCase, string userType = "Regular User")
		{
			// Set up the API URL
			userTypeId = reusable.DoGetUserRoleTypeId(userType);
			List<ProductProperty> vendorServicesPropertyRecordsForAddUser = new List<ProductProperty>();
			if (testCase == "GetProductVendorServicesProperties")
			{
				userPersonaId = "0";
			}
			else
			{
				if (testCase.Contains("GetProductVendorServicesPropertiesPostNewUser"))
				{
					newUsername = testCase.Contains("WithoutEmail") ? newUsername.Replace("@ApiTest.com", "") : newUsername;
					userTuple = reusable.createUserWithPropertyAndRole("16", userTypeId, newUsername, HttpVerb.Post);
				}
				else if (testCase.Contains("GetProductVendorServicesPropertiesPutUserProfileDetail"))
				{
					if (testCase.Contains("WithoutEmail"))
					{
						userTuple = reusable.createUserWithPropertyAndRole("16", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put);
					}
					else
					{
						userTuple = reusable.createUserWithPropertyAndRole("16", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put);
					}
				}
				userPersonaId = userTuple.Item1;
				expectedProductProperty = userTuple.Item2;
				expectedProductRole = userTuple.Item3;
			}
			EndPointUrl = HostUrl + Properties["ProductVendorServices"] + "properties?editorPersonaId=" + personaId
				+ "&userPersonaId=" + userPersonaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ListResponse vendorServicesProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(vendorServicesProperties.Records);
			Assert.True(vendorServicesProperties.Records.Count > 0, "vendorServicesProperties.Records.Count !> 0");

			List<ProductProperty> vendorServicesPropertyRecords = JsonConvert.DeserializeObject<
				List<ProductProperty>>(JsonConvert.SerializeObject(vendorServicesProperties.Records));

			foreach (ProductProperty vendorServicesProperty in vendorServicesPropertyRecords)
			{
				Assert.NotNull(vendorServicesProperty.ID);
				Assert.True(int.Parse(vendorServicesProperty.ID) > 0, "int.Parse(vendorServicesProperty.ID) !> 0");
				Assert.NotNull(vendorServicesProperty.Name);
				Assert.True(vendorServicesProperty.Name.Length > 0, "vendorServicesProperty.Name.Length !> 0");
				if (!string.IsNullOrEmpty(vendorServicesProperty.State))
				{
					Assert.True(vendorServicesProperty.State.Length == 2, "vendorServicesProperty.State.Length != 2");
				}
				Assert.NotNull(vendorServicesProperty.IsAssigned);
				if (vendorServicesProperty.ID == expectedProductProperty)
				{
					Assert.True(vendorServicesProperty.IsAssigned, "vendorServicesProperty.IsAssigned is false.");
				}
				else
				{
					Assert.False(vendorServicesProperty.IsAssigned, "vendorServicesProperty.IsAssigned is true.");
				}
			}

			Assert.NotNull(vendorServicesProperties.RowsPerPage);
			Assert.True(vendorServicesProperties.RowsPerPage > 0, "vendorServicesProperties.RowsPerPage !> 0");
			Assert.NotNull(vendorServicesProperties.SkipRows);
			Assert.True(vendorServicesProperties.SkipRows == 0, "vendorServicesProperties.SkipRows != 0");
			Assert.NotNull(vendorServicesProperties.CurrentPage);
			Assert.True(vendorServicesProperties.CurrentPage == 0, "vendorServicesProperties.CurrentPage != 0");
			Assert.NotNull(vendorServicesProperties.TotalPages);
			Assert.True(vendorServicesProperties.TotalPages > 0, "vendorServicesProperties.TotalPages !> 0");
			Assert.NotNull(vendorServicesProperties.TotalRows);
			Assert.True(vendorServicesProperties.TotalRows == vendorServicesProperties.Records.Count
				, "vendorServicesProperties.TotalRows != vendorServicesProperties.Records.Count");
			Assert.Null(vendorServicesProperties.Additional);
			Assert.NotNull(vendorServicesProperties.IsError);
			Assert.False(vendorServicesProperties.IsError, "vendorServicesProperties.IsError is true.");
			Assert.NotNull(vendorServicesProperties.ErrorReason);
			Assert.True(vendorServicesProperties.ErrorReason.Length == 0, "vendorServicesProperties.ErrorReason.Length > 0.");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetProductVendorServicesPropertiesNoEditorPersonaIdAndUserPersonaId", "The request is invalid.")]
		[InlineData("GetProductVendorServicesPropertiesValidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "ValidPersonaId")]
		[InlineData("GetProductVendorServicesPropertiesInvalidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "InvalidPersonaId")]
		[InlineData("GetProductVendorServicesPropertiesNoEditorPersonaIdAndValidUserPersonaId", "The request is invalid.", "", "ValidPersonaId")]
		[InlineData("GetProductVendorServicesPropertiesNoEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "", "InvalidPersonaId")]
		[InlineData("GetProductVendorServicesPropertiesInvalidEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "InvalidPersonaId", "InvalidPersonaId")]
		[InlineData("GetProductVendorServicesPropertiesZeroEditorPersonaIdAndZeroUserPersonaId", "editorPersonaId not supplied.", "0", "0")]
		[InlineData("GetProductVendorServicesPropertiesInvalidEditorPersonaIdAndZeroUserPersonaId", "The request is invalid.", "InvalidPersonaId", "0")]
		[InlineData("GetProductVendorServicesPropertiesZeroEditorPersonaIdAndValidUserPersonaId", "editorPersonaId not supplied.", "0", "ValidPersonaId")]
		[InlineData("GetProductVendorServicesPropertiesZeroEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "0", "InvalidPersonaId")]
		public void GetProductVendorServicesPropertiesNegativeCases(string testCase, string errorReason, string editorPersonaId = "", string userPersonaId = "")
		{
			// Set up the API URL
			editorPersonaId = editorPersonaId == "ValidPersonaId" ? personaId : editorPersonaId;
			userPersonaId = userPersonaId == "ValidPersonaId" ? personaId : userPersonaId;
			EndPointUrl = HostUrl + Properties["ProductVendorServices"]
				+ "properties?editorPersonaId=" + editorPersonaId + "&userPersonaId=" + userPersonaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest != ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains(errorReason), $"ResponseString is not \"{errorReason}\"");
		}
		
		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetProductVendorServicesPropertyGroups")]
		[InlineData("GetProductVendorServicesPropertyGroupsPostNewUser")]
		[InlineData("GetProductVendorServicesPropertyGroupsPostNewUserWithoutEmail", "Regular User (No Email)")]
		[InlineData("GetProductVendorServicesPropertyGroupsPostNewUserSuperUser", "RealPage System Administrator")]
		[InlineData("GetProductVendorServicesPropertyGroupsPutUserProfileDetail")]
		[InlineData("GetProductVendorServicesPropertyGroupsPutUserProfileDetailWithoutEmail", "Regular User (No Email)")]
		public void GetProductVendorServicesPropertyGroupsHappyPaths(string testCase, string userType = "Regular User")
		{
			// Set up the API URL
			userTypeId = reusable.DoGetUserRoleTypeId(userType);
			List<PropertyGroup> vendorServicesPropertyGroupRecordsForAddUser = new List<PropertyGroup>();
			if (testCase == "GetProductVendorServicesPropertyGroups")
			{
				userPersonaId = "0";
			}
			else
			{
				if (testCase.Contains("GetProductVendorServicesPropertyGroupsPostNewUser"))
				{
					newUsername = testCase.Contains("WithoutEmail") ? newUsername.Replace("@ApiTest.com", "") : newUsername;
					userTuple = reusable.createUserWithPropertyAndRole("16", userTypeId, newUsername, HttpVerb.Post, true);
				}
				else if (testCase.Contains("GetProductVendorServicesPropertyGroupsPutUserProfileDetail"))
				{
					if (testCase.Contains("WithoutEmail"))
					{
						userTuple = reusable.createUserWithPropertyAndRole("16", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put);
					}
					else
					{
						userTuple = reusable.createUserWithPropertyAndRole("16", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put);
					}
				}
				userPersonaId = userTuple.Item1;
				expectedProductProperty = userTuple.Item2;
				expectedProductRole = userTuple.Item3;
			}
			EndPointUrl = HostUrl + Properties["ProductVendorServices"] + "propertyGroups?userPersonaId="
				+ userPersonaId + "&editorPersonaId=" + personaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ListResponse vendorServicesPropertyGroups = JsonConvert.DeserializeObject<ListResponse>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(vendorServicesPropertyGroups.Records);
			if (vendorServicesPropertyGroups.Additional.ToString().Contains("propertyGroup"))
			{
				Assert.True(vendorServicesPropertyGroups.Records.Count > 0, "vendorServicesPropertyGroups.Records.Count !> 0");
			}
			else
			{
				Assert.True(vendorServicesPropertyGroups.Records.Count == 0, "vendorServicesPropertyGroups.Records.Count != 0");
			}

			List<PropertyGroup> vendorServicesPropertyGroupRecords = JsonConvert.DeserializeObject<
				List<PropertyGroup>>(JsonConvert.SerializeObject(vendorServicesPropertyGroups.Records));

			foreach (PropertyGroup vendorServicesRole in vendorServicesPropertyGroupRecords)
			{
				Assert.True(vendorServicesRole.Id > 0, "vendorServicesRole.Id !> 0");
				Assert.NotNull(vendorServicesRole.IsAssigned);
				Assert.False(vendorServicesRole.IsAssigned, "vendorServicesRole.IsAssigned is true.");
			}

			Assert.NotNull(vendorServicesPropertyGroups.RowsPerPage);
			Assert.True(vendorServicesPropertyGroups.RowsPerPage > 0, "vendorServicesPropertyGroups.RowsPerPage !> 0");
			Assert.NotNull(vendorServicesPropertyGroups.SkipRows);
			Assert.True(vendorServicesPropertyGroups.SkipRows == 0, "vendorServicesPropertyGroups.SkipRows != 0");
			Assert.NotNull(vendorServicesPropertyGroups.CurrentPage);
			Assert.True(vendorServicesPropertyGroups.CurrentPage == 0, "vendorServicesPropertyGroups.CurrentPage != 0");
			Assert.NotNull(vendorServicesPropertyGroups.TotalPages);
			Assert.True(vendorServicesPropertyGroups.TotalPages > 0, "vendorServicesPropertyGroups.TotalPages !> 0");
			Assert.NotNull(vendorServicesPropertyGroups.TotalRows);
			Assert.True(vendorServicesPropertyGroups.TotalRows == vendorServicesPropertyGroups.Records.Count
				, "vendorServicesPropertyGroups.TotalRows != vendorServicesPropertyGroups.Records.Count");
			if (testCase == "GetProductVendorServicesPropertyGroups")
			{
				Assert.Null(vendorServicesPropertyGroups.Additional);
			}
			else
			{
				Assert.NotNull(vendorServicesPropertyGroups.Additional);
				if (userType == "RealPage System Administrator")
				{
					Assert.True(vendorServicesPropertyGroups.Additional.ToString().Contains("allProperties")
						, "!vendorServicesPropertyGroups.Additional.ToString().Contains(\"allProperties\")");
				}
				else
				{
					Assert.True(vendorServicesPropertyGroups.Additional.ToString().Contains("specificProperties")
						, "!vendorServicesPropertyGroups.Additional.ToString().Contains(\"specificProperties\")");
				}
			}
			Assert.NotNull(vendorServicesPropertyGroups.IsError);
			Assert.False(vendorServicesPropertyGroups.IsError, "vendorServicesPropertyGroups.IsError is true.");
			Assert.NotNull(vendorServicesPropertyGroups.ErrorReason);
			Assert.True(vendorServicesPropertyGroups.ErrorReason.Length == 0, "vendorServicesPropertyGroups.ErrorReason.Length > 0.");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetProductVendorServicesPropertyGroupsNoEditorPersonaIdAndUserPersonaId", "The request is invalid.")]
		[InlineData("GetProductVendorServicesPropertyGroupsValidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "ValidPersonaId")]
		[InlineData("GetProductVendorServicesPropertyGroupsInvalidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "InvalidPersonaId")]
		[InlineData("GetProductVendorServicesPropertyGroupsNoEditorPersonaIdAndValidUserPersonaId", "The request is invalid.", "", "ValidPersonaId")]
		[InlineData("GetProductVendorServicesPropertyGroupsNoEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "", "InvalidPersonaId")]
		[InlineData("GetProductVendorServicesPropertyGroupsInvalidEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "InvalidPersonaId", "InvalidPersonaId")]
		[InlineData("GetProductVendorServicesPropertyGroupsZeroEditorPersonaIdAndZeroUserPersonaId", "editorPersonaId not supplied.", "0", "0")]
		[InlineData("GetProductVendorServicesPropertyGroupsInvalidEditorPersonaIdAndZeroUserPersonaId", "The request is invalid.", "InvalidPersonaId", "0")]
		[InlineData("GetProductVendorServicesPropertyGroupsZeroEditorPersonaIdAndValidUserPersonaId", "editorPersonaId not supplied.", "0", "ValidPersonaId")]
		[InlineData("GetProductVendorServicesPropertyGroupsZeroEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "0", "InvalidPersonaId")]
		public void GetProductVendorServicesPropertyGroupsNegativeCases(string testCase, string errorReason, string editorPersonaId = "", string userPersonaId = "")
		{
			// Set up the API URL
			editorPersonaId = editorPersonaId == "ValidPersonaId" ? personaId : editorPersonaId;
			userPersonaId = userPersonaId == "ValidPersonaId" ? personaId : userPersonaId;
			EndPointUrl = HostUrl + Properties["ProductVendorServices"]
				+ "propertyGroups?editorPersonaId=" + editorPersonaId + "&userPersonaId=" + userPersonaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest != ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains(errorReason), $"ResponseString is not \"{errorReason}\"");
		}

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetProductVendorServicesRoles")]
		[InlineData("GetProductVendorServicesRolesPostNewUser")]
		[InlineData("GetProductVendorServicesRolesPostNewUserWithoutEmail", "Regular User (No Email)")]
		[InlineData("GetProductVendorServicesRolesPostNewUserSuperUser", "RealPage System Administrator")]
		[InlineData("GetProductVendorServicesRolesPutUserProfileDetail")]
		[InlineData("GetProductVendorServicesRolesPutUserProfileDetailWithoutEmail", "Regular User (No Email)")]
		public void GetProductVendorServicesRolesHappyPaths(string testCase, string userType= "Regular User")
		{
			// Set up the API URL
			userTypeId = reusable.DoGetUserRoleTypeId(userType);
			List<ProductRole> vendorServicesRoleRecordsForAddUser = new List<ProductRole>();
			if (testCase == "GetProductVendorServicesRoles")
			{
				userPersonaId = "0";
			}
			else
			{
				if (testCase.Contains("GetProductVendorServicesRolesPostNewUser"))
				{
					newUsername = testCase.Contains("WithoutEmail") ? newUsername.Replace("@ApiTest.com", "") : newUsername;
					userTuple = reusable.createUserWithPropertyAndRole("16", userTypeId, newUsername, HttpVerb.Post, true);
				}
				else if (testCase.Contains("GetProductVendorServicesRolesPutUserProfileDetail"))
				{
					if (testCase.Contains("WithoutEmail"))
					{
						userTuple = reusable.createUserWithPropertyAndRole("16", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put);
					}
					else
					{
						userTuple = reusable.createUserWithPropertyAndRole("16", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put);
					}
				}
				userPersonaId = userTuple.Item1;
				expectedProductProperty = userTuple.Item2;
				expectedProductRole = userTuple.Item3;
			}
			EndPointUrl = HostUrl + Properties["ProductVendorServices"] + "Roles?userPersonaId="
				+ userPersonaId + "&editorPersonaId=" + personaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ListResponse vendorServicesProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(vendorServicesProperties.Records);
			Assert.True(vendorServicesProperties.Records.Count > 0, "vendorServicesProperties.Records.Count !> 0");

			List<ProductRole> vendorServicesRoleRecords = JsonConvert.DeserializeObject<
				List<ProductRole>>(JsonConvert.SerializeObject(vendorServicesProperties.Records));

			foreach (ProductRole vendorServicesRole in vendorServicesRoleRecords)
			{
				Assert.NotNull(vendorServicesRole);
				Assert.True(vendorServicesRole.ID.Length > 0, "vendorServicesRole.ID.Length !> 0");
				Assert.NotNull(vendorServicesRole.Name);
				Assert.True(vendorServicesRole.Name.Length > 0, "vendorServicesRole.Name.Length !> 0");
				Assert.NotNull(vendorServicesRole.Description);
				Assert.True(vendorServicesRole.Description.Length > 0, "vendorServicesRole.Description.Length !> 0");
				Assert.NotNull(vendorServicesRole.IsAssigned);
				if (vendorServicesRole.ID == expectedProductRole || userType == "RealPage System Administrator")
				{
					Assert.True(vendorServicesRole.IsAssigned, "vendorServicesRole.IsAssigned is false.");
				}
				else
				{
					Assert.False(vendorServicesRole.IsAssigned, "vendorServicesRole.IsAssigned is true.");
				}
			}

			Assert.NotNull(vendorServicesProperties.RowsPerPage);
			Assert.True(vendorServicesProperties.RowsPerPage > 0, "vendorServicesProperties.RowsPerPage !> 0");
			Assert.NotNull(vendorServicesProperties.SkipRows);
			Assert.True(vendorServicesProperties.SkipRows == 0, "vendorServicesProperties.SkipRows != 0");
			Assert.NotNull(vendorServicesProperties.CurrentPage);
			Assert.True(vendorServicesProperties.CurrentPage == 0, "vendorServicesProperties.CurrentPage != 0");
			Assert.NotNull(vendorServicesProperties.TotalPages);
			Assert.True(vendorServicesProperties.TotalPages > 0, "vendorServicesProperties.TotalPages !> 0");
			Assert.NotNull(vendorServicesProperties.TotalRows);
			Assert.True(vendorServicesProperties.TotalRows == vendorServicesProperties.Records.Count
				, "vendorServicesProperties.TotalRows != vendorServicesProperties.Records.Count");
			Assert.Null(vendorServicesProperties.Additional);
			Assert.NotNull(vendorServicesProperties.IsError);
			Assert.False(vendorServicesProperties.IsError, "vendorServicesProperties.IsError is true.");
			Assert.NotNull(vendorServicesProperties.ErrorReason);
			Assert.True(vendorServicesProperties.ErrorReason.Length == 0, "vendorServicesProperties.ErrorReason.Length > 0.");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetProductVendorServicesRolesNoEditorPersonaIdAndUserPersonaId", "The request is invalid.")]
		[InlineData("GetProductVendorServicesRolesValidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "ValidPersonaId")]
		[InlineData("GetProductVendorServicesRolesInvalidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "InvalidPersonaId")]
		[InlineData("GetProductVendorServicesRolesNoEditorPersonaIdAndValidUserPersonaId", "The request is invalid.", "", "ValidPersonaId")]
		[InlineData("GetProductVendorServicesRolesNoEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "", "InvalidPersonaId")]
		[InlineData("GetProductVendorServicesRolesInvalidEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "InvalidPersonaId", "InvalidPersonaId")]
		[InlineData("GetProductVendorServicesRolesZeroEditorPersonaIdAndZeroUserPersonaId", "editorPersonaId not supplied.", "0", "0")]
		[InlineData("GetProductVendorServicesRolesInvalidEditorPersonaIdAndZeroUserPersonaId", "The request is invalid.", "InvalidPersonaId", "0")]
		[InlineData("GetProductVendorServicesRolesZeroEditorPersonaIdAndValidUserPersonaId", "editorPersonaId not supplied.", "0", "ValidPersonaId")]
		[InlineData("GetProductVendorServicesRolesZeroEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "0", "InvalidPersonaId")]
		public void GetProductVendorServicesRolesNegativeCases(string testCase, string errorReason, string editorPersonaId = "", string userPersonaId = "")
		{
			// Set up the API URL
			editorPersonaId = editorPersonaId == "ValidPersonaId" ? personaId : editorPersonaId;
			userPersonaId = userPersonaId == "ValidPersonaId" ? personaId : userPersonaId;
			EndPointUrl = HostUrl + Properties["ProductVendorServices"]
				+ "Roles?editorPersonaId=" + editorPersonaId + "&userPersonaId=" + userPersonaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest != ResponseHttpStatusCode");
			Assert.NotNull(ResponseString);
			Assert.True(ResponseString.Contains(errorReason), $"ResponseString is not \"{errorReason}\"");
		}
	}
}
