using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using System.Collections.Generic;
using System;

namespace GreenBook.Tests.LandingApiTests.ProductClientPortal
{
	public class ProductClientPortal : TestController
	{
		public ProductClientPortal(ITestOutputHelper _xUnitTestOutput)
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
		
		// ProductClientPortal=/api/products/clientportal

		//[Theory]
		//[Trait("Data-Driven", "Happy Path")]
		[InlineData("GetProductClientPortalProperties")]
		[InlineData("GetProductClientPortalPropertiesPostNewUser")]
		[InlineData("GetProductClientPortalPropertiesPostNewUserWithoutEmail", "Regular User (No Email)")]
		[InlineData("GetProductClientPortalPropertiesPostNewUserSuperUser", "RealPage System Administrator")]
		[InlineData("GetProductClientPortalPropertiesPutUserProfileDetail")]
		[InlineData("GetProductClientPortalPropertiesPutUserProfileDetailWithoutEmail", "Regular User (No Email)")]
		public void GetProductClientPortalPropertiesHappyPaths(string testCase, string userType = "Regular User")
		{
			// Set up the API URL
			List<ProductProperty> clientPortalPropertyRecordsForAddUser = new List<ProductProperty>();
			userTypeId = reusable.DoGetUserRoleTypeId(userType);
			if (testCase == "GetProductClientPortalProperties")
			{
				userPersonaId = "0";
			}
			else
			{
				if (testCase.Contains("GetProductClientPortalPropertiesPostNewUser"))
				{
					newUsername = testCase.Contains("WithoutEmail") ? newUsername.Replace("@ApiTest.com", "") : newUsername;
					userTuple = reusable.createUserWithPropertyAndRole("14", userTypeId, newUsername, HttpVerb.Post);
				}
				else if (testCase.Contains("GetProductClientPortalPropertiesPutUserProfileDetail"))
				{
					if (testCase.Contains("WithoutEmail"))
					{
						userTuple = reusable.createUserWithPropertyAndRole("14", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put);
					}
					else
					{
						userTuple = reusable.createUserWithPropertyAndRole("14", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put);
					}
				}
				userPersonaId = userTuple.Item1;
				expectedProductProperty = userTuple.Item2;
				expectedProductRole = userTuple.Item3;
			}
			EndPointUrl = HostUrl + Properties["ProductClientPortal"] + "properties?editorPersonaId=" + personaId
				+ "&userPersonaId=" + userPersonaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ListResponse clientPortalProperties = JsonConvert.DeserializeObject<ListResponse>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(clientPortalProperties.Records);
			Assert.True(clientPortalProperties.Records.Count > 0, "clientPortalProperties.Records.Count !> 0");

			List<ProductProperty> clientPortalPropertyRecords = JsonConvert.DeserializeObject<
				List<ProductProperty>>(JsonConvert.SerializeObject(clientPortalProperties.Records));

			foreach (ProductProperty clientPortalProperty in clientPortalPropertyRecords)
			{
				Assert.NotNull(clientPortalProperty.ID);
				Assert.True(int.Parse(clientPortalProperty.ID.Replace("P", "")) > 0 && clientPortalProperty.ID.StartsWith("P"), "clientPortalProperty.ID.Replace(\"P\", \"\")) > 0 && clientPortalProperty.ID.StartsWith(\"P\")");
				Assert.NotNull(clientPortalProperty.Name);
				Assert.True(clientPortalProperty.Name.Length > 0, "clientPortalProperty.Name.Length !> 0");
				if (!string.IsNullOrEmpty(clientPortalProperty.State))
				{
					Assert.True(clientPortalProperty.State.Length == 2, "clientPortalProperty.State.Length != 2");
				}
				Assert.NotNull(clientPortalProperty.IsAssigned);
				if (clientPortalProperty.ID == expectedProductProperty)
				{
					Assert.True(clientPortalProperty.IsAssigned, "clientPortalProperty.IsAssigned is false.");
				}
				else
				{
					Assert.False(clientPortalProperty.IsAssigned, "clientPortalProperty.IsAssigned is true.");
				}
			}

			Assert.NotNull(clientPortalProperties.RowsPerPage);
			Assert.True(clientPortalProperties.RowsPerPage > 0, "clientPortalProperties.RowsPerPage !> 0");
			Assert.NotNull(clientPortalProperties.SkipRows);
			Assert.True(clientPortalProperties.SkipRows == 0, "clientPortalProperties.SkipRows != 0");
			Assert.NotNull(clientPortalProperties.CurrentPage);
			Assert.True(clientPortalProperties.CurrentPage == 0, "clientPortalProperties.CurrentPage != 0");
			Assert.NotNull(clientPortalProperties.TotalPages);
			Assert.True(clientPortalProperties.TotalPages > 0, "clientPortalProperties.TotalPages !> 0");
			Assert.NotNull(clientPortalProperties.TotalRows);
			Assert.True(clientPortalProperties.TotalRows == clientPortalProperties.Records.Count
				, "clientPortalProperties.TotalRows != clientPortalProperties.Records.Count");
			if (testCase == "GetProductClientPortalProperties")
			{
				Assert.Null(clientPortalProperties.Additional);
			}
			else
			{
				Assert.NotNull(clientPortalProperties.Additional.ToString());
				if (userType == "RealPage System Administrator")
				{
					Assert.True(clientPortalProperties.Additional.ToString().Replace(" ", "").Contains("\"allProperties\":true"));
				}
				else
				{
					Assert.True(clientPortalProperties.Additional.ToString().Replace(" ", "").Contains("\"allProperties\":false"));
				}
			}
			Assert.NotNull(clientPortalProperties.IsError);
			Assert.False(clientPortalProperties.IsError, "clientPortalProperties.IsError is true.");
			Assert.NotNull(clientPortalProperties.ErrorReason);
			Assert.True(clientPortalProperties.ErrorReason.Length == 0, "clientPortalProperties.ErrorReason.Length > 0.");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetProductClientPortalPropertiesNoEditorPersonaIdAndUserPersonaId", "The request is invalid.")]
		[InlineData("GetProductClientPortalPropertiesValidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "ValidPersonaId")]
		[InlineData("GetProductClientPortalPropertiesInvalidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "InvalidPersonaId")]
		[InlineData("GetProductClientPortalPropertiesNoEditorPersonaIdAndValidUserPersonaId", "The request is invalid.", "", "ValidPersonaId")]
		[InlineData("GetProductClientPortalPropertiesNoEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "", "InvalidPersonaId")]
		[InlineData("GetProductClientPortalPropertiesInvalidEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "InvalidPersonaId", "InvalidPersonaId")]
		[InlineData("GetProductClientPortalPropertiesZeroEditorPersonaIdAndZeroUserPersonaId", "editorPersonaId not supplied.", "0", "0")]
		[InlineData("GetProductClientPortalPropertiesInvalidEditorPersonaIdAndZeroUserPersonaId", "The request is invalid.", "InvalidPersonaId", "0")]
		[InlineData("GetProductClientPortalPropertiesZeroEditorPersonaIdAndValidUserPersonaId", "editorPersonaId not supplied.", "0", "ValidPersonaId")]
		[InlineData("GetProductClientPortalPropertiesZeroEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "0", "InvalidPersonaId")]
		public void GetProductClientPortalPropertiesNegativeCases(string testCase, string errorReason, string editorPersonaId = "", string userPersonaId = "")
		{
			// Set up the API URL
			editorPersonaId = editorPersonaId == "ValidPersonaId" ? personaId : editorPersonaId;
			userPersonaId = userPersonaId == "ValidPersonaId" ? personaId : userPersonaId;
			EndPointUrl = HostUrl + Properties["ProductClientPortal"]
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
		[InlineData("GetProductClientPortalRoles")]
		[InlineData("GetProductClientPortalRolesPostNewUser")]
		[InlineData("GetProductClientPortalRolesPostNewUserWithoutEmail", "Regular User (No Email)")]
		[InlineData("GetProductClientPortalRolesPostNewUserSuperUser", "RealPage System Administrator")]
		[InlineData("GetProductClientPortalRolesPutUserProfileDetail")]
		[InlineData("GetProductClientPortalRolesPutUserProfileDetailWithoutEmail", "Regular User (No Email)")]
		public void GetProductClientPortalRolesHappyPaths(string testCase, string userType = "Regular User")
		{
			// Set up the API URL
			List<ProductRole> clientPortalRoleRecordsForAddUser = new List<ProductRole>();
			userTypeId = reusable.DoGetUserRoleTypeId(userType);
			if (testCase == "GetProductClientPortalRoles")
			{
				userPersonaId = "0";
			}
			else
			{
				if (testCase.Contains("GetProductClientPortalRolesPostNewUser"))
				{
					newUsername = testCase.Contains("WithoutEmail") ? newUsername.Replace("@ApiTest.com", "") : newUsername;
					userTuple = reusable.createUserWithPropertyAndRole("14", userTypeId, newUsername, HttpVerb.Post);
				}
				else if (testCase.Contains("GetProductClientPortalRolesPutUserProfileDetail"))
				{
					if (testCase.Contains("WithoutEmail"))
					{
						userTuple = reusable.createUserWithPropertyAndRole("14", userTypeId, Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put);
					}
					else
					{
						userTuple = reusable.createUserWithPropertyAndRole("14", userTypeId, Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put);
					}
				}
				userPersonaId = userTuple.Item1;
				expectedProductProperty = userTuple.Item2;
				expectedProductRole = userTuple.Item3;
			}
			EndPointUrl = HostUrl + Properties["ProductClientPortal"] + "Roles?userPersonaId="
				+ userPersonaId + "&editorPersonaId=" + personaId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

			// Extract API's JSON Response
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			ListResponse clientPortalRoles = JsonConvert.DeserializeObject<ListResponse>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(clientPortalRoles.Records);
			Assert.True(clientPortalRoles.Records.Count > 0, "clientPortalProperties.Records.Count !> 0");

			List<ProductRole> clientPortalRoleRecords = JsonConvert.DeserializeObject<
				List<ProductRole>>(JsonConvert.SerializeObject(clientPortalRoles.Records));

			foreach (ProductRole clientPortalRole in clientPortalRoleRecords)
			{
				Assert.NotNull(clientPortalRole);
				Assert.True(clientPortalRole.ID.Length > 0, "clientPortalRole.ID.Length !> 0");
				Assert.NotNull(clientPortalRole.Name);
				Assert.True(clientPortalRole.Name.Length > 0, "clientPortalRole.Name.Length !> 0");
				Assert.NotNull(clientPortalRole.IsAssigned);
				if (clientPortalRole.ID == expectedProductRole || (userType == "RealPage System Administrator" && clientPortalRole.Name == "Client Portal Administrator"))
				{
					Assert.True(clientPortalRole.IsAssigned, "clientPortalRole.IsAssigned is false.");
				}
				else
				{
					Assert.False(clientPortalRole.IsAssigned, "clientPortalRole.IsAssigned is true.");
				}
			}

			Assert.NotNull(clientPortalRoles.RowsPerPage);
			Assert.True(clientPortalRoles.RowsPerPage > 0, "clientPortalProperties.RowsPerPage !> 0");
			Assert.NotNull(clientPortalRoles.SkipRows);
			Assert.True(clientPortalRoles.SkipRows == 0, "clientPortalProperties.SkipRows != 0");
			Assert.NotNull(clientPortalRoles.CurrentPage);
			Assert.True(clientPortalRoles.CurrentPage == 0, "clientPortalProperties.CurrentPage != 0");
			Assert.NotNull(clientPortalRoles.TotalPages);
			Assert.True(clientPortalRoles.TotalPages > 0, "clientPortalProperties.TotalPages !> 0");
			Assert.NotNull(clientPortalRoles.TotalRows);
			Assert.True(clientPortalRoles.TotalRows == clientPortalRoles.Records.Count
				, "clientPortalProperties.TotalRows != clientPortalProperties.Records.Count");
			Assert.Null(clientPortalRoles.Additional);
			Assert.NotNull(clientPortalRoles.IsError);
			Assert.False(clientPortalRoles.IsError, "clientPortalProperties.IsError is true.");
			Assert.NotNull(clientPortalRoles.ErrorReason);
			Assert.True(clientPortalRoles.ErrorReason.Length == 0, "clientPortalProperties.ErrorReason.Length > 0.");
		}

		//[Theory]
		//[Trait("Data-Driven", "Negative Case")]
		[InlineData("GetProductClientPortalRolesNoEditorPersonaIdAndUserPersonaId", "The request is invalid.")]
		[InlineData("GetProductClientPortalRolesValidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "ValidPersonaId")]
		[InlineData("GetProductClientPortalRolesInvalidEditorPersonaIdAndNoUserPersonaId", "The request is invalid.", "InvalidPersonaId")]
		[InlineData("GetProductClientPortalRolesNoEditorPersonaIdAndValidUserPersonaId", "The request is invalid.", "", "ValidPersonaId")]
		[InlineData("GetProductClientPortalRolesNoEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "", "InvalidPersonaId")]
		[InlineData("GetProductClientPortalRolesInvalidEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "InvalidPersonaId", "InvalidPersonaId")]
		[InlineData("GetProductClientPortalRolesZeroEditorPersonaIdAndZeroUserPersonaId", "editorPersonaId not supplied.", "0", "0")]
		[InlineData("GetProductClientPortalRolesInvalidEditorPersonaIdAndZeroUserPersonaId", "The request is invalid.", "InvalidPersonaId", "0")]
		[InlineData("GetProductClientPortalRolesZeroEditorPersonaIdAndValidUserPersonaId", "editorPersonaId not supplied.", "0", "ValidPersonaId")]
		[InlineData("GetProductClientPortalRolesZeroEditorPersonaIdAndInvalidUserPersonaId", "The request is invalid.", "0", "InvalidPersonaId")]
		public void GetProductClientPortalRolesNegativeCases(string testCase, string errorReason, string editorPersonaId = "", string userPersonaId = "")
		{
			// Set up the API URL
			editorPersonaId = editorPersonaId == "ValidPersonaId" ? personaId : editorPersonaId;
			userPersonaId = userPersonaId == "ValidPersonaId" ? personaId : userPersonaId;
			EndPointUrl = HostUrl + Properties["ProductClientPortal"]
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
