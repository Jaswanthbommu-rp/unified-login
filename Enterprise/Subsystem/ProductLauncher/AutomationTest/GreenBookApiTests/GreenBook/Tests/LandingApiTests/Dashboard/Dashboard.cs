using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Data;
using GreenBook.Models;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;

namespace GreenBook.Tests
{
	public class Dashboard : TestController
	{
		public Dashboard(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			dashboardApiTestEnterpriseUsername = CurrentlyLoggedInUser;

			// Extract Expected UserLogin JSON Response
			expectedUserLogin = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLogins(JsonConvert.DeserializeObject<UserLogin>(
				reusable.DoGetUserLoginUser(dashboardApiTestEnterpriseUsername)).RealPageId));
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			XunitTestOutPut.WriteLine("\n\nAPI JSON Response:\n" + JsonConvert.SerializeObject(expectedUserLogin));

			// Extract Expected Organizations JSON Response
			EndPointUrl = HostUrl + Properties["LandingOrganization"] + "Person/" + expectedUserLogin.RealPageId.ToString();
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			expectedOrganizationList = JsonConvert.DeserializeObject<ObjectListOutput<OrganizationTestModel, ErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nAPI JSON Response:\n" + JsonConvert.SerializeObject(expectedOrganizationList));

			// Extract Expected ContactMechanisms JSON Response
			EndPointUrl = HostUrl + Properties["ContactMechanism"].Replace("{realPageId}", expectedUserLogin.RealPageId.ToString());
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			expectedContactMechanismList = JsonConvert.DeserializeObject<ObjectListOutput<CommonAddress, ErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nAPI JSON Response:\n" + JsonConvert.SerializeObject(expectedContactMechanismList));
						
			// Extract Expected LandingOrganizationProducts JSON Response
			EndPointUrl = HostUrl + Properties["Personas"] + "products";
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			expectedPersonaProductList = JsonConvert.DeserializeObject<ObjectListOutput<PersonaProductUserDetails, ErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nAPI JSON Response:\n" + JsonConvert.SerializeObject(expectedPersonaProductList));

			// Extract Expected ProfileDetails JSON Response
			EndPointUrl = HostUrl + Properties["Profiles"] + "details?realPageId=" + expectedUserLogin.RealPageId;
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			expectedProfileDetails = JsonConvert.DeserializeObject<ObjectOutput<ProfileDetailTestModel, ErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nAPI JSON Response:\n" + JsonConvert.SerializeObject(expectedProfileDetails));

			// Extract Expected Resources from LandingOrganizationProducts JSON Response
			EndPointUrl = HostUrl + Properties["Personas"] + "products?productSelectType=ResourcesOnly";
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			expectedPersonaResourceList = JsonConvert.DeserializeObject<ObjectListOutput<PersonaProductUserDetails, ErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nAPI JSON Response:\n" + JsonConvert.SerializeObject(expectedPersonaResourceList));

			// Extract Expected TelecommunicationNumbers JSON Response
			EndPointUrl = HostUrl + Properties["TelecommunicationNumber"].Replace("{realPageId}", expectedUserLogin.RealPageId.ToString());
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			expectedTelecommunicationNumbers = JsonConvert.DeserializeObject<ObjectListOutput<
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber, ErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nAPI JSON Response:\n" + JsonConvert.SerializeObject(expectedTelecommunicationNumbers));
		}

		JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private UserLogin expectedUserLogin;
		private ObjectListOutput<OrganizationTestModel, ErrorData> expectedOrganizationList;
		private ObjectListOutput<CommonAddress, ErrorData> expectedContactMechanismList;
		private ObjectListOutput<PersonaProductUserDetails, ErrorData> expectedPersonaProductList;
		private ObjectOutput<ProfileDetailTestModel, ErrorData> expectedProfileDetails;
		private ObjectListOutput<PersonaProductUserDetails, ErrorData> expectedPersonaResourceList;
		private ObjectListOutput<
			RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.TelecommunicationNumber, ErrorData>
			expectedTelecommunicationNumbers;
		private static string dashboardApiTestEnterpriseUsername;

		// Dashboard=/api/dashboard //

