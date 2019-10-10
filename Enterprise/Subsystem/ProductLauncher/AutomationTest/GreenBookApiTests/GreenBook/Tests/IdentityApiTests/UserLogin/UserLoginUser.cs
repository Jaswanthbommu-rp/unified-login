using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System.Collections.Generic;
using System.Data;

namespace GreenBook.Tests
{
	public class UserLoginUser : TestController
	{
		public UserLoginUser(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;

			dbManager = new DatabaseController(DbConnString);
		}

		JsonController jsonManager = new JsonController();
		DatabaseController dbManager;
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
		private string loginUsername;
		private DataTable loginUser = new DataTable();

		// UserLoginUser=/api/userlogin/user

		//[Fact, Trait("", "Happy Path")]
		public void GetUserLoginUser()
		{
			// Set up the API URL
			loginUsername = Properties["enterpriseUsername"];
			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "?enterpriseUserName=" + WebUtility.UrlEncode(loginUsername);

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponseIdentity(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			UserLogin userLoginUserResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Get Expected Response from DB
			UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(loginUsername));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(userLoginUserResponse.UserId);
			Assert.True(userLoginUserResponse.UserId == expectedUserLoginUser.UserId, "userLoginUserResponse.UserId == expectedUserLoginUser.UserId");
			Assert.NotNull(userLoginUserResponse.PartyId);
			Assert.True(userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId");
			Assert.NotNull(userLoginUserResponse.RealPageId);
			Assert.True(userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId");
			Assert.NotNull(userLoginUserResponse.LoginName);
			Assert.True(userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName");

			if (userLoginUserResponse.LoginNameType != null)
			{
				Assert.NotNull(userLoginUserResponse.LoginNameType);
				Assert.True(userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
			}

			Assert.NotNull(userLoginUserResponse.IsActive);
			Assert.True(userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive");
			Assert.NotNull(userLoginUserResponse.IsLocked);
			Assert.True(userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked");
			Assert.NotNull(userLoginUserResponse.IsTainted);
			Assert.True(userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted");

			if (userLoginUserResponse.PasswordModifiedDate != null)
			{
				Assert.NotNull(userLoginUserResponse.PasswordModifiedDate);
				Assert.True(userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
			}
			if (userLoginUserResponse.FromDate != null)
			{
				Assert.NotNull(userLoginUserResponse.FromDate);
				Assert.True(userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
			}
			if (userLoginUserResponse.ThruDate != null)
			{
				Assert.NotNull(userLoginUserResponse.ThruDate);
				Assert.True(userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
			}

			Assert.NotNull(userLoginUserResponse.StatusSetDate);
			Assert.True(userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
			Assert.NotNull(userLoginUserResponse.LastLogin);
			Assert.True(userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");
			
			Assert.True(userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");
			
			//if (userLoginUserResponse.Status != null)
			//{
			//	Assert.NotNull(userLoginUserResponse.Status);
			//	Assert.True(userLoginUserResponse.Status == expectedUserLoginUser.Status, "userLoginUserResponse.Status == expectedUserLoginUser.Status");
			//}
		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetUserLoginUserGoogleIdpUser()
		{
			// Set up the API URL
			loginUser = dbManager.executeQuery("SELECT distinct TOP 1 pcm.partyid, ul.partyid, ul.loginname "
				+ "FROM[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderType] idpt "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSettingType] idpst "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSetting] idps "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[ContactMechanismIdentity] cmid "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[Organization] o "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyRelationship] pr "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[UserLogin] ul "
				+ "on ul.partyid = pr.partyidfrom "
				+ "on pr.partyidto = o.partyid "
				+ "on pcm.partyid = o.partyid "
				+ "on pcm.contactmechanismid = cmid.contactmechanismid "
				+ "on cmid.[IdentityProviderSettingId] = idps.[IdentityProviderSettingId] "
				+ "on idps.identityprovidersettingtypeid = idpst.identityprovidersettingtypeid "
				+ "on idpt.identityprovidertypeid = idpst.identityprovidertypeid "
				+ "where idpt.description = 'Google' ");
			loginUsername = loginUser.Rows[0]["LoginName"].ToString();

			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "?enterpriseUserName=" + loginUsername;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			UserLogin userLoginUserResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Get Expected Response from DB
			UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(loginUsername));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(userLoginUserResponse.UserId);
			Assert.True(userLoginUserResponse.UserId == expectedUserLoginUser.UserId, "userLoginUserResponse.UserId == expectedUserLoginUser.UserId");
			Assert.NotNull(userLoginUserResponse.PartyId);
			Assert.True(userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId");
			Assert.NotNull(userLoginUserResponse.RealPageId);
			Assert.True(userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId");
			Assert.NotNull(userLoginUserResponse.LoginName);
			Assert.True(userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName");
			
			Assert.True(userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
			
			Assert.NotNull(userLoginUserResponse.IsActive);
			Assert.True(userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive");
			Assert.NotNull(userLoginUserResponse.IsLocked);
			Assert.True(userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked");
			Assert.NotNull(userLoginUserResponse.IsTainted);
			Assert.True(userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted");
			
			Assert.True(userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
			
			Assert.True(userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
			
			Assert.True(userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
			
			Assert.NotNull(userLoginUserResponse.StatusSetDate);
			Assert.True(userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
			Assert.NotNull(userLoginUserResponse.LastLogin);
			Assert.True(userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");
			
			Assert.True(userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");
			
			//Assert.True(userLoginUserResponse.Status == expectedUserLoginUser.Status, "userLoginUserResponse.Status == expectedUserLoginUser.Status");
		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetUserLoginUserOktaIdpUser()
		{
			// Set up the API URL
			loginUser = dbManager.executeQuery("SELECT distinct TOP 1 pcm.partyid, ul.partyid, ul.loginname "
				+ "FROM[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderType] idpt "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSettingType] idpst "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSetting] idps "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[ContactMechanismIdentity] cmid "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[Organization] o "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyRelationship] pr "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[UserLogin] ul "
				+ "on ul.partyid = pr.partyidfrom "
				+ "on pr.partyidto = o.partyid "
				+ "on pcm.partyid = o.partyid "
				+ "on pcm.contactmechanismid = cmid.contactmechanismid "
				+ "on cmid.[IdentityProviderSettingId] = idps.[IdentityProviderSettingId] "
				+ "on idps.identityprovidersettingtypeid = idpst.identityprovidersettingtypeid "
				+ "on idpt.identityprovidertypeid = idpst.identityprovidertypeid "
				+ "where idpt.description = 'Okta Provider' ");
			loginUsername = loginUser.Rows[0]["LoginName"].ToString();

			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "?enterpriseUserName=" + loginUsername;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			UserLogin userLoginUserResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Get Expected Response from DB
			UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser("Michael.hart@realpage.com"));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(userLoginUserResponse.UserId);
			Assert.True(userLoginUserResponse.UserId == expectedUserLoginUser.UserId, "userLoginUserResponse.UserId == expectedUserLoginUser.UserId");
			Assert.NotNull(userLoginUserResponse.PartyId);
			Assert.True(userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId");
			Assert.NotNull(userLoginUserResponse.RealPageId);
			Assert.True(userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId");
			Assert.NotNull(userLoginUserResponse.LoginName);
			Assert.True(userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName");

			if (userLoginUserResponse.LoginNameType != null)
			{
				Assert.NotNull(userLoginUserResponse.LoginNameType);
				Assert.True(userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
			}

			Assert.NotNull(userLoginUserResponse.IsActive);
			Assert.True(userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive");
			Assert.NotNull(userLoginUserResponse.IsLocked);
			Assert.True(userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked");
			Assert.NotNull(userLoginUserResponse.IsTainted);
			Assert.True(userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted");

			if (userLoginUserResponse.PasswordModifiedDate != null)
			{
				Assert.NotNull(userLoginUserResponse.PasswordModifiedDate);
				Assert.True(userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
			}
			if (userLoginUserResponse.FromDate != null)
			{
				Assert.NotNull(userLoginUserResponse.FromDate);
				Assert.True(userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
			}
			if (userLoginUserResponse.ThruDate != null)
			{
				Assert.NotNull(userLoginUserResponse.ThruDate);
				Assert.True(userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
			}

			Assert.NotNull(userLoginUserResponse.StatusSetDate);
			Assert.True(userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
			Assert.NotNull(userLoginUserResponse.LastLogin);
			Assert.True(userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");
			
			Assert.True(userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");
			
			//if (userLoginUserResponse.Status != null)
			//{
			//	Assert.NotNull(userLoginUserResponse.Status);
			//	Assert.True(userLoginUserResponse.Status == expectedUserLoginUser.Status, "userLoginUserResponse.Status == expectedUserLoginUser.Status");
			//}
		}

		//[Fact, Trait("", "Data-Driven")]
		public void GetUserLoginUserAzureIdpUser()
		{
			// Set up the API URL
			loginUser = 
				dbManager.executeQuery("SELECT TOP 1 pcm.partyid, ul.partyid, ul.loginname "
				+ "FROM[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderType] idpt "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSettingType] idpst "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSetting] idps "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[ContactMechanismIdentity] cmid "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[Organization] o "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyRelationship] pr "
				+ "inner join[identity].[Ident].[UserLogin] ul "
				+ "on ul.partyid = pr.partyidfrom "
				+ "on pr.partyidto = o.partyid "
				+ "on pcm.partyid = o.partyid "
				+ "on pcm.contactmechanismid = cmid.contactmechanismid "
				+ "on cmid.[IdentityProviderSettingId] = idps.[IdentityProviderSettingId] "
				+ "on idps.identityprovidersettingtypeid = idpst.identityprovidersettingtypeid "
				+ "on idpt.identityprovidertypeid = idpst.identityprovidertypeid "
				+ "where idpt.description = 'Azure AD' ");
			loginUsername = loginUser.Rows[0]["LoginName"].ToString();

			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "?enterpriseUserName=" + loginUsername;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			UserLogin userLoginUserResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Get Expected Response from DB
			UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser("mhhart324@RPAuth.onmicrosoft.com"));

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(userLoginUserResponse.UserId);
			Assert.True(userLoginUserResponse.UserId == expectedUserLoginUser.UserId, "userLoginUserResponse.UserId == expectedUserLoginUser.UserId");
			Assert.NotNull(userLoginUserResponse.PartyId);
			Assert.True(userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId");
			Assert.NotNull(userLoginUserResponse.RealPageId);
			Assert.True(userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId");
			Assert.NotNull(userLoginUserResponse.LoginName);
			Assert.True(userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName");

			if (userLoginUserResponse.LoginNameType != null)
			{
				Assert.NotNull(userLoginUserResponse.LoginNameType);
				Assert.True(userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
			}

			Assert.NotNull(userLoginUserResponse.IsActive);
			Assert.True(userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive");
			Assert.NotNull(userLoginUserResponse.IsLocked);
			Assert.True(userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked");
			Assert.NotNull(userLoginUserResponse.IsTainted);
			Assert.True(userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted");

			if (userLoginUserResponse.PasswordModifiedDate != null)
			{
				Assert.NotNull(userLoginUserResponse.PasswordModifiedDate);
				Assert.True(userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
			}
			if (userLoginUserResponse.FromDate != null)
			{
				Assert.NotNull(userLoginUserResponse.FromDate);
				Assert.True(userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
			}
			if (userLoginUserResponse.ThruDate != null)
			{
				Assert.NotNull(userLoginUserResponse.ThruDate);
				Assert.True(userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
			}

			Assert.NotNull(userLoginUserResponse.StatusSetDate);
			Assert.True(userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
			Assert.NotNull(userLoginUserResponse.LastLogin);
			Assert.True(userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");
			
			Assert.True(userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");
			
			//if (userLoginUserResponse.Status != null)
			//{
			//	Assert.NotNull(userLoginUserResponse.Status);
			//	Assert.True(userLoginUserResponse.Status == expectedUserLoginUser.Status, "userLoginUserResponse.Status == expectedUserLoginUser.Status");
			//}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetUserLoginUserInvalidUsername()
		{
			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "?enterpriseUserName=invalidUsername";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.NoContent == ResponseHttpStatusCode, "HttpStatusCode.NoContent == ResponseHttpStatusCode");
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetUserLoginUserNoUsername()
		{
			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "?enterpriseUserName=";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.True(ResponseString == "\"Invalid parameter: enterpriseUserName\"", "ResponseString == \"\"Invalid parameter: enterpriseUserName\"\"");
		}

		//[Fact, Trait("", "Happy Path")]
		public void GetUserLoginUserRealPageId()
		{
			// Get Expected Response from DB
			UserLogin expectedUserLoginUser = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser));

			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "/" + expectedUserLoginUser.RealPageId;

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			UserLogin userLoginUserResponse = JsonConvert.DeserializeObject<UserLogin>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
			
			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(userLoginUserResponse.UserId);
			Assert.True(userLoginUserResponse.UserId == expectedUserLoginUser.UserId, "userLoginUserResponse.UserId == expectedUserLoginUser.UserId");
			Assert.NotNull(userLoginUserResponse.PartyId);
			Assert.True(userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId, "userLoginUserResponse.PartyId == expectedUserLoginUser.PartyId");
			Assert.NotNull(userLoginUserResponse.RealPageId);
			Assert.True(userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId, "userLoginUserResponse.RealPageId == expectedUserLoginUser.RealPageId");
			Assert.NotNull(userLoginUserResponse.LoginName);
			Assert.True(userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName, "userLoginUserResponse.LoginName == expectedUserLoginUser.LoginName");

			if (userLoginUserResponse.LoginNameType != null)
			{
				Assert.NotNull(userLoginUserResponse.LoginNameType);
				Assert.True(userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType, "userLoginUserResponse.LoginNameType == expectedUserLoginUser.LoginNameType");
			}

			Assert.NotNull(userLoginUserResponse.IsActive);
			Assert.True(userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive, "userLoginUserResponse.IsActive == expectedUserLoginUser.IsActive");
			Assert.NotNull(userLoginUserResponse.IsLocked);
			Assert.True(userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked, "userLoginUserResponse.IsLocked == expectedUserLoginUser.IsLocked");
			Assert.NotNull(userLoginUserResponse.IsTainted);
			Assert.True(userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted, "userLoginUserResponse.IsTainted == expectedUserLoginUser.IsTainted");

			if (userLoginUserResponse.PasswordModifiedDate != null)
			{
				Assert.NotNull(userLoginUserResponse.PasswordModifiedDate);
				Assert.True(userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString()), "userLoginUserResponse.PasswordModifiedDate.ToString().Equals(expectedUserLoginUser.PasswordModifiedDate.ToString())");
			}
			if (userLoginUserResponse.FromDate != null)
			{
				Assert.NotNull(userLoginUserResponse.FromDate);
				Assert.True(userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString()), "userLoginUserResponse.FromDate.ToString().Equals(expectedUserLoginUser.FromDate.ToString())");
			}
			if (userLoginUserResponse.ThruDate != null)
			{
				Assert.NotNull(userLoginUserResponse.ThruDate);
				Assert.True(userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString()), "userLoginUserResponse.ThruDate.ToString().Equals(expectedUserLoginUser.ThruDate.ToString())");
			}

			Assert.NotNull(userLoginUserResponse.StatusSetDate);
			Assert.True(userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString()), "userLoginUserResponse.StatusSetDate.ToString().Equals(expectedUserLoginUser.StatusSetDate.ToString())");
			Assert.NotNull(userLoginUserResponse.LastLogin);
			Assert.True(userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString()), "userLoginUserResponse.LastLogin.ToString().Equals(expectedUserLoginUser.LastLogin.ToString())");
			
			Assert.True(userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser, "userLoginUserResponse.IsSuperUser == expectedUserLoginUser.IsSuperUser");
			
			//if (userLoginUserResponse.Status != null)
			//{
			//	Assert.NotNull(userLoginUserResponse.Status);
			//	Assert.True(userLoginUserResponse.Status == expectedUserLoginUser.Status, "userLoginUserResponse.Status == expectedUserLoginUser.Status");
			//}
		}

		//[Fact, Trait("", "Negative Case")]
		public void GetUserLoginUserInvalidRealPageId()
		{
			// Set up the API URL
			loginUsername = Properties["enterpriseUsername"];
			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "?enterpriseUserName=" + WebUtility.UrlEncode(loginUsername);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "/invalidRealPageId";

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
			Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
		}
	}
}