		//[Fact, Trait("", "Happy Path")]
		public void GetDashboard()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Dashboard"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			DashboardElementsResponseTestModel dashboardElementsResponseTestModel
				= JsonConvert.DeserializeObject<DashboardElementsResponseTestModel>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.Avatar != null)
			{
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.Avatar.Length > 0
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.Avatar.Length > 0");
			}
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.UserId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.UserId == expectedUserLogin.UserId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.UserId == expectedUserLogin.UserId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PartyId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PartyId == expectedUserLogin.PartyId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PartyId == expectedUserLogin.PartyId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.RealPageId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.RealPageId == expectedUserLogin.RealPageId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.RealPageId == expectedUserLogin.RealPageId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginName == expectedUserLogin.LoginName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginName == expectedUserLogin.LoginName");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginNameType == expectedUserLogin.LoginNameType
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginNameType == expectedUserLogin.LoginNameType");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsActive);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsActive == expectedUserLogin.IsActive
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsActive == expectedUserLogin.IsActive");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsLocked);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsLocked == expectedUserLogin.IsLocked
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsLocked == expectedUserLogin.IsLocked");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsTainted);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsTainted == expectedUserLogin.IsTainted
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsTainted == expectedUserLogin.IsTainted");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PasswordModifiedDate == expectedUserLogin.PasswordModifiedDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PasswordModifiedDate == expectedUserLogin.PasswordModifiedDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.FromDate);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.FromDate == expectedUserLogin.FromDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.FromDate == expectedUserLogin.FromDate");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.ThruDate);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.ThruDate == expectedUserLogin.ThruDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.ThruDate == expectedUserLogin.ThruDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.StatusSetDate);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.StatusSetDate == expectedUserLogin.StatusSetDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.StatusSetDate == expectedUserLogin.StatusSetDate");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LastLogin);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LastLogin == expectedUserLogin.LastLogin
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LastLogin == expectedUserLogin.LastLogin");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsSuperUser);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsSuperUser == expectedUserLogin.IsSuperUser
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsSuperUser == expectedUserLogin.IsSuperUser");
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.Status == expectedUserLogin.Status
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.Status == expectedUserLogin.Status");

			for (int countOrganizations = 0; countOrganizations < dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization.Count;
				countOrganizations++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].RealPageId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].RealPageId
					== expectedOrganizationList.list[countOrganizations].RealPageId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].RealPageId "
					+ "== expectedOrganizationList.list[countOrganizations].RealPageId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].PartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].PartyId
					== expectedOrganizationList.list[countOrganizations].PartyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].PartyId "
					+ "== expectedOrganizationList.list[countOrganizations].PartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].Name
					== expectedOrganizationList.list[countOrganizations].Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].Name "
					+ "== expectedOrganizationList.list[countOrganizations].Name");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId);
				if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipId)
				{
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId
						== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipId
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipId");
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom);
				if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdFrom)
				{
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom
						== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdFrom
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdFrom");
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId);
				if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId)
				{
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId
						== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId");
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId);
				if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId)
				{
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId
						== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId");
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name);
				if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.Name)
				{
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name
						== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.Name
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.Name");
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo);
				if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdTo)
				{
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo
						== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdTo
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdTo");
				}
				if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId)
				{
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId
						== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId
						== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.Name);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.Name
						== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.Name
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.Name "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.Name");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipTypeId);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipTypeId
						== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipTypeId
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipTypeId "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipTypeId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId
						== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom
						== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo
						== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Name);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Name
						== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Name
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Name "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Name");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Description);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Description
						== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Description
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Description "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Description");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.FromDate);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.FromDate
						== expectedOrganizationList.list[countOrganizations].partyRelationship.FromDate
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.FromDate "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.FromDate");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.ThruDate);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.ThruDate
						== expectedOrganizationList.list[countOrganizations].partyRelationship.ThruDate
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.ThruDate "
						+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.ThruDate");
				}
			}

			for (int countContactMechanisms = 0; countContactMechanisms < dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism.Count;
				countContactMechanisms++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].PartyContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].PartyContactMechanismId
					== expectedContactMechanismList.list[countContactMechanisms].PartyContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].PartyContactMechanismId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].PartyContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].ContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].ContactMechanismId
					== expectedContactMechanismList.list[countContactMechanisms].ContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].ContactMechanismId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].ContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressString);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressString
					== expectedContactMechanismList.list[countContactMechanisms].AddressString
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressString "
					+ "== expectedContactMechanismList.list[countContactMechanisms].AddressString");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressType);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressType
					== expectedContactMechanismList.list[countContactMechanisms].AddressType
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressType "
					+ "== expectedContactMechanismList.list[countContactMechanisms].AddressType");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId
					== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.Name
					== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.Name "
					+ "== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.Name");
			}

			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedRoles);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedRoles == 0
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedRoles == 0");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProducts);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProducts
				== dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts.Count
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProducts "
				+ "== dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts.Count");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProperties);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProperties == 0
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProperties == 0");

			for (int countPersonaProducts = 0; countPersonaProducts < dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts.Count;
				countPersonaProducts++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonaId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonaId
					== expectedPersonaProductList.list[countPersonaProducts].PersonaId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonaId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].PersonaId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationPartyId
					== expectedPersonaProductList.list[countPersonaProducts].OrganizationPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationPartyId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].OrganizationPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationName
					== expectedPersonaProductList.list[countPersonaProducts].OrganizationName
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationName "
					+ "== expectedPersonaProductList.list[countPersonaProducts].OrganizationName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].UserId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].UserId
					== expectedPersonaProductList.list[countPersonaProducts].UserId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].UserId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].UserId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonPartyId
					== expectedPersonaProductList.list[countPersonaProducts].PersonPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonPartyId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].PersonPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TotalAccounts);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TotalAccounts
					== expectedPersonaProductList.list[countPersonaProducts].TotalAccounts
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TotalAccounts "
					+ "== expectedPersonaProductList.list[countPersonaProducts].TotalAccounts");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].MetatagUniqueId
					== expectedPersonaProductList.list[countPersonaProducts].MetatagUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].MetatagUniqueId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].MetatagUniqueId");
				// ProductGUID no longer exists.
				//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductGUID);
				//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductGUID
				//	== expectedPersonaProductList.list[countPersonaProducts].ProductGUID
				//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductGUID "
				//	+ "== expectedPersonaProductList.list[countPersonaProducts].ProductGUID");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductId
					== expectedPersonaProductList.list[countPersonaProducts].ProductId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleUniqueId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleUniqueId
					== expectedPersonaProductList.list[countPersonaProducts].TitleUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleUniqueId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].TitleUniqueId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleId
					== expectedPersonaProductList.list[countPersonaProducts].TitleId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].TitleId");
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClassName
						== expectedPersonaProductList.list[countPersonaProducts].ClassName
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClassName "
						+ "== expectedPersonaProductList.list[countPersonaProducts].ClassName");
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClientId
						== expectedPersonaProductList.list[countPersonaProducts].ClientId
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClientId "
						+ "== expectedPersonaProductList.list[countPersonaProducts].ClientId");
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SettingsUrl
						== expectedPersonaProductList.list[countPersonaProducts].SettingsUrl
						, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SettingsUrl "
						+ "== expectedPersonaProductList.list[countPersonaProducts].SettingsUrl");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductUrl);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductUrl
					== expectedPersonaProductList.list[countPersonaProducts].ProductUrl
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductUrl "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductUrl");
				for (int countProductActivitiesList = 0; countProductActivitiesList < dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList.Count;
					countProductActivitiesList++)
				{
					for (int countMetaGuids = 0; countMetaGuids < dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId.Count;
					countMetaGuids++)
					{
						//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]);
						//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
						//	== expectedPersonaProductList.list[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
						//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids] "
						//	+ "== expectedPersonaProductList.list[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]");
					}
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsNewTab);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsNewTab
					== expectedPersonaProductList.list[countPersonaProducts].IsNewTab
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsNewTab "
					+ "== expectedPersonaProductList.list[countPersonaProducts].IsNewTab");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductName
					== expectedPersonaProductList.list[countPersonaProducts].ProductName
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductName "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductDescription);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductDescription
					== expectedPersonaProductList.list[countPersonaProducts].ProductDescription
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductDescription "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductDescription");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsFavorite);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsFavorite
					== expectedPersonaProductList.list[countPersonaProducts].IsFavorite
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsFavorite "
					+ "== expectedPersonaProductList.list[countPersonaProducts].IsFavorite");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].HasAccess);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].HasAccess
					== expectedPersonaProductList.list[countPersonaProducts].HasAccess
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].HasAccess "
					+ "== expectedPersonaProductList.list[countPersonaProducts].HasAccess");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].FamilyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].FamilyId
					== expectedPersonaProductList.list[countPersonaProducts].FamilyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].FamilyId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].FamilyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Family);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Family
					== expectedPersonaProductList.list[countPersonaProducts].Family
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Family "
					+ "== expectedPersonaProductList.list[countPersonaProducts].Family");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SolutionId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SolutionId
					== expectedPersonaProductList.list[countPersonaProducts].SolutionId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SolutionId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].SolutionId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Solution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Solution
					== expectedPersonaProductList.list[countPersonaProducts].Solution
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Solution "
					+ "== expectedPersonaProductList.list[countPersonaProducts].Solution");
				//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Subsolution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Subsolution
					== expectedPersonaProductList.list[countPersonaProducts].Subsolution
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Subsolution "
					+ "== expectedPersonaProductList.list[countPersonaProducts].Subsolution");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsResource);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsResource
					== expectedPersonaProductList.list[countPersonaProducts].IsResource
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsResource "
					+ "== expectedPersonaProductList.list[countPersonaProducts].IsResource");
				//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].LearnMore);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].LearnMore
					== expectedPersonaProductList.list[countPersonaProducts].LearnMore
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].LearnMore "
					+ "== expectedPersonaProductList.list[countPersonaProducts].LearnMore");
			}

			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyRoleId);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyRoleId == expectedProfileDetails.obj.partyRole.PartyRoleId
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyRoleId == expectedPartyRole.obj.partyRole.PartyRoleId");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyId);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyId == expectedProfileDetails.obj.partyRole.PartyId
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyId == expectedPartyRole.obj.partyRole.PartyId");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.RoleTypeId);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.RoleTypeId == expectedProfileDetails.obj.partyRole.RoleTypeId
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.RoleTypeId == expectedPartyRole.obj.partyRole.RoleTypeId");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.Name);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.Name == expectedProfileDetails.obj.partyRole.Name
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.Name == expectedPartyRole.obj.partyRole.Name");

			for (int countTelecommunicationNumbers = 0; countTelecommunicationNumbers < dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber.Count;
				countTelecommunicationNumbers++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PartyContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PartyContactMechanismId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PartyContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PartyContactMechanismId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PartyContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].CountryCode);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].CountryCode
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].CountryCode
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].CountryCode "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].CountryCode");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].AreaCode);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].AreaCode
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].AreaCode
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].AreaCode "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].AreaCode");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PhoneNumber);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PhoneNumber
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PhoneNumber
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PhoneNumber "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PhoneNumber");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismUsageTypeId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismUsageTypeId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.Name
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.Name "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.Name");
			}

			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PartyId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PartyId == expectedProfileDetails.obj.PartyId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.PartyId == expectedProfileDetails.obj.PartyId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.RealPageId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.RealPageId == expectedProfileDetails.obj.RealPageId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.RealPageId == expectedProfileDetails.obj.RealPageId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.FirstName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.FirstName == expectedProfileDetails.obj.FirstName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.FirstName == expectedProfileDetails.obj.FirstName");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.MiddleName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.MiddleName == expectedProfileDetails.obj.MiddleName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.MiddleName == expectedProfileDetails.obj.MiddleName");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.LastName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.LastName == expectedProfileDetails.obj.LastName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.LastName == expectedProfileDetails.obj.LastName");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.Suffix == expectedProfileDetails.obj.Suffix
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.Suffix == expectedProfileDetails.obj.Suffix");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.Title == expectedProfileDetails.obj.Title
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.Title == expectedProfileDetails.obj.Title");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PreferredContactMethodId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PreferredContactMethodId == expectedProfileDetails.obj.PreferredContactMethodId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.PreferredContactMethodId == expectedProfileDetails.obj.PreferredContactMethodId");

			if (dashboardElementsResponseTestModel.dashboardElements.trainingAchievements != null)
			{
				for (int countTrainingAchievements = 0; countTrainingAchievements < dashboardElementsResponseTestModel.dashboardElements.trainingAchievements.Count;
					countTrainingAchievements++)
				{
					// TODO: No Assertion for this field yet because its class definition is empty and not MVP.
				}
			}

			for (int countPersonaResources = 0; countPersonaResources < dashboardElementsResponseTestModel.dashboardElements.resources.Count;
				countPersonaResources++)
			{
				if (dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonaId
					== expectedPersonaResourceList.list[countPersonaResources].PersonaId)
				{
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonaId);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonaId
						== expectedPersonaResourceList.list[countPersonaResources].PersonaId
						, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].PersonaId "
						+ "== expectedPersonaResourceList.list[countPersonaProducts].PersonaId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationPartyId);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationPartyId
						== expectedPersonaResourceList.list[countPersonaResources].OrganizationPartyId
						, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].OrganizationPartyId "
						+ "== expectedPersonaResourceList.list[countPersonaProducts].OrganizationPartyId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationName);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationName
						== expectedPersonaResourceList.list[countPersonaResources].OrganizationName
						, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].OrganizationName "
						+ "== expectedPersonaResourceList.list[countPersonaProducts].OrganizationName");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].UserId);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].UserId
						== expectedPersonaResourceList.list[countPersonaResources].UserId
						, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].UserId "
						+ "== expectedPersonaResourceList.list[countPersonaProducts].UserId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonPartyId);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonPartyId
						== expectedPersonaResourceList.list[countPersonaResources].PersonPartyId
						, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].PersonPartyId "
						+ "== expectedPersonaResourceList.list[countPersonaProducts].PersonPartyId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TotalAccounts);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TotalAccounts
						== expectedPersonaResourceList.list[countPersonaResources].TotalAccounts
						, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].TotalAccounts "
						+ "== expectedPersonaResourceList.list[countPersonaProducts].TotalAccounts");
					//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].MetatagUniqueId);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].MetatagUniqueId
					//	== expectedPersonaResourceList.list[countPersonaResources].MetatagUniqueId
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].MetatagUniqueId "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].MetatagUniqueId");
					//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductGUID);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductGUID
					//	== expectedPersonaResourceList.list[countPersonaResources].ProductGUID
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductGUID "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductGUID");
					//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductId);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductId
					//	== expectedPersonaResourceList.list[countPersonaResources].ProductId
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductId "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleUniqueId);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleUniqueId
					//	== expectedPersonaResourceList.list[countPersonaResources].TitleUniqueId
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].TitleUniqueId "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].TitleUniqueId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleId);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleId
					//	== expectedPersonaResourceList.list[countPersonaResources].TitleId
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].TitleId "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].TitleId");
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ClassName
					//	== expectedPersonaResourceList.list[countPersonaResources].ClassName
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ClassName "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ClassName");
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ClientId
					//	== expectedPersonaResourceList.list[countPersonaResources].ClientId
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ClientId "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ClientId");
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].SettingsUrl
					//	== expectedPersonaResourceList.list[countPersonaResources].SettingsUrl
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].SettingsUrl "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].SettingsUrl");
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductUrl
					//	== expectedPersonaResourceList.list[countPersonaResources].ProductUrl
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductUrl "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductUrl");
					if (dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList != null)
					{
						for (int countProductActivitiesList = 0; countProductActivitiesList < dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList.Count;
							countProductActivitiesList++)
						{
							for (int countMetaGuids = 0; countMetaGuids < dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId.Count;
							countMetaGuids++)
							{
								Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]);
								//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
								//	== expectedPersonaResourceList.list[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
								//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids] "
								//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]");
							}
						}
					}
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsNewTab);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsNewTab
						== expectedPersonaResourceList.list[countPersonaResources].IsNewTab
						, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].IsNewTab "
						+ "== expectedPersonaResourceList.list[countPersonaProducts].IsNewTab");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductName);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductName
					//	== expectedPersonaResourceList.list[countPersonaResources].ProductName
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductName "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductName");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductDescription);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductDescription
					//	== expectedPersonaResourceList.list[countPersonaResources].ProductDescription
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductDescription "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductDescription");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsFavorite);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsFavorite
					//	== expectedPersonaResourceList.list[countPersonaResources].IsFavorite
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].IsFavorite "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].IsFavorite");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].HasAccess);
					Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].HasAccess
						== expectedPersonaResourceList.list[countPersonaResources].HasAccess
						, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].HasAccess "
						+ "== expectedPersonaResourceList.list[countPersonaProducts].HasAccess");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].FamilyId);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].FamilyId
					//	== expectedPersonaResourceList.list[countPersonaResources].FamilyId
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].FamilyId "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].FamilyId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Family);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Family
					//	== expectedPersonaResourceList.list[countPersonaResources].Family
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].Family "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].Family");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].SolutionId);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].SolutionId
					//	== expectedPersonaResourceList.list[countPersonaResources].SolutionId
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].SolutionId "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].SolutionId");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Solution);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Solution
					//	== expectedPersonaResourceList.list[countPersonaResources].Solution
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].Solution "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].Solution");
					//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Subsolution);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Subsolution
					//	== expectedPersonaResourceList.list[countPersonaResources].Subsolution
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].Subsolution "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].Subsolution");
					Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsResource);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsResource
					//	== expectedPersonaResourceList.list[countPersonaResources].IsResource
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].IsResource "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].IsResource");
					//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].LearnMore);
					//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].LearnMore
					//	== expectedPersonaResourceList.list[countPersonaResources].LearnMore
					//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].LearnMore "
					//	+ "== expectedPersonaResourceList.list[countPersonaProducts].LearnMore");
				}
			}
		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetDashboardWithValidOptionalParameterValues()
		{
			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Dashboard"] + "?realPageId=" + expectedUserLogin.RealPageId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			DashboardElementsResponseTestModel dashboardElementsResponseTestModel
				= JsonConvert.DeserializeObject<DashboardElementsResponseTestModel>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.Avatar != null)
			{
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.Avatar.Length > 0
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.Avatar.Length > 0");
			}
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.UserId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.UserId == expectedUserLogin.UserId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.UserId == expectedUserLogin.UserId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PartyId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PartyId == expectedUserLogin.PartyId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PartyId == expectedUserLogin.PartyId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.RealPageId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.RealPageId == expectedUserLogin.RealPageId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.RealPageId == expectedUserLogin.RealPageId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginName == expectedUserLogin.LoginName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginName == expectedUserLogin.LoginName");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginNameType == expectedUserLogin.LoginNameType
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginNameType == expectedUserLogin.LoginNameType");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsActive);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsActive == expectedUserLogin.IsActive
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsActive == expectedUserLogin.IsActive");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsLocked);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsLocked == expectedUserLogin.IsLocked
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsLocked == expectedUserLogin.IsLocked");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsTainted);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsTainted == expectedUserLogin.IsTainted
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsTainted == expectedUserLogin.IsTainted");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PasswordModifiedDate == expectedUserLogin.PasswordModifiedDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PasswordModifiedDate == expectedUserLogin.PasswordModifiedDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.FromDate);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.FromDate == expectedUserLogin.FromDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.FromDate == expectedUserLogin.FromDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.ThruDate);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.ThruDate == expectedUserLogin.ThruDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.ThruDate == expectedUserLogin.ThruDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.StatusSetDate);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.StatusSetDate == expectedUserLogin.StatusSetDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.StatusSetDate == expectedUserLogin.StatusSetDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LastLogin);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LastLogin == expectedUserLogin.LastLogin
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LastLogin == expectedUserLogin.LastLogin");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsSuperUser);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsSuperUser == expectedUserLogin.IsSuperUser
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsSuperUser == expectedUserLogin.IsSuperUser");
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.Status == expectedUserLogin.Status
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.Status == expectedUserLogin.Status");

			for (int countOrganizations = 0; countOrganizations < dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization.Count;
				countOrganizations++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].RealPageId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].RealPageId
					== expectedOrganizationList.list[countOrganizations].RealPageId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].RealPageId "
					+ "== expectedOrganizationList.list[countOrganizations].RealPageId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].PartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].PartyId
					== expectedOrganizationList.list[countOrganizations].PartyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].PartyId "
					+ "== expectedOrganizationList.list[countOrganizations].PartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].Name
					== expectedOrganizationList.list[countOrganizations].Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].Name "
					+ "== expectedOrganizationList.list[countOrganizations].Name");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.Name");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.Name
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.Name "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.Name");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Name
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Name "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Name");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Description);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Description
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Description
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Description "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Description");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.FromDate);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.FromDate
					== expectedOrganizationList.list[countOrganizations].partyRelationship.FromDate
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.FromDate "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.FromDate");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.ThruDate);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.ThruDate
					== expectedOrganizationList.list[countOrganizations].partyRelationship.ThruDate
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.ThruDate "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.ThruDate");
			}

			for (int countContactMechanisms = 0; countContactMechanisms < dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism.Count;
				countContactMechanisms++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].PartyContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].PartyContactMechanismId
					== expectedContactMechanismList.list[countContactMechanisms].PartyContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].PartyContactMechanismId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].PartyContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].ContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].ContactMechanismId
					== expectedContactMechanismList.list[countContactMechanisms].ContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].ContactMechanismId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].ContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressString);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressString
					== expectedContactMechanismList.list[countContactMechanisms].AddressString
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressString "
					+ "== expectedContactMechanismList.list[countContactMechanisms].AddressString");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressType);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressType
					== expectedContactMechanismList.list[countContactMechanisms].AddressType
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressType "
					+ "== expectedContactMechanismList.list[countContactMechanisms].AddressType");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId
					== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.Name
					== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.Name "
					+ "== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.Name");
			}

			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedRoles);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedRoles == 0
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedRoles == 0");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProducts);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProducts
				== dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts.Count
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProducts "
				+ "== dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts.Count");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProperties);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProperties == 0
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProperties == 0");

			for (int countPersonaProducts = 0; countPersonaProducts < dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts.Count;
				countPersonaProducts++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonaId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonaId
					== expectedPersonaProductList.list[countPersonaProducts].PersonaId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonaId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].PersonaId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationPartyId
					== expectedPersonaProductList.list[countPersonaProducts].OrganizationPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationPartyId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].OrganizationPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationName
					== expectedPersonaProductList.list[countPersonaProducts].OrganizationName
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationName "
					+ "== expectedPersonaProductList.list[countPersonaProducts].OrganizationName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].UserId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].UserId
					== expectedPersonaProductList.list[countPersonaProducts].UserId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].UserId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].UserId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonPartyId
					== expectedPersonaProductList.list[countPersonaProducts].PersonPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonPartyId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].PersonPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TotalAccounts);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TotalAccounts
					== expectedPersonaProductList.list[countPersonaProducts].TotalAccounts
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TotalAccounts "
					+ "== expectedPersonaProductList.list[countPersonaProducts].TotalAccounts");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].MetatagUniqueId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].MetatagUniqueId
					== expectedPersonaProductList.list[countPersonaProducts].MetatagUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].MetatagUniqueId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].MetatagUniqueId");
				//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductGUID);
				//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductGUID
				//	== expectedPersonaProductList.list[countPersonaProducts].ProductGUID
					//, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductGUID "
					//+ "== expectedPersonaProductList.list[countPersonaProducts].ProductGUID");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductId
					== expectedPersonaProductList.list[countPersonaProducts].ProductId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleUniqueId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleUniqueId
					== expectedPersonaProductList.list[countPersonaProducts].TitleUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleUniqueId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].TitleUniqueId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleId
					== expectedPersonaProductList.list[countPersonaProducts].TitleId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].TitleId");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClassName
					== expectedPersonaProductList.list[countPersonaProducts].ClassName
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClassName "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ClassName");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClientId
					== expectedPersonaProductList.list[countPersonaProducts].ClientId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClientId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ClientId");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SettingsUrl
					== expectedPersonaProductList.list[countPersonaProducts].SettingsUrl
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SettingsUrl "
					+ "== expectedPersonaProductList.list[countPersonaProducts].SettingsUrl");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductUrl);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductUrl
					== expectedPersonaProductList.list[countPersonaProducts].ProductUrl
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductUrl "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductUrl");
				for (int countProductActivitiesList = 0; countProductActivitiesList < dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList.Count;
					countProductActivitiesList++)
				{
					for (int countMetaGuids = 0; countMetaGuids < dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId.Count;
					countMetaGuids++)
					{
						Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]);
						Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
							== expectedPersonaProductList.list[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
							, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids] "
							+ "== expectedPersonaProductList.list[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]");
					}
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsNewTab);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsNewTab
					== expectedPersonaProductList.list[countPersonaProducts].IsNewTab
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsNewTab "
					+ "== expectedPersonaProductList.list[countPersonaProducts].IsNewTab");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductName
					== expectedPersonaProductList.list[countPersonaProducts].ProductName
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductName "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductDescription);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductDescription
					== expectedPersonaProductList.list[countPersonaProducts].ProductDescription
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductDescription "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductDescription");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsFavorite);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsFavorite
					== expectedPersonaProductList.list[countPersonaProducts].IsFavorite
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsFavorite "
					+ "== expectedPersonaProductList.list[countPersonaProducts].IsFavorite");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].HasAccess);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].HasAccess
					== expectedPersonaProductList.list[countPersonaProducts].HasAccess
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].HasAccess "
					+ "== expectedPersonaProductList.list[countPersonaProducts].HasAccess");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].FamilyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].FamilyId
					== expectedPersonaProductList.list[countPersonaProducts].FamilyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].FamilyId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].FamilyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Family);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Family
					== expectedPersonaProductList.list[countPersonaProducts].Family
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Family "
					+ "== expectedPersonaProductList.list[countPersonaProducts].Family");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SolutionId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SolutionId
					== expectedPersonaProductList.list[countPersonaProducts].SolutionId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SolutionId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].SolutionId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Solution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Solution
					== expectedPersonaProductList.list[countPersonaProducts].Solution
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Solution "
					+ "== expectedPersonaProductList.list[countPersonaProducts].Solution");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Subsolution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Subsolution
					== expectedPersonaProductList.list[countPersonaProducts].Subsolution
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Subsolution "
					+ "== expectedPersonaProductList.list[countPersonaProducts].Subsolution");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsResource);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsResource
					== expectedPersonaProductList.list[countPersonaProducts].IsResource
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsResource "
					+ "== expectedPersonaProductList.list[countPersonaProducts].IsResource");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].LearnMore);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].LearnMore
					== expectedPersonaProductList.list[countPersonaProducts].LearnMore
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].LearnMore "
					+ "== expectedPersonaProductList.list[countPersonaProducts].LearnMore");
			}

			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyRoleId);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyRoleId == expectedProfileDetails.obj.partyRole.PartyRoleId
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyRoleId == expectedPartyRole.obj.partyRole.PartyRoleId");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyId);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyId == expectedProfileDetails.obj.partyRole.PartyId
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyId == expectedPartyRole.obj.partyRole.PartyId");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.RoleTypeId);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.RoleTypeId == expectedProfileDetails.obj.partyRole.RoleTypeId
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.RoleTypeId == expectedPartyRole.obj.partyRole.RoleTypeId");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.Name);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.Name == expectedProfileDetails.obj.partyRole.Name
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.Name == expectedPartyRole.obj.partyRole.Name");

			for (int countTelecommunicationNumbers = 0; countTelecommunicationNumbers < dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber.Count;
				countTelecommunicationNumbers++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PartyContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PartyContactMechanismId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PartyContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PartyContactMechanismId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PartyContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].CountryCode);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].CountryCode
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].CountryCode
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].CountryCode "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].CountryCode");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].AreaCode);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].AreaCode
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].AreaCode
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].AreaCode "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].AreaCode");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PhoneNumber);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PhoneNumber
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PhoneNumber
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PhoneNumber "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PhoneNumber");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismUsageTypeId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismUsageTypeId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.Name
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.Name "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.Name");
			}

			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PartyId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PartyId == expectedProfileDetails.obj.PartyId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.PartyId == expectedProfileDetails.obj.PartyId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.RealPageId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.RealPageId == expectedProfileDetails.obj.RealPageId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.RealPageId == expectedProfileDetails.obj.RealPageId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.FirstName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.FirstName == expectedProfileDetails.obj.FirstName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.FirstName == expectedProfileDetails.obj.FirstName");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.MiddleName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.MiddleName == expectedProfileDetails.obj.MiddleName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.MiddleName == expectedProfileDetails.obj.MiddleName");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.LastName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.LastName == expectedProfileDetails.obj.LastName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.LastName == expectedProfileDetails.obj.LastName");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.Suffix == expectedProfileDetails.obj.Suffix
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.Suffix == expectedProfileDetails.obj.Suffix");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.Title == expectedProfileDetails.obj.Title
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.Title == expectedProfileDetails.obj.Title");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PreferredContactMethodId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PreferredContactMethodId == expectedProfileDetails.obj.PreferredContactMethodId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.PreferredContactMethodId == expectedProfileDetails.obj.PreferredContactMethodId");

			if (dashboardElementsResponseTestModel.dashboardElements.trainingAchievements != null)
			{
				for (int countTrainingAchievements = 0; countTrainingAchievements < dashboardElementsResponseTestModel.dashboardElements.trainingAchievements.Count;
					countTrainingAchievements++)
				{
					// TODO: No Assertion for this field yet because its class definition is empty and not MVP.
				}
			}

			for (int countPersonaResources = 0; countPersonaResources < dashboardElementsResponseTestModel.dashboardElements.resources.Count;
				countPersonaResources++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonaId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonaId
					== expectedPersonaResourceList.list[countPersonaResources].PersonaId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].PersonaId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].PersonaId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationPartyId
					== expectedPersonaResourceList.list[countPersonaResources].OrganizationPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].OrganizationPartyId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].OrganizationPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationName
					== expectedPersonaResourceList.list[countPersonaResources].OrganizationName
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].OrganizationName "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].OrganizationName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].UserId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].UserId
					== expectedPersonaResourceList.list[countPersonaResources].UserId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].UserId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].UserId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonPartyId
					== expectedPersonaResourceList.list[countPersonaResources].PersonPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].PersonPartyId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].PersonPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TotalAccounts);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TotalAccounts
					== expectedPersonaResourceList.list[countPersonaResources].TotalAccounts
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].TotalAccounts "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].TotalAccounts");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].MetatagUniqueId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].MetatagUniqueId
					== expectedPersonaResourceList.list[countPersonaResources].MetatagUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].MetatagUniqueId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].MetatagUniqueId");
				//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductGUID);
				//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductGUID
				//	== expectedPersonaResourceList.list[countPersonaResources].ProductGUID
				//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductGUID "
				//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductGUID");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductId
					== expectedPersonaResourceList.list[countPersonaResources].ProductId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleUniqueId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleUniqueId
					== expectedPersonaResourceList.list[countPersonaResources].TitleUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].TitleUniqueId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].TitleUniqueId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleId
					== expectedPersonaResourceList.list[countPersonaResources].TitleId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].TitleId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].TitleId");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ClassName
					== expectedPersonaResourceList.list[countPersonaResources].ClassName
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ClassName "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ClassName");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ClientId
					== expectedPersonaResourceList.list[countPersonaResources].ClientId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ClientId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ClientId");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].SettingsUrl
					== expectedPersonaResourceList.list[countPersonaResources].SettingsUrl
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].SettingsUrl "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].SettingsUrl");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductUrl
					== expectedPersonaResourceList.list[countPersonaResources].ProductUrl
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductUrl "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductUrl");
				if (dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList != null)
				{
					for (int countProductActivitiesList = 0; countProductActivitiesList < dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList.Count;
						countProductActivitiesList++)
					{
						for (int countMetaGuids = 0; countMetaGuids < dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId.Count;
						countMetaGuids++)
						{
							Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]);
							Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
								== expectedPersonaResourceList.list[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
								, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids] "
								+ "== expectedPersonaResourceList.list[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]");
						}
					}
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsNewTab);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsNewTab
					== expectedPersonaResourceList.list[countPersonaResources].IsNewTab
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].IsNewTab "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].IsNewTab");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductName
					== expectedPersonaResourceList.list[countPersonaResources].ProductName
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductName "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductDescription);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductDescription
					== expectedPersonaResourceList.list[countPersonaResources].ProductDescription
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductDescription "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductDescription");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsFavorite);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsFavorite
					== expectedPersonaResourceList.list[countPersonaResources].IsFavorite
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].IsFavorite "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].IsFavorite");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].HasAccess);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].HasAccess
					== expectedPersonaResourceList.list[countPersonaResources].HasAccess
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].HasAccess "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].HasAccess");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].FamilyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].FamilyId
					== expectedPersonaResourceList.list[countPersonaResources].FamilyId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].FamilyId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].FamilyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Family);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Family
					== expectedPersonaResourceList.list[countPersonaResources].Family
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].Family "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].Family");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].SolutionId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].SolutionId
					== expectedPersonaResourceList.list[countPersonaResources].SolutionId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].SolutionId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].SolutionId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Solution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Solution
					== expectedPersonaResourceList.list[countPersonaResources].Solution
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].Solution "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].Solution");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Subsolution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Subsolution
					== expectedPersonaResourceList.list[countPersonaResources].Subsolution
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].Subsolution "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].Subsolution");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsResource);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsResource
					== expectedPersonaResourceList.list[countPersonaResources].IsResource
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].IsResource "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].IsResource");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].LearnMore);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].LearnMore
					== expectedPersonaResourceList.list[countPersonaResources].LearnMore
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].LearnMore "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].LearnMore");
			}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetDashboardWithInvalidOptionalParameterValues()
		{
			// Extract Expected Resources from LandingOrganizationProducts JSON Response
			EndPointUrl = HostUrl + Properties["Personas"] + "products?personaId=0&productSelectType=ResourcesOnly";
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
			expectedPersonaResourceList = JsonConvert.DeserializeObject<ObjectListOutput<PersonaProductUserDetails, ErrorData>>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nAPI JSON Response:\n" + JsonConvert.SerializeObject(expectedPersonaResourceList));

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Dashboard"] + "?realPageId=invalidRealPageId&personaId=0";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			DashboardElementsResponseTestModel dashboardElementsResponseTestModel
				= JsonConvert.DeserializeObject<DashboardElementsResponseTestModel>(ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			if (dashboardElementsResponseTestModel.dashboardElements.profileDetail.Avatar != null)
			{
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.Avatar.Length > 0
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.Avatar.Length > 0");
			}
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.UserId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.UserId == expectedUserLogin.UserId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.UserId == expectedUserLogin.UserId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PartyId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PartyId == expectedUserLogin.PartyId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PartyId == expectedUserLogin.PartyId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.RealPageId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.RealPageId == expectedUserLogin.RealPageId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.RealPageId == expectedUserLogin.RealPageId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginName == expectedUserLogin.LoginName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginName == expectedUserLogin.LoginName");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginNameType == expectedUserLogin.LoginNameType
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LoginNameType == expectedUserLogin.LoginNameType");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsActive);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsActive == expectedUserLogin.IsActive
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsActive == expectedUserLogin.IsActive");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsLocked);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsLocked == expectedUserLogin.IsLocked
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsLocked == expectedUserLogin.IsLocked");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsTainted);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsTainted == expectedUserLogin.IsTainted
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsTainted == expectedUserLogin.IsTainted");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PasswordModifiedDate == expectedUserLogin.PasswordModifiedDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.PasswordModifiedDate == expectedUserLogin.PasswordModifiedDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.FromDate);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.FromDate == expectedUserLogin.FromDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.FromDate == expectedUserLogin.FromDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.ThruDate);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.ThruDate == expectedUserLogin.ThruDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.ThruDate == expectedUserLogin.ThruDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.StatusSetDate);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.StatusSetDate == expectedUserLogin.StatusSetDate
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.StatusSetDate == expectedUserLogin.StatusSetDate");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LastLogin);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LastLogin == expectedUserLogin.LastLogin
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.LastLogin == expectedUserLogin.LastLogin");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsSuperUser);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsSuperUser == expectedUserLogin.IsSuperUser
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.IsSuperUser == expectedUserLogin.IsSuperUser");
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.Status == expectedUserLogin.Status
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.userLogin.Status == expectedUserLogin.Status");

			for (int countOrganizations = 0; countOrganizations < dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization.Count;
				countOrganizations++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].RealPageId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].RealPageId
					== expectedOrganizationList.list[countOrganizations].RealPageId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].RealPageId "
					+ "== expectedOrganizationList.list[countOrganizations].RealPageId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].PartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].PartyId
					== expectedOrganizationList.list[countOrganizations].PartyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].PartyId "
					+ "== expectedOrganizationList.list[countOrganizations].PartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].Name
					== expectedOrganizationList.list[countOrganizations].Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].Name "
					+ "== expectedOrganizationList.list[countOrganizations].Name");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyIdTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyIdTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RealPageIdTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RealPageIdTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.PartyRoleTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.ParentPartyRoleTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeFrom.Name "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeFrom.Name");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.RoleTypeIdTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.RoleTypeIdTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.PartyRoleTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.ParentPartyRoleTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.Name
					== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.roleTypeTo.Name "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.roleTypeTo.Name");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.PartyRelationshipTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.PartyRelationshipTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RelationshipTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidFrom");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.RoleTypeIdValidTo");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Name
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Name "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Name");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Description);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Description
					== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Description
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.partyRelationshipType.Description "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.partyRelationshipType.Description");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.FromDate);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.FromDate
					== expectedOrganizationList.list[countOrganizations].partyRelationship.FromDate
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.FromDate "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.FromDate");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.ThruDate);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.ThruDate
					== expectedOrganizationList.list[countOrganizations].partyRelationship.ThruDate
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.organization[countOrganizations].partyRelationship.ThruDate "
					+ "== expectedOrganizationList.list[countOrganizations].partyRelationship.ThruDate");
			}

			for (int countContactMechanisms = 0; countContactMechanisms < dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism.Count;
				countContactMechanisms++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].PartyContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].PartyContactMechanismId
					== expectedContactMechanismList.list[countContactMechanisms].PartyContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].PartyContactMechanismId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].PartyContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].ContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].ContactMechanismId
					== expectedContactMechanismList.list[countContactMechanisms].ContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].ContactMechanismId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].ContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressString);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressString
					== expectedContactMechanismList.list[countContactMechanisms].AddressString
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressString "
					+ "== expectedContactMechanismList.list[countContactMechanisms].AddressString");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressType);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressType
					== expectedContactMechanismList.list[countContactMechanisms].AddressType
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].AddressType "
					+ "== expectedContactMechanismList.list[countContactMechanisms].AddressType");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId
					== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId "
					+ "== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.Name
					== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.contactMechanism[countContactMechanisms].contactMechanismUsageType.Name "
					+ "== expectedContactMechanismList.list[countContactMechanisms].contactMechanismUsageType.Name");
			}

			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedRoles);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedRoles == 0
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedRoles == 0");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProducts);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProducts
				== dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts.Count
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProducts "
				+ "== dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts.Count");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProperties);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProperties == 0
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.SummaryCount.TotalAssignedProperties == 0");

			for (int countPersonaProducts = 0; countPersonaProducts < dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts.Count;
				countPersonaProducts++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonaId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonaId
					== expectedPersonaProductList.list[countPersonaProducts].PersonaId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonaId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].PersonaId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationPartyId
					== expectedPersonaProductList.list[countPersonaProducts].OrganizationPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationPartyId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].OrganizationPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationName
					== expectedPersonaProductList.list[countPersonaProducts].OrganizationName
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].OrganizationName "
					+ "== expectedPersonaProductList.list[countPersonaProducts].OrganizationName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].UserId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].UserId
					== expectedPersonaProductList.list[countPersonaProducts].UserId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].UserId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].UserId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonPartyId
					== expectedPersonaProductList.list[countPersonaProducts].PersonPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].PersonPartyId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].PersonPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TotalAccounts);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TotalAccounts
					== expectedPersonaProductList.list[countPersonaProducts].TotalAccounts
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TotalAccounts "
					+ "== expectedPersonaProductList.list[countPersonaProducts].TotalAccounts");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].MetatagUniqueId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].MetatagUniqueId
					== expectedPersonaProductList.list[countPersonaProducts].MetatagUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].MetatagUniqueId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].MetatagUniqueId");
				//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductGUID);
				//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductGUID
				//	== expectedPersonaProductList.list[countPersonaProducts].ProductGUID
				//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductGUID "
				//	+ "== expectedPersonaProductList.list[countPersonaProducts].ProductGUID");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductId
					== expectedPersonaProductList.list[countPersonaProducts].ProductId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleUniqueId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleUniqueId
					== expectedPersonaProductList.list[countPersonaProducts].TitleUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleUniqueId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].TitleUniqueId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleId
					== expectedPersonaProductList.list[countPersonaProducts].TitleId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].TitleId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].TitleId");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClassName
					== expectedPersonaProductList.list[countPersonaProducts].ClassName
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClassName "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ClassName");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClientId
					== expectedPersonaProductList.list[countPersonaProducts].ClientId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ClientId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ClientId");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SettingsUrl
					== expectedPersonaProductList.list[countPersonaProducts].SettingsUrl
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SettingsUrl "
					+ "== expectedPersonaProductList.list[countPersonaProducts].SettingsUrl");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductUrl);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductUrl
					== expectedPersonaProductList.list[countPersonaProducts].ProductUrl
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductUrl "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductUrl");
				for (int countProductActivitiesList = 0; countProductActivitiesList < dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList.Count;
					countProductActivitiesList++)
				{
					for (int countMetaGuids = 0; countMetaGuids < dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId.Count;
					countMetaGuids++)
					{
						Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]);
						Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
							== expectedPersonaProductList.list[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
							, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids] "
							+ "== expectedPersonaProductList.list[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]");
					}
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsNewTab);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsNewTab
					== expectedPersonaProductList.list[countPersonaProducts].IsNewTab
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsNewTab "
					+ "== expectedPersonaProductList.list[countPersonaProducts].IsNewTab");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductName
					== expectedPersonaProductList.list[countPersonaProducts].ProductName
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductName "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductDescription);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductDescription
					== expectedPersonaProductList.list[countPersonaProducts].ProductDescription
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].ProductDescription "
					+ "== expectedPersonaProductList.list[countPersonaProducts].ProductDescription");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsFavorite);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsFavorite
					== expectedPersonaProductList.list[countPersonaProducts].IsFavorite
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsFavorite "
					+ "== expectedPersonaProductList.list[countPersonaProducts].IsFavorite");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].HasAccess);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].HasAccess
					== expectedPersonaProductList.list[countPersonaProducts].HasAccess
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].HasAccess "
					+ "== expectedPersonaProductList.list[countPersonaProducts].HasAccess");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].FamilyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].FamilyId
					== expectedPersonaProductList.list[countPersonaProducts].FamilyId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].FamilyId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].FamilyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Family);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Family
					== expectedPersonaProductList.list[countPersonaProducts].Family
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Family "
					+ "== expectedPersonaProductList.list[countPersonaProducts].Family");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SolutionId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SolutionId
					== expectedPersonaProductList.list[countPersonaProducts].SolutionId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].SolutionId "
					+ "== expectedPersonaProductList.list[countPersonaProducts].SolutionId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Solution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Solution
					== expectedPersonaProductList.list[countPersonaProducts].Solution
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Solution "
					+ "== expectedPersonaProductList.list[countPersonaProducts].Solution");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Subsolution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Subsolution
					== expectedPersonaProductList.list[countPersonaProducts].Subsolution
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].Subsolution "
					+ "== expectedPersonaProductList.list[countPersonaProducts].Subsolution");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsResource);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsResource
					== expectedPersonaProductList.list[countPersonaProducts].IsResource
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].IsResource "
					+ "== expectedPersonaProductList.list[countPersonaProducts].IsResource");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].LearnMore);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].LearnMore
					== expectedPersonaProductList.list[countPersonaProducts].LearnMore
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.assignedProducts[countPersonaProducts].LearnMore "
					+ "== expectedPersonaProductList.list[countPersonaProducts].LearnMore");
			}

			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyRoleId);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyRoleId == expectedProfileDetails.obj.partyRole.PartyRoleId
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyRoleId == expectedPartyRole.obj.partyRole.PartyRoleId");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyId);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyId == expectedProfileDetails.obj.partyRole.PartyId
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.PartyId == expectedPartyRole.obj.partyRole.PartyId");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.RoleTypeId);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.RoleTypeId == expectedProfileDetails.obj.partyRole.RoleTypeId
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.RoleTypeId == expectedPartyRole.obj.partyRole.RoleTypeId");
			//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.Name);
			//Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.Name == expectedProfileDetails.obj.partyRole.Name
			//	, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.partyRole.Name == expectedPartyRole.obj.partyRole.Name");

			for (int countTelecommunicationNumbers = 0; countTelecommunicationNumbers < dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber.Count;
				countTelecommunicationNumbers++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PartyContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PartyContactMechanismId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PartyContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PartyContactMechanismId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PartyContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].CountryCode);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].CountryCode
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].CountryCode
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].CountryCode "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].CountryCode");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].AreaCode);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].AreaCode
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].AreaCode
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].AreaCode "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].AreaCode");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PhoneNumber);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PhoneNumber
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PhoneNumber
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].PhoneNumber "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].PhoneNumber");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismUsageTypeId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].ContactMechanismUsageTypeId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].ContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.ParentContactMechanismUsageTypeId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.Name);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.Name
					== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.Name
					, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.telecommunicationNumber[countTelecommunicationNumbers].contactMechanismUsageType.Name "
					+ "== expectedTelecommunicationNumbers.list[countTelecommunicationNumbers].contactMechanismUsageType.Name");
			}

			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PartyId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PartyId == expectedProfileDetails.obj.PartyId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.PartyId == expectedProfileDetails.obj.PartyId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.RealPageId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.RealPageId == expectedProfileDetails.obj.RealPageId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.RealPageId == expectedProfileDetails.obj.RealPageId");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.FirstName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.FirstName == expectedProfileDetails.obj.FirstName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.FirstName == expectedProfileDetails.obj.FirstName");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.MiddleName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.MiddleName == expectedProfileDetails.obj.MiddleName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.MiddleName == expectedProfileDetails.obj.MiddleName");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.LastName);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.LastName == expectedProfileDetails.obj.LastName
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.LastName == expectedProfileDetails.obj.LastName");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.Suffix == expectedProfileDetails.obj.Suffix
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.Suffix == expectedProfileDetails.obj.Suffix");
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.Title == expectedProfileDetails.obj.Title
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.Title == expectedProfileDetails.obj.Title");
			Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PreferredContactMethodId);
			Assert.True(dashboardElementsResponseTestModel.dashboardElements.profileDetail.PreferredContactMethodId == expectedProfileDetails.obj.PreferredContactMethodId
				, "dashboardElementsResponseTestModel.dashboardElements.profileDetail.PreferredContactMethodId == expectedProfileDetails.obj.PreferredContactMethodId");

			if (dashboardElementsResponseTestModel.dashboardElements.trainingAchievements != null)
			{
				for (int countTrainingAchievements = 0; countTrainingAchievements < dashboardElementsResponseTestModel.dashboardElements.trainingAchievements.Count;
					countTrainingAchievements++)
				{
					// TODO: No Assertion for this field yet because its class definition is empty and not MVP.
				}
			}

			for (int countPersonaResources = 0; countPersonaResources < dashboardElementsResponseTestModel.dashboardElements.resources.Count;
				countPersonaResources++)
			{
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonaId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonaId
					== expectedPersonaResourceList.list[countPersonaResources].PersonaId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].PersonaId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].PersonaId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationPartyId
					== expectedPersonaResourceList.list[countPersonaResources].OrganizationPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].OrganizationPartyId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].OrganizationPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].OrganizationName
					== expectedPersonaResourceList.list[countPersonaResources].OrganizationName
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].OrganizationName "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].OrganizationName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].UserId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].UserId
					== expectedPersonaResourceList.list[countPersonaResources].UserId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].UserId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].UserId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonPartyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].PersonPartyId
					== expectedPersonaResourceList.list[countPersonaResources].PersonPartyId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].PersonPartyId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].PersonPartyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TotalAccounts);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TotalAccounts
					== expectedPersonaResourceList.list[countPersonaResources].TotalAccounts
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].TotalAccounts "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].TotalAccounts");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].MetatagUniqueId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].MetatagUniqueId
					== expectedPersonaResourceList.list[countPersonaResources].MetatagUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].MetatagUniqueId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].MetatagUniqueId");
				//Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductGUID);
				//Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductGUID
				//	== expectedPersonaResourceList.list[countPersonaResources].ProductGUID
				//	, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductGUID "
				//	+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductGUID");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductId
					== expectedPersonaResourceList.list[countPersonaResources].ProductId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleUniqueId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleUniqueId
					== expectedPersonaResourceList.list[countPersonaResources].TitleUniqueId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].TitleUniqueId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].TitleUniqueId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].TitleId
					== expectedPersonaResourceList.list[countPersonaResources].TitleId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].TitleId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].TitleId");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ClassName
					== expectedPersonaResourceList.list[countPersonaResources].ClassName
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ClassName "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ClassName");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ClientId
					== expectedPersonaResourceList.list[countPersonaResources].ClientId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ClientId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ClientId");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].SettingsUrl
					== expectedPersonaResourceList.list[countPersonaResources].SettingsUrl
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].SettingsUrl "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].SettingsUrl");
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductUrl
					== expectedPersonaResourceList.list[countPersonaResources].ProductUrl
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductUrl "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductUrl");
				if (dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList != null)
				{
					for (int countProductActivitiesList = 0; countProductActivitiesList < dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList.Count;
						countProductActivitiesList++)
					{
						for (int countMetaGuids = 0; countMetaGuids < dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId.Count;
						countMetaGuids++)
						{
							Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]);
							Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
								== expectedPersonaResourceList.list[countPersonaResources].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]
								, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids] "
								+ "== expectedPersonaResourceList.list[countPersonaProducts].ActivitiesList[countProductActivitiesList].MetatagUniqueId[countMetaGuids]");
						}
					}
				}
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsNewTab);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsNewTab
					== expectedPersonaResourceList.list[countPersonaResources].IsNewTab
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].IsNewTab "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].IsNewTab");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductName);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductName
					== expectedPersonaResourceList.list[countPersonaResources].ProductName
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductName "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductName");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductDescription);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].ProductDescription
					== expectedPersonaResourceList.list[countPersonaResources].ProductDescription
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].ProductDescription "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].ProductDescription");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsFavorite);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsFavorite
					== expectedPersonaResourceList.list[countPersonaResources].IsFavorite
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].IsFavorite "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].IsFavorite");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].HasAccess);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].HasAccess
					== expectedPersonaResourceList.list[countPersonaResources].HasAccess
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].HasAccess "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].HasAccess");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].FamilyId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].FamilyId
					== expectedPersonaResourceList.list[countPersonaResources].FamilyId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].FamilyId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].FamilyId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Family);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Family
					== expectedPersonaResourceList.list[countPersonaResources].Family
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].Family "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].Family");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].SolutionId);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].SolutionId
					== expectedPersonaResourceList.list[countPersonaResources].SolutionId
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].SolutionId "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].SolutionId");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Solution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Solution
					== expectedPersonaResourceList.list[countPersonaResources].Solution
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].Solution "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].Solution");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Subsolution);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].Subsolution
					== expectedPersonaResourceList.list[countPersonaResources].Subsolution
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].Subsolution "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].Subsolution");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsResource);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].IsResource
					== expectedPersonaResourceList.list[countPersonaResources].IsResource
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].IsResource "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].IsResource");
				Assert.NotNull(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].LearnMore);
				Assert.True(dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaResources].LearnMore
					== expectedPersonaResourceList.list[countPersonaResources].LearnMore
					, "dashboardElementsResponseTestModel.dashboardElements.resources[countPersonaProducts].LearnMore "
					+ "== expectedPersonaResourceList.list[countPersonaProducts].LearnMore");
			}
		}
	}
}
