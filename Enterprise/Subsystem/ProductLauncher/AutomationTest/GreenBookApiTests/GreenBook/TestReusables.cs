using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using System.Data;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using System.Net;
using System.IO;

namespace GreenBook
{
	class TestReusables : TestBase
	{
		JsonController jsonManager = new JsonController();
		DatabaseController dbManager;

		internal TestReusables()
		{
			dbManager = new DatabaseController(DbConnString);
		}

		/* 
         * A method that returns GET securityQuestionsApiResponse for ForgotPassword flow APIs.
         * INPUT    : 
         * OUTPUT   : GET securityQuestionsApiResponse in string format (or null for any Exception) 
         */
		public string DoGetSecurityQuestionsApiResponseForPayload()
		{
			EndPointUrl = HostUrl + Properties["GetSecurityQuestions"] + WebUtility.UrlEncode(Properties["enterpriseUsername"]);
			EndPointUrl = HostUrl + Properties["GetSecurityQuestions"] + WebUtility.UrlEncode(JsonConvert.DeserializeObject<SecurityQuestionResponse>(
				DoPostNewUserForApiTests(Properties["enterpriseUsername"], EndPointUrl, HttpVerb.Get)).EnterpriseUserName);
			return GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: ""));
		}

		/* 
         * A method that returns the payload for POST /api/credential/ChangePassword.
         * INPUT    : string _EnterpriseUserName, string _ActivityToken, string _Password, string _CorrectAnswerToken, int _PasswordLength = 8
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostChangePasswordPayload(string _GetSecurityQuestionsApiResponse, string _ActivityToken, string _Password, string _CorrectAnswerToken, int _PasswordLength = 8)
		{
			string payload = DoPostVerifySecurityAnswersPayload(_GetSecurityQuestionsApiResponse);

			UserSecurityAnswer verifySecurityAnswersPayload = JsonConvert.DeserializeObject<UserSecurityAnswer>(payload);

			EndPointUrl = HostUrl + Properties["VerifySecurityAnswers"];
			SecurityAnswerResponse verifySecurityAnswersModel = JsonConvert.DeserializeObject<SecurityAnswerResponse>(GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload)));

			ChangePassword changePasswordRequest = new ChangePassword();
			changePasswordRequest.EnterpriseUserName = verifySecurityAnswersPayload.EnterpriseUserName;
			changePasswordRequest.ActivityToken = _ActivityToken != null ? _ActivityToken : verifySecurityAnswersPayload.ActivityToken;
			changePasswordRequest.NewPassword = _Password != null ? _Password : string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(_PasswordLength);
			changePasswordRequest.CorrectAnswerToken = _CorrectAnswerToken != null ? _CorrectAnswerToken : verifySecurityAnswersModel.CorrectAnswerToken;

			return JsonConvert.SerializeObject(changePasswordRequest);
		}

		/* 
         * A method that returns the payload for POST /api/credential/VerifySecurityAnswers.
         * INPUT    : string _EnterpriseUserName = "", string _ActivityToken = ""
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostVerifySecurityAnswersPayload(string _GetSecurityQuestionsApiResponse, string _ActivityToken = "")
		{
			SecurityQuestionResponse getSecurityQuestions = JsonConvert.DeserializeObject<SecurityQuestionResponse>(_GetSecurityQuestionsApiResponse);
			
			if (getSecurityQuestions.ErrorReason == "Max attempts to get security questions exceeded.")
			{
				getSecurityQuestions = JsonConvert.DeserializeObject<SecurityQuestionResponse>(DoResetMaximumActivityAttempts(Properties["enterpriseUsername"], EndPointUrl, HttpVerb.Get));
			}

			UserSecurityAnswer postSecurityAnswer = new UserSecurityAnswer();
			
			postSecurityAnswer.EnterpriseUserName = getSecurityQuestions.EnterpriseUserName;

			if (_ActivityToken == null)
			{
				postSecurityAnswer.ActivityToken = _ActivityToken;
			}
			else if (_ActivityToken.Length > 0)
			{
				postSecurityAnswer.ActivityToken = _ActivityToken.Replace(" ", "");
			}
			else
			{
				postSecurityAnswer.ActivityToken = getSecurityQuestions.ActivityToken;
			}
			
			IList<SecurityQuestionAnswer> postSecurityQuestionAnswerList = new List<SecurityQuestionAnswer>();
			SecurityQuestionAnswer postSecurityQuestionAnswer = new SecurityQuestionAnswer();
			foreach (SecurityQuestion securityQuestion in getSecurityQuestions.SecurityQuestions)
			{
				postSecurityQuestionAnswer.SecurityQuestionId = securityQuestion.SecurityQuestionId;
				postSecurityQuestionAnswer.Answer = "real";
				postSecurityQuestionAnswerList.Add(postSecurityQuestionAnswer);
				postSecurityQuestionAnswer = new SecurityQuestionAnswer();
			}
			postSecurityAnswer.SecurityQuestionAnswers = postSecurityQuestionAnswerList;

			return JsonConvert.SerializeObject(postSecurityAnswer);
		}

		/* 
         * A method that resets the Maximum Activity Attempts 
         * and returns a new string response.
		 * INPUT    : string _EnterpriseUserName, string _EndPointUrl, HttpVerb _HttpVerb, string _JsonPayload
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoResetMaximumActivityAttempts(string _EnterpriseUserName, string _EndPointUrl, HttpVerb _HttpVerb, string _JsonPayload = "", string _ActivityIds = "2, 5, 6")
		{
			DataTable resetAttempts =
				dbManager.executeQuery(string.Format("update [" + Properties["identityDatabase"] 
				+ "].[Ident].[ActivityAttempts] set AttemptCount = 0 where ActivityId in ({1}) "
				+ " and activityattemptsid in (select top 100 activityattemptsid from[" + Properties["identityDatabase"] 
				+ "].[Ident].[ActivityAttempts] where EnterpriseUsername = '{0}' order by lastattemptdatetime desc)"
				+ "\nselect * from[" + Properties["identityDatabase"] + "].[Ident].[ActivityAttempts] where ActivityId in ({1})"
				+ " and activityattemptsid in (select top 100 activityattemptsid from[" + Properties["identityDatabase"] 
				+ "].[Ident].[ActivityAttempts] where EnterpriseUsername = '{0}' order by lastattemptdatetime desc)"
				+ " order by lastattemptdatetime desc", _EnterpriseUserName, _ActivityIds));

			return GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: _HttpVerb, jsonPayload: _JsonPayload));
		}

		/* 
         * A method that creates a NewUser after infinite reset attempts 
         * and returns a new string response.
		 * INPUT    : string _EnterpriseUserName, string _EndPointUrl, HttpVerb _HttpVerb, string _JsonPayload
         * OUTPUT   : JSON Response in string format (or null for any Exception) 
         */
		public string DoPostNewUserForApiTests(string _EnterpriseUserName, string _EndPointUrl, HttpVerb _HttpVerb, string _JsonPayload = "")
		{
			string securityQuestionAnswerPayload = DoPostSetUserSecurityQuestionsPayload();
			EndPointUrl = HostUrl + Properties["SetUserSecurityQuestions"];
			SetUserSecurityQuestionsResponse securityQuestionAnswerResponse
				= JsonConvert.DeserializeObject<SetUserSecurityQuestionsResponse>(GetHttpWebResponseValue(
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: securityQuestionAnswerPayload)));

			if (EndPointUrl.Contains(Properties["VerifySecurityAnswers"]) && _HttpVerb == HttpVerb.Post)
			{
				EndPointUrl = HostUrl + Properties["GetSecurityQuestions"] + WebUtility.UrlEncode(securityQuestionAnswerResponse.EnterpriseUserName);
				string getSecurityQuestionsApiResponse = GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: ""));

				_JsonPayload = DoPostVerifySecurityAnswersPayload(getSecurityQuestionsApiResponse);
			}

			EndPointUrl = _EndPointUrl.Replace(WebUtility.UrlEncode(_EnterpriseUserName), WebUtility.UrlEncode(securityQuestionAnswerResponse.EnterpriseUserName));

			return GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: _HttpVerb, jsonPayload: _JsonPayload));
		}

		/* 
         * A method that returns the payload for POST /api/credential/ValidatePassword.
         * INPUT    : int _OrganizationId, string _EnterpriseUserName, string _Password, int _PasswordLength = 8
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostValidatePasswordPayload(int _OrganizationId, string _EnterpriseUserName, string _Password, int _PasswordLength = 8)
		{
			ValidatePassword validatePasswordRequest = new ValidatePassword();
			//validatePasswordRequest.OrganizationId = _OrganizationId;
			validatePasswordRequest.EnterpriseUserName = _EnterpriseUserName;
			validatePasswordRequest.PasswordToValidate = _Password != null ? _Password : string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(_PasswordLength);

			return JsonConvert.SerializeObject(validatePasswordRequest);
		}

		/* 
         * A method that returns the payload for POST /api/credential/setPassword.
         * INPUT    : string _EnterpriseUserName, string _Password, int _PasswordLength = 8
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostSetPasswordPayload(string _EnterpriseUserName, string _Password, string _ActivityToken, int _PasswordLength = 8)
		{
			SetPassword setPasswordRequest = new SetPassword();
			setPasswordRequest.ActivityToken = _ActivityToken;
			setPasswordRequest.EnterpriseUserName = _EnterpriseUserName;
			setPasswordRequest.NewPassword = _Password != null ? _Password : string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(_PasswordLength);

			return JsonConvert.SerializeObject(setPasswordRequest);
		}

		/* 
         * A method that returns the payload for POST /api/credential/ResetPassword.
         * INPUT    : int _OrganizationId, string _EnterpriseUserName, string _OldPassword, int _PasswordLength = 8
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostResetPasswordPayload(string _OldPassword, string _NewPassword)
		{
			UserResetPassword resetPasswordRequest = new UserResetPassword();
			resetPasswordRequest.OldPassword = _OldPassword != null ? _OldPassword : string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(10);
			resetPasswordRequest.NewPassword = _NewPassword != null ? _NewPassword : string.Concat("P@ssw0rd", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(15);

			return JsonConvert.SerializeObject(resetPasswordRequest);
		}

		/* 
         * A method that gets the PasswordPolicy to be used for testing.
		 * INPUT    : void
         * OUTPUT   : PasswordPolicy JSON object.
         */
		public ObjectOutput<PasswordPolicy, ErrorData> DoGetPasswordPolicy()
		{
			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}"
				, JsonConvert.DeserializeObject<UserLogin>(DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString());
			long portfolioId = JsonConvert.DeserializeObject<List<Organization>>(
					 GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")))[0].PartyId;

			EndPointUrl = HostUrl + Properties["PasswordPolicy"] + portfolioId;

			// Execute API
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			return JsonConvert.DeserializeObject<ObjectOutput<PasswordPolicy, ErrorData>>(responseValue);
		}

		/* 
         * A method that returns the payload for POST and PUT /api/Persons.
         * INPUT    : string _FirstName, string _LastName, string _MiddleName = "", string _Suffix = "", string _Title = ""
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostPutPersonsPayload(string _FirstName, string _LastName, string _MiddleName = "", string _Suffix = "", string _Title = "", HttpVerb _HttpVerb = HttpVerb.Post)
		{
			Person person = new Person();

			if (_HttpVerb == HttpVerb.Post)
			{
				person.PartyId = 0;
				person.FirstName = _FirstName;
				person.LastName = _LastName;
				person.MiddleName = _MiddleName;
				person.Suffix = _Suffix;
				person.Title = _Title;
				person.PreferredContactMethodId = 1;
			}
			else if (_HttpVerb == HttpVerb.Put)
			{
				DataTable party =
					dbManager.executeQuery("SELECT EnterpriseParty.PartyId, EnterpriseParty.RealPageId FROM [" + Properties["identityDatabase"] +
					"].[Enterprise].[Party] EnterpriseParty INNER JOIN [" + Properties["identityDatabase"] + "].[Person].[Person] PersonPerson on EnterpriseParty.PartyId = PersonPerson.PartyId " +
					"where EnterpriseParty.PartyId = (SELECT PartyId from [" + Properties["identityDatabase"] + "].[Ident].[UserLogin] where LoginName = '" + Properties["enterpriseUsername"] + "')");


				person.PartyId = Int64.Parse(party.Rows[0]["PartyId"].ToString());
				person.RealPageId = Guid.Parse(party.Rows[0]["RealPageId"].ToString());
				person.FirstName = _FirstName;
				person.LastName = _LastName;
				person.MiddleName = _MiddleName;
				person.Suffix = _Suffix;
				person.Title = _Title;
				person.PreferredContactMethodId = 1;
			}

			return JsonConvert.SerializeObject(person);
		}

		/* 
         * A method that returns the payload for PUT /api/user/profiledetail .
         * INPUT    : 
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPutUserProfileDetailPayload(string _newLoginName, int _userRole)
		{
			string payload = DoPostEmailNotificationPayload(_newLoginName);

			EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(_newLoginName)
				+ "&newUserRegistrationToken=" + DoPostNewUserToken(payload, _userRole);
			
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			UserLogin newUserLogin = JsonConvert.DeserializeObject<UserLogin>(DoGetUserLoginUser(_newLoginName));
			ProfileDetail profileDetail = JsonConvert.DeserializeObject<ProfileDetail>(payload);
			profileDetail.userLogin.UserId = newUserLogin.UserId;
			profileDetail.userLogin.PartyId = newUserLogin.PartyId;
			profileDetail.userLogin.RealPageId = newUserLogin.RealPageId;
			profileDetail.PartyId = newUserLogin.PartyId;
			profileDetail.RealPageId = newUserLogin.RealPageId;

			return JsonConvert.SerializeObject(profileDetail);
		}

		/* 
         * A method that returns the string-format JSON that has RealPageId for GET and POST /api/persons/{realPageId}/electronicaddress.
         * INPUT    : 
         * OUTPUT   : JSON in string format (or null for any Exception) 
         */
		public string DoGetUserLoginUser(string _EnterpriseUsername)
		{
			EndPointUrl = HostIdentityUrl + Properties["UserLoginUser"] + "?enterpriseUserName=" 
				+ WebUtility.UrlEncode(_EnterpriseUsername);

			EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(
				GetHttpWebResponseValue(GetHttpWebResponse(
					endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: ""))).RealPageId;

			return GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: ""));
		}

		/* 
         * A method that returns the payload for POST /api/log/login
         * INPUT    : 
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostLoginPayload(string _EnterpriseUsername, string _ActivityType)
		{
			ActivityAttempt activityAttempt = new ActivityAttempt();

			activityAttempt.EnterpriseUserName = _EnterpriseUsername;
			activityAttempt.ActivityType = (ActivityType)Enum.Parse(typeof(ActivityType), _ActivityType);

			var request = (HttpWebRequest)WebRequest.Create("http://ifconfig.me");
			request.UserAgent = "curl"; // this simulate curl linux command
			string publicIPAddress;
			request.Method = "GET";
			using (WebResponse response = request.GetResponse())
			{
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					publicIPAddress = reader.ReadToEnd().Replace("\n", "");
				}
			}

			activityAttempt.UserDeviceDetails = new UserDeviceDetails();
			activityAttempt.UserDeviceDetails.IpAddress = publicIPAddress;
			activityAttempt.UserDeviceDetails.BrowserType = "InternetExplorer11";
			activityAttempt.UserDeviceDetails.BrowserName = "InternetExplorer";
			activityAttempt.UserDeviceDetails.Version = "11.0";
			activityAttempt.UserDeviceDetails.Platform = "WinNT";
			activityAttempt.UserDeviceDetails.IsMobile = false;
			activityAttempt.UserDeviceDetails.DeviceType = "DesktopOrLaptop";
			activityAttempt.UserDeviceDetails.Timezone = TimeZoneInfo.Local.DisplayName;
			activityAttempt.UserAgent = null;
			activityAttempt.AuthenticationServiceId = "local";

			return JsonConvert.SerializeObject(activityAttempt);
		}

		/* 
         * A method that returns the Users table data for 
		 * GET /api/IdentityConfig/GetUserByEnterpriseUserNameAndProvider, 
		 * /api/IdentityConfig/GetUserByEnterpriseUserName and /api/IdentityConfig/GetUserByEnterpriseUserId 
         * INPUT    : 
         * OUTPUT   : UserDetails in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectUser()
		{
			DataTable userDetails =
				dbManager.executeQuery("SELECT TOP 1 * from [" + Properties["identityDatabase"] + "].[Ident].[Users] where IdentityProvider like 'IDP%'");

			return userDetails;
		}

		/* 
         * A method that returns the Clients table data for 
		 * GET /api/IdentityConfig/GetClient
         * INPUT    : 
         * OUTPUT   : clientDetails in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectClient()
		{
			DataTable clientDetails =
				dbManager.executeQuery("SELECT TOP 1 * from [" + Properties["identityDatabase"] + "].[Ident].[Clients]");

			return clientDetails;
		}

		/* 
         * A method that returns the ClientSecrets table data for 
		 * GET /api/IdentityConfig/GetClient
         * INPUT    : 
         * OUTPUT   : clientSecrets in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectClientSecrets(string _ClientId)
		{
			DataTable clientSecrets =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[ClientSecrets] WHERE ClientId = " + _ClientId);

			return clientSecrets;
		}

		/* 
         * A method that returns the ClientRedirectUris table data for 
		 * GET /api/IdentityConfig/GetClient
         * INPUT    : 
         * OUTPUT   : clientRedirectUris in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectClientRedirectUris(string _ClientId)
		{
			DataTable clientRedirectUris =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[ClientRedirectUris] WHERE ClientId = " + _ClientId);

			return clientRedirectUris;
		}

		/* 
         * A method that returns the ClientScopes table data for 
		 * GET /api/IdentityConfig/GetClient
         * INPUT    : 
         * OUTPUT   : ClientScopes in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectClientScopes(string _ClientId)
		{
			DataTable clientScopes =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[ClientScopes] WHERE ClientId = " + _ClientId);

			return clientScopes;
		}

		/* 
         * A method that returns the ClientPostLogoutRedirectUris table data for 
		 * GET /api/IdentityConfig/GetClient
         * INPUT    : 
         * OUTPUT   : ClientPostLogoutRedirectUris in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectClientPostLogoutRedirectUris(string _ClientId)
		{
			DataTable clientPostLogoutRedirectUris =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[ClientPostLogoutRedirectUris] WHERE ClientId = " + _ClientId);

			return clientPostLogoutRedirectUris;
		}

		/* 
         * A method that returns the Tokens table data for GET /api/IdentityConfig/GetTokensBySubject 
         * INPUT    : 
         * OUTPUT   : tokenDetails in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectTokens()
		{
			DataTable tokenDetails =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[Tokens] "
				+ "where SubjectCode = (SELECT TOP 1 SubjectCode FROM [" + Properties["identityDatabase"] + "].[Ident].[Tokens]) "
				+ "AND TokenType = (SELECT TOP 1 TokenType FROM [" + Properties["identityDatabase"] + "].[Ident].[Tokens])");

			return tokenDetails;
		}

		/* 
         * A method that returns the Consents table data for GET /api/IdentityConfig/GetConsentsBySubject 
         * INPUT    : 
         * OUTPUT   : consentDetails in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectConsents()
		{
			DataTable consentDetails =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[Consents] "
				+ "where SubjectCode = (SELECT TOP 1 SubjectCode FROM [" + Properties["identityDatabase"] + "].[Ident].[Consents])");

			return consentDetails;
		}

		/* 
         * A method that returns the payload for POST /api/IdentityConfig/InsertIdentityToken 
         * INPUT    : 
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostInsertIdentityTokenPayload()
		{
			DataTable tokenDetails = DoSelectTokens();

			Token tokenPayload = new Token();

			tokenPayload.TokenKey = Guid.NewGuid().ToString();
			tokenPayload.TokenType = int.Parse(tokenDetails.Rows[0]["TokenType"].ToString());
			tokenPayload.ClientCode = tokenDetails.Rows[0]["ClientCode"].ToString();
			tokenPayload.SubjectCode = tokenDetails.Rows[0]["SubjectCode"].ToString();
			tokenPayload.Expiry = DateTimeOffset.Now;
			tokenPayload.JsonCode = tokenDetails.Rows[0]["JsonCode"].ToString();
			tokenPayload.AuthCodeChallenge = tokenDetails.Rows[0]["AuthCodeChallenge"].ToString();
			tokenPayload.AuthCodeChallengeMethod = tokenDetails.Rows[0]["AuthCodeChallengeMethod"].ToString();

			if (tokenDetails.Rows[0]["IsOpenId"].ToString().Length > 0)
			{
				tokenPayload.IsOpenId = Convert.ToBoolean(tokenDetails.Rows[0]["IsOpenId"]);
			}
			else
			{
				tokenPayload.IsOpenId = null;
			}

			tokenPayload.Nonce = tokenDetails.Rows[0]["Nonce"].ToString();
			tokenPayload.RedirectUri = tokenDetails.Rows[0]["RedirectUri"].ToString();
			tokenPayload.SessionId = tokenDetails.Rows[0]["SessionId"].ToString();

			if (tokenDetails.Rows[0]["WasConsentShown"].ToString().Length > 0)
			{
				tokenPayload.WasConsentShown = Convert.ToBoolean(tokenDetails.Rows[0]["WasConsentShown"]);
			}
			else
			{
				tokenPayload.WasConsentShown = null;
			}

			return JsonConvert.SerializeObject(tokenPayload);
		}

		/* 
         * A method that returns the portfolioProductUserClaims table data for GET /api/IdentityConfig/GetAllPortfolioProductUserClaims 
         * INPUT    : 
         * OUTPUT   : portfolioProductUserClaims in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectPortfolioProductUserClaims()
		{
			DataTable portfolioProductUserClaims =
				dbManager.executeQuery("SELECT PPUC.PortfolioProductUserClaimsID as Id, P.PortfolioId as PortfolioId"
		+ ", C.ClientCode as ClientId, PPU.UserID as UserId, PPUC.Type as [Type], PPUC.Value as [Value] "
		+ "FROM	[" + Properties["identityDatabase"] + "].[Ident].[PortfolioProductUser] PPU WITH(NOLOCK) "
		+ "INNER JOIN[" + Properties["identityDatabase"] + "].[Ident].[PortfolioProductUserClaims] PPUC WITH(NOLOCK) ON PPU.PortfolioProductUserID = PPUC.PortfolioProductUserID "
		+ "INNER JOIN[" + Properties["identityDatabase"] + "].[Ident].[Portfolio] P WITH(NOLOCK) ON P.PortfolioId = PPU.PortfolioID "
		+ "INNER JOIN[" + Properties["identityDatabase"] + "].[Ident].[PortfolioProduct] PP WITH(NOLOCK) ON PPU.PortfolioId = PP.PortfolioID "
		+ "INNER JOIN[" + Properties["identityDatabase"] + "].[Ident].[Product] PR WITH(NOLOCK) ON PP.ProductId = Pr.ProductId "
		+ "INNER JOIN[" + Properties["identityDatabase"] + "].[Ident].[Clients] C WITH(NOLOCK) ON PR.ClientId = C.ClientId");

			return portfolioProductUserClaims;
		}

		/* 
         * A method that returns the ScopeClaims table data for GET /api/IdentityConfig/GetAllScopeClaims 
         * INPUT    : 
         * OUTPUT   : allScopeClaims in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectScopeClaims()
		{
			DataTable allScopeClaims =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[ScopeClaims]");

			return allScopeClaims;
		}

		/* 
         * A method that returns the Scopes table data for GET /api/IdentityConfig/GetAllScopes  
         * INPUT    : 
         * OUTPUT   : allScopes in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectScopes()
		{
			DataTable allScopes =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[Scopes]");

			return allScopes;
		}

		/* 
         * A method that returns the ScopeSecrets table data for GET /api/IdentityConfig/GetAllScopeSecrets  
         * INPUT    : 
         * OUTPUT   : allScopeSecrets in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectScopeSecrets()
		{
			DataTable allScopeSecrets =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[ScopeSecrets]");

			return allScopeSecrets;
		}

		/* 
         * A method that returns the payload for POST /api/IdentityConfig/InsertConsent.
         * INPUT    : 
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostInsertConsentPayload()
		{
			DataTable token = DoSelectTokens();

			Consent consentPayload = new Consent();

			consentPayload.SubjectCode = token.Rows[0]["SubjectCode"].ToString();
			consentPayload.ClientCode = token.Rows[0]["ClientCode"].ToString();
			consentPayload.Scopes = "";

			return JsonConvert.SerializeObject(consentPayload);
		}

		/* 
         * A method that returns the IdentityProviderType table data for GET /api/identityprovider/providerconfiguration/{IdentityProviderTypeId}
         * INPUT    : 
         * OUTPUT   : identityProviderTypes in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectIdentityProviderType()
		{
			DataTable identityProviderTypes =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderType]");

			return identityProviderTypes;
		}

		/* 
         * A method that returns the IdentityProviderSettingType table data for POST /api/identityprovider/settings
         * INPUT    : 
         * OUTPUT   : identityProviderSettingTypes in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectIdentityProviderSettingType()
		{
			DataTable identityProviderSettingTypes =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSettingType]");

			return identityProviderSettingTypes;
		}

		/* 
         * A method that returns the payload for POST /api/identityprovider/settings.
         * INPUT    : 
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostIdentityProviderSettingsPayload()
		{
			DataTable identityProviderSettingTypes = DoSelectIdentityProviderSettingType();

			IdentityProviderSetting identityProviderSettingPayload = new IdentityProviderSetting();

			identityProviderSettingPayload.IdentityProviderSettingId = 0;
			identityProviderSettingPayload.IdentityProviderSettingTypeId = int.Parse(identityProviderSettingTypes.Rows[0]["IdentityProviderSettingTypeId"].ToString());
			identityProviderSettingPayload.Value = string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray()));

			return JsonConvert.SerializeObject(identityProviderSettingPayload);
		}

		/* 
         * A method that returns the payload for POST /api/identityprovider/settingtypes.
         * INPUT    : 
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostIdentityProviderSettingTypesPayload()
		{
			DataTable identityProviderTypes = DoSelectIdentityProviderType();

			IdentityProviderSettingType identityProviderSettingTypePayload = new IdentityProviderSettingType();

			identityProviderSettingTypePayload.IdentityProviderSettingTypeId = 0;
			identityProviderSettingTypePayload.IdentityProviderTypeId = int.Parse(identityProviderTypes.Rows[0]["IdentityProviderTypeId"].ToString());
			identityProviderSettingTypePayload.Name = string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray()));

			return JsonConvert.SerializeObject(identityProviderSettingTypePayload);
		}

		/* 
         * A method that returns the payload for POST /api/identityprovider/types.
         * INPUT    : 
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostIdentityProviderTypesPayload()
		{
			// Set up the API URL
			EndPointUrl = HostIdentityUrl + Properties["IdentityProviderType"] + "?enterpriseUserName=" + WebUtility.UrlEncode(Properties["enterpriseUsername"]);

			// Execute API
			IdentityProviderTypeOutput identityProviderTypeOutput = JsonConvert.DeserializeObject<IdentityProviderTypeOutput>
				(GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")));

			IdentityProviderType identityProviderTypePayload = new IdentityProviderType();

			identityProviderTypePayload.AuthenticationType = identityProviderTypeOutput.identityProviderType.AuthenticationType;

			return JsonConvert.SerializeObject(identityProviderTypePayload);
		}

		/* 
         * A method that returns a string-formatted payload for POST /api/userlogin/Auth
         * INPUT    : 
         * OUTPUT   : Payload in String format (or null for any Exception) 
         */
		public string DoPostUserLoginAuthPayload(string _EnterpriseUsername, string _Password)
		{
			AuthUserDetails authUserDetails = new AuthUserDetails();

			authUserDetails.EnterpriseUserName = _EnterpriseUsername;
			authUserDetails.Password = _Password;
			authUserDetails.UserDeviceDetails = new UserDeviceDetails();

			var request = (HttpWebRequest)WebRequest.Create("https://api.ipify.org");
			request.UserAgent = "curl"; // this simulate curl linux command
			string publicIPAddress;
			request.Method = "GET";
			using (WebResponse response = request.GetResponse())
			{
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					publicIPAddress = reader.ReadToEnd().Replace("\n", "");
				}
			}

			authUserDetails.UserDeviceDetails.IpAddress = publicIPAddress;
			authUserDetails.UserDeviceDetails.BrowserType = "InternetExplorer11";
			authUserDetails.UserDeviceDetails.BrowserName = "InternetExplorer";
			authUserDetails.UserDeviceDetails.Version = "11.0";
			authUserDetails.UserDeviceDetails.Platform = "WinNT";
			authUserDetails.UserDeviceDetails.IsMobile = false;
			authUserDetails.UserDeviceDetails.DeviceType = "DesktopOrLaptop";
			authUserDetails.UserDeviceDetails.Timezone = TimeZoneInfo.Local.DisplayName;

			return JsonConvert.SerializeObject(authUserDetails);
		}

		/* 
         * A method that returns the Activity table data 
         * INPUT    : 
         * OUTPUT   : activityDetails in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectActivity(string _ActivityIds)
		{
			DataTable activityDetails =
				dbManager.executeQuery("SELECT TOP 1 * from [" + Properties["identityDatabase"] + "].[Ident].[Activity] "
				+ "where ActivityId in (" + _ActivityIds + ")");

			return activityDetails;
		}

		/* 
         * A method that returns a string-formatted payload for POST and PUT /api/persons/{realPageId}/electronicaddress
         * INPUT    : 
         * OUTPUT   : Payload in String format (or null for any Exception) 
         */
		public string DoPostPutElectronicAddressPayload(HttpVerb _HttpVerb = HttpVerb.Put)
		{
			LinkElectronicAddress linkElectronicAddress = new LinkElectronicAddress();

			if (_HttpVerb == HttpVerb.Post)
			{
				EndPointUrl = HostUrl + Properties["ContactMechanismUsageTypes"] + "?ContactMechanismUsageTypeName=" + Uri.EscapeDataString("Email Notification");
				ObjectListOutput<ContactMechanismUsageType, IErrorData> ContactMechanismUsageTypes
					= JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(GetHttpWebResponseValue(
						GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")));

				linkElectronicAddress.PartyContactMechanism.FromDate = DateTime.Now;
				linkElectronicAddress.PartyContactMechanism.ThruDate = DateTime.Now.AddYears(1);
				linkElectronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId = ContactMechanismUsageTypes.list[0].ContactMechanismUsageTypeId;
				linkElectronicAddress.ElectronicAddress.AddressType = ContactMechanismUsageTypes.list[0].Name;
				linkElectronicAddress.ElectronicAddress.AddressString = Guid.NewGuid().ToString().Remove(6) + "@apiTest.com";
			}
			else if (_HttpVerb == HttpVerb.Put)
			{
				UserLogin userLogin = JsonConvert.DeserializeObject<UserLogin>(DoGetUserLoginUser(CurrentlyLoggedInUser));
				EndPointUrl = HostUrl + Properties["ElectronicAddress"].Replace("{realPageId}", userLogin.RealPageId.ToString());

				ObjectListOutput<ElectronicAddress, IErrorData> electronicAddress
					= JsonConvert.DeserializeObject<ObjectListOutput<ElectronicAddress, IErrorData>>(
						GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")));

				linkElectronicAddress.PartyContactMechanism = new PartyContactMechanism();
				linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId = electronicAddress.list[0].PartyContactMechanismId;
				linkElectronicAddress.PartyContactMechanism.PartyId = userLogin.PartyId;
				linkElectronicAddress.PartyContactMechanism.ContactMechanismId = electronicAddress.list[0].ContactMechanismId;				
				linkElectronicAddress.PartyContactMechanism.FromDate = DateTime.Now;
				linkElectronicAddress.PartyContactMechanism.ThruDate = DateTime.Now.AddYears(1);

				linkElectronicAddress.ElectronicAddress = new ElectronicAddress();
				linkElectronicAddress.ElectronicAddress.PartyContactMechanismId = electronicAddress.list[0].PartyContactMechanismId;
				linkElectronicAddress.ElectronicAddress.ContactMechanismId = electronicAddress.list[0].ContactMechanismId;
				linkElectronicAddress.ElectronicAddress.AddressString = Guid.NewGuid().ToString().Remove(6) + "@apiT3st.com";
				linkElectronicAddress.ElectronicAddress.AddressType = electronicAddress.list[0].AddressType;

				linkElectronicAddress.ElectronicAddress.contactMechanismUsageType = new ContactMechanismUsageType();
				linkElectronicAddress.ElectronicAddress.contactMechanismUsageType.ContactMechanismUsageTypeId = electronicAddress.list[0].contactMechanismUsageType.ContactMechanismUsageTypeId;
				linkElectronicAddress.ElectronicAddress.contactMechanismUsageType.ParentContactMechanismUsageTypeId = electronicAddress.list[0].contactMechanismUsageType.ParentContactMechanismUsageTypeId;
				linkElectronicAddress.ElectronicAddress.contactMechanismUsageType.Name = electronicAddress.list[0].contactMechanismUsageType.Name;

				linkElectronicAddress.ContactMechanismUsageType = linkElectronicAddress.ElectronicAddress.contactMechanismUsageType;
			}

			return JsonConvert.SerializeObject(linkElectronicAddress);
		}

		/* 
         * A method that returns a string-formatted payload for PUT /api/profiles/
         * INPUT    : 
         * OUTPUT   : Payload in String format (or null for any Exception) 
         */
		public string DoPutProfilesPayload()
		{
			//Set up the API URL
			EndPointUrl = HostUrl + Properties["Profiles"] + (JsonConvert.DeserializeObject<Person>(DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId);

			//Execute API
			var response = GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

			// Extract API's JSON Response
			var responseValue = GetHttpWebResponseValue(response);
			ObjectOutput<Profile, IErrorData> profileOutput = JsonConvert.DeserializeObject<ObjectOutput<Profile, IErrorData>>(responseValue);


            Profile profile = new Profile();

            profile.TelecommunicationNumber = profileOutput.obj.TelecommunicationNumber;
            profile.PartyRole = new PartyRole();
            profile.PartyRole.PartyRoleId = profileOutput.obj.PartyRole.PartyRoleId;
            profile.PartyRole.PartyId = profileOutput.obj.PartyRole.PartyId;
            profile.PartyRole.RoleTypeId = profileOutput.obj.PartyRole.RoleTypeId;
            profile.PartyRole.Name = profileOutput.obj.PartyRole.Name;
            profile.PartyId = profileOutput.obj.PartyId;
            profile.RealPageId = profileOutput.obj.RealPageId;
            profile.FirstName = profileOutput.obj.FirstName;
            profile.MiddleName = profileOutput.obj.MiddleName;
            profile.LastName = profileOutput.obj.LastName;
            profile.Suffix = profileOutput.obj.Suffix;
            profile.Title = profileOutput.obj.Title;
            profile.PreferredContactMethodId = profileOutput.obj.PreferredContactMethodId;


            return JsonConvert.SerializeObject(profile);
		}

		/* 
         * A method that returns a string-formatted payload for POST and PUT /api/persons/{realPageId}/PostalAddress
         * INPUT    : 
         * OUTPUT   : Payload in String format (or null for any Exception) 
         */
		public string DoPostPutPostalAddressPayload(string _PostalAddressUsername, HttpVerb _HttpVerb = HttpVerb.Put)
		{
			LinkPostalAddress linkPostalAddress = new LinkPostalAddress();
			linkPostalAddress.PartyContactMechanism = new PartyContactMechanism();
			linkPostalAddress.ContactMechanismUsageType = new ContactMechanismUsageType();
			linkPostalAddress.StreetAddress = new StreetAddress();
			linkPostalAddress.ContactMechanismBoundary = new ContactMechanismBoundary();
			linkPostalAddress.GeographicBoundary = new List<GeographicBoundary>();
			GeographicBoundary geographicBoundary = new GeographicBoundary();
			geographicBoundary.GeographicBoundaryType = new GeographicBoundaryType();

			if (_HttpVerb == HttpVerb.Post)
			{
				EndPointUrl = HostUrl + Properties["ContactMechanismUsageTypes"] + "?ContactMechanismUsageTypeName=" + Uri.EscapeDataString("Contact Mechanism Type");
				ObjectListOutput<ContactMechanismUsageType, IErrorData> ContactMechanismUsageTypes
					= JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(GetHttpWebResponseValue(
						GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")));

				linkPostalAddress.PartyContactMechanism.FromDate = DateTime.Now;
				linkPostalAddress.PartyContactMechanism.ThruDate = DateTime.Now.AddYears(1);
				linkPostalAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId = ContactMechanismUsageTypes.list[0].ContactMechanismUsageTypeId;
				linkPostalAddress.StreetAddress.StreetAddress1 = "2201 Lakeside Blvd";
				linkPostalAddress.ContactMechanismBoundary.FromDate = DateTime.Now;
				linkPostalAddress.ContactMechanismBoundary.ThruDate = DateTime.Now.AddYears(1);
				geographicBoundary.GeographicBoundaryType.TypeName = "City";
				geographicBoundary.Name = "Richardson " + Guid.NewGuid().ToString().Remove(4);
				linkPostalAddress.GeographicBoundary.Add(geographicBoundary);
			}
			else if (_HttpVerb == HttpVerb.Put)
			{
				DataTable postalAddressDetails = new DataTable();
				string selectPostalAddresDetails = "SELECT TOP 1 cmb.[ContactMechanismBoundaryId]\n"
					+ ", cmb.[FromDate] as [ContactMechanismBoundaryFromDate], cmb.[ThruDate] as [ContactMechanismBoundaryThruDate]\n"
					+ ", gb.[GeographicBoundaryId], gb.[Name] as [GeographicBoundaryName], gb.[GeographicBoundaryCode], gb.[Abbreviation]\n"
					+ ", gbt.[GeographicBoundaryTypeId], gbt.[Name] as [GeographicBoundaryTypeName]\n"
					+ "from[" + Properties["identityDatabase"] + "].Enterprise.PartyContactMechanism pcm\n"
					+ "JOIN[" + Properties["identityDatabase"] + "].Enterprise.Party p ON p.PartyId = pcm.PartyId\n"
					+ "join [" + Properties["identityDatabase"] + "].Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId\n"
					+ "join[" + Properties["identityDatabase"] + "].Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID\n"
					+ "JOIN[" + Properties["identityDatabase"] + "].Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId\n"
					+ "JOIN[" + Properties["identityDatabase"] + "].Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId\n"
					+ "WHERE p.RealPageId = (select RealPageId from [" + Properties["identityDatabase"] + "].Enterprise.Party\n"
					+ "WHERE partyid = (select partyid from [" + Properties["identityDatabase"] + "].ident.userLogin\n"
					+ "WHERE loginName = '" + _PostalAddressUsername + "')) \n"
					+ "AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE())\n"
					+ "ORDER BY gb.[GeographicBoundaryId] desc";

				postalAddressDetails = dbManager.executeQuery(selectPostalAddresDetails);

				UserLogin userLogin = JsonConvert.DeserializeObject<UserLogin>(DoGetUserLoginUser(CurrentlyLoggedInUser));
				EndPointUrl = HostUrl + Properties["PostalAddress"].Replace("{realPageId}", userLogin.RealPageId.ToString());

				ObjectListOutput<PostalAddress, IErrorData> postalAddress
				= JsonConvert.DeserializeObject<ObjectListOutput<PostalAddress, IErrorData>>(
						GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")));

				linkPostalAddress.PartyContactMechanism.PartyContactMechanismId = postalAddress.list[0].PartyContactMechanismId;
				linkPostalAddress.PartyContactMechanism.PartyId = userLogin.PartyId;
				linkPostalAddress.PartyContactMechanism.ContactMechanismId = postalAddress.list[0].ContactMechanismId;
				linkPostalAddress.StreetAddress.ContactMechanismId = postalAddress.list[0].ContactMechanismId;
				linkPostalAddress.ContactMechanismBoundary.ContactMechanismId = postalAddress.list[0].ContactMechanismId;
				linkPostalAddress.PartyContactMechanism.FromDate = DateTime.Now;
				linkPostalAddress.PartyContactMechanism.ThruDate = DateTime.Now.AddYears(1);

				linkPostalAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId = postalAddress.list[0].contactMechanismUsageType.ContactMechanismUsageTypeId;
				linkPostalAddress.ContactMechanismUsageType.ParentContactMechanismUsageTypeId = postalAddress.list[0].contactMechanismUsageType.ParentContactMechanismUsageTypeId;
				linkPostalAddress.ContactMechanismUsageType.Name = postalAddress.list[0].contactMechanismUsageType.Name;

				linkPostalAddress.StreetAddress.StreetAddress1 = postalAddress.list[0].AddressString;
				linkPostalAddress.StreetAddress.StreetAddress2 = "";
				linkPostalAddress.StreetAddress.StreetAddress3 = "";

				linkPostalAddress.ContactMechanismBoundary.ContactMechanismBoundaryId = int.Parse(postalAddressDetails.Rows[0]["ContactMechanismBoundaryId"].ToString());
				linkPostalAddress.ContactMechanismBoundary.GeographicBoundaryId = int.Parse(postalAddressDetails.Rows[0]["GeographicBoundaryId"].ToString());

				if (postalAddressDetails.Rows[0]["ContactMechanismBoundaryFromDate"].ToString().Length < 1)
				{
					linkPostalAddress.ContactMechanismBoundary.FromDate = DateTime.Now;
				}
				else
				{
					linkPostalAddress.ContactMechanismBoundary.FromDate = Convert.ToDateTime(postalAddressDetails.Rows[0]["ContactMechanismBoundaryFromDate"].ToString());
				}

				if (postalAddressDetails.Rows[0]["ContactMechanismBoundaryThruDate"].ToString().Length < 1)
				{
					linkPostalAddress.ContactMechanismBoundary.ThruDate = DateTime.Now;
				}
				else
				{
					linkPostalAddress.ContactMechanismBoundary.ThruDate = Convert.ToDateTime(postalAddressDetails.Rows[0]["ContactMechanismBoundaryThruDate"].ToString());
				}

				GeographicBoundary newGeographicBoundary = new GeographicBoundary();

				for (int countGeographicBoundary = 0; countGeographicBoundary < postalAddressDetails.Rows.Count; countGeographicBoundary++)
				{
					newGeographicBoundary.GeographicBoundaryId = int.Parse(postalAddressDetails.Rows[countGeographicBoundary]["GeographicBoundaryId"].ToString());

					newGeographicBoundary.GeographicBoundaryType = new GeographicBoundaryType();
					newGeographicBoundary.GeographicBoundaryType.GeographicBoundaryTypeId = int.Parse(postalAddressDetails.Rows[countGeographicBoundary]["GeographicBoundaryTypeId"].ToString());
					newGeographicBoundary.GeographicBoundaryType.TypeName = postalAddressDetails.Rows[countGeographicBoundary]["GeographicBoundaryTypeName"].ToString();
					newGeographicBoundary.Name = postalAddressDetails.Rows[countGeographicBoundary]["GeographicBoundaryName"].ToString();
					newGeographicBoundary.GeographicBoundaryCode = postalAddressDetails.Rows[countGeographicBoundary]["GeographicBoundaryCode"].ToString();
					newGeographicBoundary.Abbreviation = postalAddressDetails.Rows[countGeographicBoundary]["Abbreviation"].ToString();
					newGeographicBoundary.GeographicBoundaryTypeId = int.Parse(postalAddressDetails.Rows[countGeographicBoundary]["GeographicBoundaryTypeId"].ToString());
					newGeographicBoundary.TypeName = postalAddressDetails.Rows[countGeographicBoundary]["GeographicBoundaryTypeName"].ToString();
					linkPostalAddress.GeographicBoundary.Add(newGeographicBoundary);
					newGeographicBoundary = new GeographicBoundary();
				}
			}

			return JsonConvert.SerializeObject(linkPostalAddress);
		}

		/* 
         * A method that returns the ContactMechanismUsageType table data for GET /api/ContactMechanismUsageType
         * INPUT    : 
         * OUTPUT   : identityProviderSettingTypes in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectContactMechanismUsageType(string _Name = "")
		{
			if (_Name.Length > 0)
			{
				_Name = "\nWHERE ParentContactMechanismUsageTypeId = (SELECT TOP 1 ContactMechanismUsageTypeId "
					+ "FROM [" + Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsageType] WHERE Name = '" + _Name + "')";
			}
			
			DataTable contactMechanismUsageType =
				dbManager.executeQuery("SELECT * from [" + Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsageType]" + _Name);

			return contactMechanismUsageType;
		}

		/* 
         * A method that returns UserLogin data associated with UserSecurityQuestions table 
		 * for GET /api/ContactMechanismUsageType
         * INPUT    : string _OrderBy = "asc"
         * OUTPUT   : userLoginsWithSecurityQuestions in DataTable format (or null for any Exception) 
         */
		public DataTable DoSelectUserLoginsWithSecurityQuestions(string _OrderBy = "asc")
		{
			DataTable userLoginsWithSecurityQuestions =
				dbManager.executeQuery("SELECT distinct pcm.partyid, ul.partyid, ul.loginname "
				+ (_OrderBy == "desc"? "" : ", usa.SecurityQuestionId, sq.Question ") // Choose Descending Order to avoid selecting SecurityQuestions.
				+ "FROM[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderType] idpt "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSettingType] idpst "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[IdentityProviderSetting] idps "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[ContactMechanismIdentity] cmid "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[Organization] o "
				+ "inner join[" + Properties["identityDatabase"] + "].[Enterprise].[PartyRelationship] pr "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[UserLogin] ul "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[usersecurityanswer] usa "
				+ "inner join[" + Properties["identityDatabase"] + "].[Ident].[SecurityQuestion] sq "
				+ "on usa.SecurityQuestionId = sq.SecurityQuestionId "
				+ "on usa.userid = ul.userid "
				+ "on ul.partyid = pr.partyidfrom "
				+ "on pr.partyidto = o.partyid "
				+ "on pcm.partyid = o.partyid "
				+ "on pcm.contactmechanismid = cmid.contactmechanismid "
				+ "on cmid.[IdentityProviderSettingId] = idps.[IdentityProviderSettingId] "
				+ "on idps.identityprovidersettingtypeid = idpst.identityprovidersettingtypeid "
				+ "on idpt.identityprovidertypeid = idpst.identityprovidertypeid "
				+ "where idpt.description = 'IdentityServer' "
				+ "\nAND ul.LoginName like '%ApiTest%'"
				+ "\nORDER BY ul.LoginName " + _OrderBy);

			return userLoginsWithSecurityQuestions;
		}

        /* 
        * A method that returns the payload for POST and PUT /api/UserLogins.
        * INPUT    : 
        * OUTPUT   : Payload in string format (or null for any Exception) 
        */
        public string DoPostPutUserLogins(int _UserId, int _PartyId, string _RealpageId, string _LoginNameType, bool _IsActive, bool _isLocked, bool _isTainted, string _PasswordModifiedDate, string _StatusSetDate, string _LastLogin, bool _IsSuperUser, string Status, string _LoginName, string _fromDate, string _thruDate = "", HttpVerb _HttpVerb = HttpVerb.Post)
        {
            UserLogin userlogin = new UserLogin();

            if (_HttpVerb == HttpVerb.Post)
            {
                userlogin.LoginName = string.Concat(Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "@test.com");
                userlogin.FromDate = Convert.ToDateTime(_fromDate);
                userlogin.ThruDate = Convert.ToDateTime(_thruDate);

                userlogin.UserId = 0;
                userlogin.PartyId = 0;
                userlogin.RealPageId = new Guid();
                userlogin.LoginNameType = "1";
                userlogin.IsActive = true;
                userlogin.IsLocked = false;
                userlogin.IsTainted = false;
                userlogin.PasswordModifiedDate = Convert.ToDateTime(_PasswordModifiedDate);
                userlogin.StatusSetDate = Convert.ToDateTime(_StatusSetDate);
                userlogin.LastLogin = Convert.ToDateTime(_LastLogin);
                userlogin.IsSuperUser = true;
                //userlogin.Status = "";


            }
            else if (_HttpVerb == HttpVerb.Put)
            {
                userlogin.LoginName = _LoginName;
                userlogin.FromDate = Convert.ToDateTime(_fromDate);
                userlogin.ThruDate = Convert.ToDateTime(_thruDate);

                userlogin.UserId = _UserId;
                userlogin.PartyId = _PartyId;
                userlogin.RealPageId = new Guid(_RealpageId);
                userlogin.LoginNameType = "1";
                userlogin.IsActive = true;
                userlogin.IsLocked = false;
                userlogin.IsTainted = false;
                userlogin.PasswordModifiedDate = Convert.ToDateTime(_PasswordModifiedDate);
                userlogin.StatusSetDate = Convert.ToDateTime(_StatusSetDate);
                userlogin.LastLogin = Convert.ToDateTime(_LastLogin);
                userlogin.IsSuperUser = true;
                //userlogin.Status = "";
            }

            return JsonConvert.SerializeObject(userlogin);
        }

        public string DoPostPutTelecommunicationNumberPayload(string _TelecommunicationNumberUsername, HttpVerb _HttpVerb = HttpVerb.Put)
        {
            DataTable telecommunicationNumberDetails = dbManager.executeQuery("SELECT pcm.PartyContactMechanismId , cmut.ContactMechanismUsageTypeID, "
				+ "pcm.fromDate, pcm.thruDate, pcm.partyId,"
                + "tele.[ContactMechanismID], tele.[CountryCode], tele.[AreaCode], tele.[PhoneNumber], "
                + "cmut.[ContactMechanismUsageTypeID], cmut.[ParentContactMechanismUsageTypeID], cmut.[Name] "
                + "FROM[" + Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsageType] cmut "
                + "INNER JOIN[" + Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsage] cmu "
                + "INNER JOIN[" + Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
                + "INNER JOIN[" + Properties["identityDatabase"] + "].[Enterprise].[TelecommunicationsNumber] tele "
                + "ON pcm.ContactMechanismId = tele.ContactMechanismId "
                + "ON pcm.partycontactmechanismid = cmu.partycontactmechanismid "
                + "ON cmut.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID "
                + " where pcm.partyid = (select partyid from[" + Properties["identityDatabase"] + "].[Ident].[UserLogin] where loginname = '" + _TelecommunicationNumberUsername + "') "
                + "ORDER BY partycontactmechanismid ASC");

            LinkTelecommunicationNumber linkTelecommunicationNumber = new LinkTelecommunicationNumber();
			linkTelecommunicationNumber.PartyContactMechanism = new PartyContactMechanism();
			linkTelecommunicationNumber.ContactMechanismUsageType = new ContactMechanismUsageType();
			linkTelecommunicationNumber.TelecommunicationNumber = new TelecommunicationNumber();
			linkTelecommunicationNumber.TelecommunicationNumber.contactMechanismUsageType = new ContactMechanismUsageType();

			if (_HttpVerb == HttpVerb.Put)
			{
				linkTelecommunicationNumber.PartyContactMechanism.PartyContactMechanismId = int.Parse(telecommunicationNumberDetails.Rows[0]["PartyContactMechanismId"].ToString());
				linkTelecommunicationNumber.PartyContactMechanism.PartyId = int.Parse(telecommunicationNumberDetails.Rows[0]["PartyId"].ToString());
				linkTelecommunicationNumber.PartyContactMechanism.ContactMechanismId = int.Parse(telecommunicationNumberDetails.Rows[0]["ContactMechanismId"].ToString());
				linkTelecommunicationNumber.PartyContactMechanism.FromDate = Convert.ToDateTime(telecommunicationNumberDetails.Rows[0]["FromDate"].ToString());
				linkTelecommunicationNumber.PartyContactMechanism.ThruDate = Convert.ToDateTime(telecommunicationNumberDetails.Rows[0]["ThruDate"].ToString());

				linkTelecommunicationNumber.ContactMechanismUsageType.ContactMechanismUsageTypeId = int.Parse(telecommunicationNumberDetails.Rows[0]["ContactMechanismUsageTypeId"].ToString());
				linkTelecommunicationNumber.ContactMechanismUsageType.ParentContactMechanismUsageTypeId = int.Parse(telecommunicationNumberDetails.Rows[0]["ParentContactMechanismUsageTypeId"].ToString());
				linkTelecommunicationNumber.ContactMechanismUsageType.Name = telecommunicationNumberDetails.Rows[0]["Name"].ToString();

				linkTelecommunicationNumber.TelecommunicationNumber.PartyContactMechanismId = int.Parse(telecommunicationNumberDetails.Rows[0]["PartyContactMechanismId"].ToString());
				linkTelecommunicationNumber.TelecommunicationNumber.ContactMechanismId = int.Parse(telecommunicationNumberDetails.Rows[0]["ContactMechanismId"].ToString());
				linkTelecommunicationNumber.TelecommunicationNumber.CountryCode = telecommunicationNumberDetails.Rows[0]["CountryCode"].ToString();
				linkTelecommunicationNumber.TelecommunicationNumber.AreaCode = telecommunicationNumberDetails.Rows[0]["AreaCode"].ToString();
				linkTelecommunicationNumber.TelecommunicationNumber.PhoneNumber = telecommunicationNumberDetails.Rows[0]["PhoneNumber"].ToString();

				linkTelecommunicationNumber.TelecommunicationNumber.contactMechanismUsageType.ContactMechanismUsageTypeId = int.Parse(telecommunicationNumberDetails.Rows[0]["ContactMechanismUsageTypeId"].ToString());
				linkTelecommunicationNumber.TelecommunicationNumber.contactMechanismUsageType.ParentContactMechanismUsageTypeId = int.Parse(telecommunicationNumberDetails.Rows[0]["ParentContactMechanismUsageTypeId"].ToString());
				linkTelecommunicationNumber.TelecommunicationNumber.contactMechanismUsageType.Name = telecommunicationNumberDetails.Rows[0]["Name"].ToString();
			}
			else if (_HttpVerb == HttpVerb.Post)
			{
				EndPointUrl = HostUrl + Properties["ContactMechanismUsageTypes"] + "?ContactMechanismUsageTypeName=" + Uri.EscapeDataString("Phone Type");
				ObjectListOutput<ContactMechanismUsageType, IErrorData> ContactMechanismUsageTypes
					= JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(GetHttpWebResponseValue(
						GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")));

				linkTelecommunicationNumber.PartyContactMechanism.FromDate = DateTime.Now;
				linkTelecommunicationNumber.PartyContactMechanism.ThruDate = DateTime.Now.AddYears(2);

				linkTelecommunicationNumber.ContactMechanismUsageType.ContactMechanismUsageTypeId = ContactMechanismUsageTypes.list[0].ContactMechanismUsageTypeId;
				linkTelecommunicationNumber.ContactMechanismUsageType.ParentContactMechanismUsageTypeId = ContactMechanismUsageTypes.list[0].ParentContactMechanismUsageTypeId;
				linkTelecommunicationNumber.ContactMechanismUsageType.Name = ContactMechanismUsageTypes.list[0].Name;
				
				linkTelecommunicationNumber.TelecommunicationNumber.CountryCode = "44";
				linkTelecommunicationNumber.TelecommunicationNumber.AreaCode = "7911";
				linkTelecommunicationNumber.TelecommunicationNumber.PhoneNumber = "654321";

				linkTelecommunicationNumber.TelecommunicationNumber.contactMechanismUsageType = linkTelecommunicationNumber.ContactMechanismUsageType;
			}

			return JsonConvert.SerializeObject(linkTelecommunicationNumber);
        }

		/* 
         * A method that returns the ContactMechanism data for GET /api/persons/{realPageId}/contactmechanism
         * INPUT    : 
         * OUTPUT   : contactMechanismList in DataTable format (or null for any Exception) 
         */
		public DataTable DoListContactMechanismsForPerson(string _RealPageId)
		{
			DataTable contactMechanismList =
				dbManager.executeQuery("SELECT  pcm.PartyContactMechanismId, cm.ContactMechanismID,"
					+ "ea.ElectronicAddressString AS AddressString, 'Email' AS AddressType,"
					+ "cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId,"
					+ "cmut.ParentContactMechanismUsageTypeID, cmut.Name "
					+ "FROM    [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsageType cmut "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsage cmu ON cmut.ContactMechanismUsageTypeId = cmu.ContactMechanismUsageTypeId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].Party p ON p.PartyId = pcm.PartyId "
					+ "WHERE   p.RealPageId = '" + _RealPageId + "' AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE()) UNION ALL "
					+ "SELECT  pcm.PartyContactMechanismId,	cm.ContactMechanismID,pa.StreetAddress1 AS AddressString,"
					+ "'Street Address' AS AddressType,	cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId,"
					+ "cmut.ParentContactMechanismUsageTypeID,cmut.Name "
					+ "FROM    [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsageType cmut "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsage cmu ON cmut.ContactMechanismUsageTypeId = cmu.ContactMechanismUsageTypeId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].Party p ON p.PartyId = pcm.PartyId "
					+ "WHERE   p.RealPageId = '" + _RealPageId + "' "
					+ "AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE()) UNION ALL "
					+ "SELECT  pcm.PartyContactMechanismId,	cm.ContactMechanismID,gb.Name AS AddressString, "
					+ "gbt.Name AS AddressType,	cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId,"
					+ "cmut.ParentContactMechanismUsageTypeID,cmut.Name "
					+ "FROM    [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsageType cmut "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsage cmu ON cmut.ContactMechanismUsageTypeId = cmu.ContactMechanismUsageTypeId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].Party p ON p.PartyId = pcm.PartyId "
					+ "WHERE   p.RealPageId = '" + _RealPageId + "' AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE()) UNION ALL "
					+ "SELECT  pcm.PartyContactMechanismId,	cm.ContactMechanismID,"
					+ "CONCAT(tm.CountryCode, tm.AreaCode, tm.PhoneNumber) AS AddressString,"
					+ "'Telecommunications Number' AS AddressType,"
					+ "cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId,"
					+ "cmut.ParentContactMechanismUsageTypeID,	cmut.Name "
					+ "FROM    [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsageType cmut "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsage cmu ON cmut.ContactMechanismUsageTypeId = cmu.ContactMechanismUsageTypeId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID "
					+ "JOIN [" + Properties["identityDatabase"] + "].[Enterprise].Party p ON p.PartyId = pcm.PartyId WHERE   p.RealPageId = '" + _RealPageId + "' "
					+ "AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE())");

			return contactMechanismList;
		}

		/* 
         * A method that returns the payload for POST /api/emailnotification/newprofile.
         * INPUT    : string _EnterpriseUserName, string _OrganizationRealPageId
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
		public string DoPostEmailNotificationPayload(string _EnterpriseUserName = "", string _FirstName = "", string _LastName = "", string _Persona = "")
		{
			ProfileDetail profileDetailRequest = new ProfileDetail();
			profileDetailRequest.userLogin = new UserLogin();
			profileDetailRequest.organization = new List<Organization>();

			// FirstName
			if (_FirstName == null)
			{
				profileDetailRequest.FirstName = _FirstName;
			}
			else if (_FirstName.Length > 0)
			{
				profileDetailRequest.FirstName = _FirstName.Replace(" ", "");
			}
			else
			{
				profileDetailRequest.FirstName = "AutoFirstName";
			}

			// Personas
			if (_Persona == null)
			{
				profileDetailRequest.Persona = null;
			}
			else if (_Persona.Length > 0)
			{
				profileDetailRequest.Persona = JsonConvert.DeserializeObject<List<Persona>>(_Persona);
			}
			else
			{
				EndPointUrl = HostUrl + Properties["Persona"] + "/Environment";
				profileDetailRequest.Persona = (JsonConvert.DeserializeObject<ObjectListOutput<Persona, ErrorData>>(GetHttpWebResponseValue(
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")))).list;
			}

			// LastName
			if (_LastName == null)
			{
				profileDetailRequest.LastName = _LastName;
			}
			else if (_LastName.Length > 0)
			{
				profileDetailRequest.LastName = _LastName.Replace(" ", "");
			}
			else
			{
				profileDetailRequest.LastName = "AutoLastName";
			}

			// EnterpriseUserName
			if (_EnterpriseUserName == null)
			{
				profileDetailRequest.userLogin.LoginName = _EnterpriseUserName;
			}
			else if (_EnterpriseUserName.Length > 0)
			{
				profileDetailRequest.userLogin.LoginName = _EnterpriseUserName.Replace(" ", "");
			}

			EndPointUrl = HostIdentityUrl + Properties["Organization"].Replace("{realPageId}"
				, JsonConvert.DeserializeObject<UserLogin>(DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString());
			profileDetailRequest.organization = JsonConvert.DeserializeObject<IList<Organization>>(
					GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")));
			
			return JsonConvert.SerializeObject(profileDetailRequest);
		}

		/* 
         * A method that returns the payload for POST /api/newuser.
         * INPUT    : string _Payload
         * OUTPUT   : NewUserToken in string format (or null for any Exception) 
         */
		public string DoPostNewUserToken(string _Payload, int _UserType = 0)
		{
			EndPointUrl = HostUrl + Properties["NewUser"] + "?userType=" + _UserType;
			return JsonConvert.DeserializeObject<CreateUserResponse>(GetHttpWebResponseValue(GetHttpWebResponse(
				endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: _Payload))).UserToken;
		}

        public string DoGetProfileDetailsPayload()
        {

            ProfileDetail profileDetail = new ProfileDetail();
            profileDetail.organization = new List<Organization>();
            Organization organization = new Organization();




            DataTable getProfileDetails = dbManager.executeQuery("	SELECT DISTINCT a.PartyId, pp.RealPageId, a.LoginName, a.PasswordModifiedDate, a.LastLoginDate, a.FromDate, a.ThruDate, i.PartyRoleId, i.PartyId, i.RoleTypeId, j.Name, "
                 + "p.PartyId, pp.RealPageId, p.FirstName, p.MiddleName, p.LastName, p.Suffix, p.Title, p.PreferredContactMethodId, "
                 + "o.PartyId as OPartyId, o.Name as OName, po.RealPageId as ORealPageId , po.CreateDate , rtf.Name RoleNameFrom, rtt.Name RoleNameTo,rt.Name AS RelationshipType,  "
                 + "pr.PartyRelationshipId, pr.PartyIdFrom, pf.RealPageId AS RealPageIdFrom, pr.PartyIdTo, pt.RealPageId AS RealPageIdTo,  "
                 + "pr.RoleTypeIdFrom, 				 "
                 + "pr.RoleTypeIdTo,	 "
                 + "rtf.PartyRoleTypeId as FromPartyRoleTypeId, rtf.ParentPartyRoleTypeId as FromParentPartyRoleTypeId , rtf.Name as FromName,  "
                 + "rtt.PartyRoleTypeId as ToPartyRoleTypeId, rtt.ParentPartyRoleTypeId as ToParentPartyRoleTypeId , rtt.Name as ToName,  "
                 + "pr.PartyRelationshipTypeId, rt.RelationshipTypeId, rt.RoleTypeIdValidFrom, rt.RoleTypeIdValidTo, rt.Name as RName, rt.Description,  "
                 + "ert.PartyRoleTypeId , ert.ParentPartyRoleTypeId, ert.Name , "
                 + "pr.PartyRelationshipTypeId,	pr.FromDate, pr.ThruDate, "
                 + "pro.PartyRoleId, pro.PartyId, pro.RoleTypeId,	ert.Name "
                 + "FROM [Identity].[Enterprise].PartyRelationship pr "
                 + "INNER JOIN [Identity].[Enterprise].[Party] pf ON (pr.PartyIdFrom = pf.PartyId) "
                 + "INNER JOIN [Identity].[Enterprise].[Party] pt ON (pr.PartyIdTo = pt.PartyId) "
                 + "INNER JOIN [Identity].[Enterprise].[Party] pp ON (pr.PartyIdFrom = pp.PartyId) "
                 + "INNER JOIN[Identity].[Person].[Person] p ON (pp.PartyId = p.PartyId) "
                 + "INNER JOIN[Identity].[Enterprise].[Organization] o ON (pr.PartyIdTo = o.PartyId) "
                 + "INNER JOIN [Identity].[Enterprise].[Party] po ON (o.PartyId = po.PartyId) "
                 + "INNER JOIN [Identity].[Enterprise].[PartyRole] pro ON (p.PartyId = pro.PartyId) "
                 + "JOIN [Identity].[Enterprise].[RoleType] ert on (ert.PartyRoleTypeId = pro.RoleTypeId) "
                 + "INNER JOIN [Identity].[Enterprise].[RelationshipType] rt ON (pr.PartyRelationshipTypeId = rt.RelationshipTypeId) "
                 + "INNER JOIN [Identity].[Enterprise].RoleType rtf ON (pr.RoleTypeIdFrom = rtf.PartyRoleTypeId) "
                 + "INNER JOIN [Identity].[Enterprise].RoleType rtt ON (pr.RoleTypeIdTo = rtt.PartyRoleTypeId) "
                 + "LEFT OUTER JOIN [Identity].[Enterprise].[RoleType] prt ON (rtf.ParentPartyRoleTypeId = prt.PartyRoleTypeId) "
                 + "INNER JOIN[Identity].[Ident].[UserLogin] a "
                 + "INNER JOIN[Identity].[Enterprise].[PartyRole] i "
                 + "INNER JOIN[Identity].[Enterprise].[RoleType] j "
                 + "ON j.PartyRoleTypeId = i.RoleTypeId  "
                 + "ON i.PartyId = a.PartyId "
                 + "ON a.PartyId = p.PartyId "
                 + "WHERE pp.partyid = (select partyid from[Identity].[Ident].[UserLogin] where loginname = 'james@test.com')");


            profileDetail.Avatar = null;
            profileDetail.userLogin.PartyId = int.Parse(getProfileDetails.Rows[0]["PartyId"].ToString());
            profileDetail.userLogin.LoginName = getProfileDetails.Rows[0]["LoginName"].ToString();
            profileDetail.userLogin.FromDate = Convert.ToDateTime(getProfileDetails.Rows[0]["FromDate"].ToString());
            profileDetail.userLogin.ThruDate = Convert.ToDateTime(getProfileDetails.Rows[0]["ThruDate"].ToString());
            //profileDetail.userLogin.StatusSetDate = Convert.ToDateTime(getProfileDetails.Rows[0]["StatusSetDate"].ToString());
            //profileDetail.userLogin.LastLogin = Convert.ToDateTime(getProfileDetails.Rows[0]["LastLoginDate"].ToString());

            profileDetail.PartyRole.PartyRoleId = int.Parse(getProfileDetails.Rows[0]["PartyRoleId"].ToString());
            profileDetail.PartyRole.PartyId = int.Parse(getProfileDetails.Rows[0]["PartyId"].ToString());
            profileDetail.PartyRole.RoleTypeId = int.Parse(getProfileDetails.Rows[0]["RoleTypeId"].ToString());
            profileDetail.PartyRole.Name = getProfileDetails.Rows[0]["Name"].ToString();

            profileDetail.organization = new List<Organization>();
            if (organization != null)
            {
                for (int countOrganization = 0; countOrganization < getProfileDetails.Rows.Count; countOrganization++)
                {
                    organization.RealPageId = new Guid(getProfileDetails.Rows[0]["ORealPageId"].ToString());
                    organization.PartyId = int.Parse(getProfileDetails.Rows[0]["OPartyId"].ToString());
                    organization.BooksMasterId = 0;
                    organization.Name = getProfileDetails.Rows[0]["OName"].ToString();
                    organization.partyRelationship.PartyRelationshipId = int.Parse(getProfileDetails.Rows[0]["PartyRelationshipId"].ToString());
                    organization.partyRelationship.PartyIdFrom = int.Parse(getProfileDetails.Rows[0]["PartyIdFrom"].ToString());
                    organization.partyRelationship.PartyIdTo = int.Parse(getProfileDetails.Rows[0]["PartyIdTo"].ToString());
                    organization.partyRelationship.RealPageIdFrom = new Guid(getProfileDetails.Rows[0]["RealPageIdFrom"].ToString());
                    organization.partyRelationship.RoleTypeFrom.PartyRoleTypeId = int.Parse(getProfileDetails.Rows[0]["FromPartyRoleTypeId"].ToString());
                    organization.partyRelationship.RoleTypeFrom.ParentPartyRoleTypeId = int.Parse(getProfileDetails.Rows[0]["FromParentPartyRoleTypeId"].ToString());
                    organization.partyRelationship.RoleTypeFrom.Name = getProfileDetails.Rows[0]["FromName"].ToString();
                    organization.partyRelationship.RoleTypeTo.PartyRoleTypeId = int.Parse(getProfileDetails.Rows[0]["ToPartyRoleTypeId"].ToString());
                    organization.partyRelationship.RoleTypeTo.ParentPartyRoleTypeId = int.Parse(getProfileDetails.Rows[0]["ToParentPartyRoleTypeId"].ToString());
                    organization.partyRelationship.RoleTypeTo.Name = getProfileDetails.Rows[0]["ToName"].ToString();
                    organization.partyRelationship.PartyRelationshipTypeId = int.Parse(getProfileDetails.Rows[0]["PartyRelationshipTypeId"].ToString());
                    organization.partyRelationship.PartyRelationshipType.RelationshipTypeId = int.Parse(getProfileDetails.Rows[0]["RelationshipTypeId"].ToString());
                    organization.partyRelationship.PartyRelationshipType.RoleTypeIdValidFrom = int.Parse(getProfileDetails.Rows[0]["RoleTypeIdValidFrom"].ToString());
                    organization.partyRelationship.PartyRelationshipType.RoleTypeIdValidTo = int.Parse(getProfileDetails.Rows[0]["RoleTypeIdValidTo"].ToString());
                    organization.partyRelationship.PartyRelationshipType.Name = getProfileDetails.Rows[0]["RName"].ToString();
                    organization.partyRelationship.PartyRelationshipType.Description = getProfileDetails.Rows[0]["Description"].ToString();
                    organization.partyRelationship.FromDate = DateTime.Parse(getProfileDetails.Rows[0]["FromDate"].ToString());
                    organization.partyRelationship.ThruDate = DateTime.Parse(getProfileDetails.Rows[0]["ThruDate"].ToString());

                    profileDetail.organization.Add(organization);
                    organization = new Organization();
                }
            }
            profileDetail.PartyId = int.Parse(getProfileDetails.Rows[0]["PartyId"].ToString());
            profileDetail.RealPageId = new Guid(getProfileDetails.Rows[0]["RealPageId"].ToString());
            profileDetail.FirstName = getProfileDetails.Rows[0]["FirstName"].ToString();
            profileDetail.MiddleName = getProfileDetails.Rows[0]["MiddleName"].ToString();
            profileDetail.LastName = getProfileDetails.Rows[0]["LastName"].ToString();
            profileDetail.Suffix = getProfileDetails.Rows[0]["Suffix"].ToString();
            profileDetail.Title = getProfileDetails.Rows[0]["Title"].ToString();
            profileDetail.PreferredContactMethodId = int.Parse(getProfileDetails.Rows[0]["PreferredContactMethodId"].ToString());

            return JsonConvert.SerializeObject(profileDetail);
        }



        public string DoPostUserSelectedSecurityQuestions(int _SecurityQuestionId, string _Answer)
		{
			SecurityQuestionAnswer securityQuestionanswer = new SecurityQuestionAnswer();

			securityQuestionanswer.SecurityQuestionId = _SecurityQuestionId;
			securityQuestionanswer.Answer = _Answer;

			return JsonConvert.SerializeObject(securityQuestionanswer);
		}

		public string DoPostSetUserSecurityQuestionsPayload(string _EnterpriseUserName = "", string _ActivityToken = "")
		{
			if (_EnterpriseUserName.Length <= 0)
			{
				_EnterpriseUserName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
			}
			string payload = DoPostEmailNotificationPayload(_EnterpriseUserName);

			EndPointUrl = HostUrl + Properties["RoleType"] + WebUtility.UrlEncode("User Role");
			ObjectListOutput<RoleType, ErrorData> roleType = JsonConvert.DeserializeObject<ObjectListOutput<RoleType, ErrorData>>(GetHttpWebResponseValue(
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")));

			EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(_EnterpriseUserName)
				+ "&newUserRegistrationToken=" + DoPostNewUserToken(payload, roleType.list[0].PartyRoleTypeId);

			if (_ActivityToken.Length <= 0)
			{
				_ActivityToken = JsonConvert.DeserializeObject<ValidateUserResponse>(GetHttpWebResponseValue(
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: ""))).ValidateUserToken;
			}

			EndPointUrl = HostUrl + Properties["UserAllSecurityQuestions"] + WebUtility.UrlEncode(_EnterpriseUserName);
			
			UsersAllSecurityQuestionResponse usersAllSecurityQuestionResponse 
				= JsonConvert.DeserializeObject<UsersAllSecurityQuestionResponse>(
					GetHttpWebResponseValue(GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: payload)));

			UserSecurityAnswer postSecurityAnswer = new UserSecurityAnswer();

			postSecurityAnswer.EnterpriseUserName = _EnterpriseUserName;
			
			postSecurityAnswer.ActivityToken = _ActivityToken;

			postSecurityAnswer.SecurityQuestionAnswers = new List<SecurityQuestionAnswer>();
			SecurityQuestionAnswer postSecurityQuestionAnswer = new SecurityQuestionAnswer();

			for (int countSecurityQuestions = 0; countSecurityQuestions < 3; countSecurityQuestions++)
			{
				postSecurityQuestionAnswer.SecurityQuestionId 
					= usersAllSecurityQuestionResponse.SecurityQuestions[countSecurityQuestions].SecurityQuestionId;
				postSecurityQuestionAnswer.Answer = "real";
				postSecurityAnswer.SecurityQuestionAnswers.Add(postSecurityQuestionAnswer);
				postSecurityQuestionAnswer = new SecurityQuestionAnswer();
			}

			return JsonConvert.SerializeObject(postSecurityAnswer);
		}

        public string DoGetTelecommunicationNumberPayload()
        {
            dbManager = new DatabaseController(Properties["dbConnection"]);
            DataTable telecommunicationnumberdetails = dbManager.executeQuery("SELECT pcm.PartyContactMechanismId , cmut.ContactMechanismUsageTypeID, "
                + "tele.[ContactMechanismID], tele.[CountryCode], tele.[AreaCode], tele.[PhoneNumber], "
                + "cmut.[ContactMechanismUsageTypeID], cmut.[ParentContactMechanismUsageTypeID], cmut.[Name] "
                + "FROM[" + Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsageType] cmut "
                + "INNER JOIN[" + Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsage] cmu "
                + "INNER JOIN[" + Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
                + "INNER JOIN[" + Properties["identityDatabase"] + "].[Enterprise].[TelecommunicationsNumber] tele "
                + "ON pcm.ContactMechanismId = tele.ContactMechanismId "
                + "ON pcm.partycontactmechanismid = cmu.partycontactmechanismid "
                + "ON cmut.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID "
                + " where pcm.partyid = (select partyid from[" + Properties["identityDatabase"] + "].[Ident].[UserLogin] where loginname = 'james@test.com') "
                + "ORDER BY partycontactmechanismid ASC");

            TelecommunicationNumber telecommuncationNumber = new TelecommunicationNumber();


            for (int countPartyContactMechanismId = 0; countPartyContactMechanismId < telecommunicationnumberdetails.Rows.Count; countPartyContactMechanismId++)
            {
                telecommuncationNumber.PartyContactMechanismId = int.Parse(telecommunicationnumberdetails.Rows[0]["PartyContactMechanismId"].ToString());
                telecommuncationNumber.ContactMechanismId = int.Parse(telecommunicationnumberdetails.Rows[0]["ContactMechanismId"].ToString());
                telecommuncationNumber.CountryCode = telecommunicationnumberdetails.Rows[0]["CountryCode"].ToString();
                telecommuncationNumber.AreaCode = telecommunicationnumberdetails.Rows[0]["AreaCode"].ToString();
                telecommuncationNumber.PhoneNumber = telecommunicationnumberdetails.Rows[0]["PhoneNumber"].ToString();
                telecommuncationNumber.ContactMechanismUsageTypeId = int.Parse(telecommunicationnumberdetails.Rows[0]["ContactMechanismUsageTypeId"].ToString());
                telecommuncationNumber.contactMechanismUsageType = new ContactMechanismUsageType();

                telecommuncationNumber.contactMechanismUsageType.ContactMechanismUsageTypeId = int.Parse(telecommunicationnumberdetails.Rows[0]["ContactMechanismUsageTypeId"].ToString());
                telecommuncationNumber.contactMechanismUsageType.ParentContactMechanismUsageTypeId = int.Parse(telecommunicationnumberdetails.Rows[0]["ParentContactMechanismUsageTypeId"].ToString());
                telecommuncationNumber.contactMechanismUsageType.Name = telecommunicationnumberdetails.Rows[0]["Name"].ToString();

            }
            return JsonConvert.SerializeObject(telecommuncationNumber);
        }
        public string DoGetOrganizationProducts()
        {

            ProductUI productUIDetail = new ProductUI();

            DataTable productUIDetails = dbManager.executeQuery("SELECT pr.ProductGUID, pr.ProductId, pst.[Name], ps.[Value], pr.Name AS ProductName, pr.Description AS ProductDescription, ptf.ProductTypeId AS FamilyId,"
            + " ptf.Name AS Family, pts.ProductTypeId AS SolutionId, pts.Name AS Solution, ps.ProductSettingId, ps.ProductSettingTypeId, pst.[Description] "
            + "FROM [Enterprise].Organization o "
            + "JOIN[Enterprise].[OrganizationProduct] op ON op.PartyId = o.PartyId "
            + "JOIN[Enterprise].[Product] pr ON pr.ProductId = op.ProductId "
            + "LEFT JOIN[Enterprise].[ProductType] pts ON pts.ProductTypeId = pr.ProductTypeId "
            + "LEFT JOIN[Enterprise].[ProductType] ptf ON ptf.ProductTypeId = pts.ParentProductTypeId "
            + "JOIN[Enterprise].[ProductConfiguration] pc ON pc.ConfigurationId = op.ConfigurationId "
            + "JOIN[Enterprise].[ProductSetting] ps ON ps.ProductSettingId = pc.ProductSettingId "
            + "JOIN[Enterprise].[ProductSettingType] pst ON pst.ProductSettingTypeId = ps.ProductSettingTypeId "
            + "JOIN[Enterprise].[Party] par ON par.PartyId = op.PartyId and par.PartyId = o.PartyId "
            + "WHERE par.RealpageId = '9E9410AE-2C41-47D2-81D1-109C08CD151C' and pr.ProductId = 1"
            + "ORDER BY pr.ProductId ASC");


            for (int countproductDetails = 0; countproductDetails < productUIDetails.Rows.Count; countproductDetails++)
            {
                productUIDetail.ProductId = int.Parse(productUIDetails.Rows[countproductDetails]["ProductId"].ToString());

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "TitleUniqueId")
                {
                    productUIDetail.TitleUniqueId = new Guid(productUIDetails.Rows[countproductDetails]["Value"].ToString());
                }

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "TitleId")
                {
                    productUIDetail.TitleId = productUIDetails.Rows[countproductDetails]["Value"].ToString();
                }

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "ClassName")
                {
                    productUIDetail.ClassName = productUIDetails.Rows[countproductDetails]["Value"].ToString();
                }

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "ClientId")
                {
                    productUIDetail.ClientId = productUIDetails.Rows[countproductDetails]["Value"].ToString();
                }

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "SettingsUrl")
                {
                    productUIDetail.SettingsUrl = productUIDetails.Rows[countproductDetails]["Value"].ToString();
                }

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "ProductUrl")
                {
                    productUIDetail.ProductUrl = productUIDetails.Rows[countproductDetails]["Value"].ToString();
                }

                productUIDetail.ActivitiesList = null;

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "IsNewTab")
                {
                    productUIDetail.IsNewTab = Convert.ToBoolean(int.Parse(productUIDetails.Rows[countproductDetails]["Value"].ToString()));
                }

                productUIDetail.ProductName = productUIDetails.Rows[countproductDetails]["ProductName"].ToString();
                productUIDetail.ProductDescription = productUIDetails.Rows[countproductDetails]["ProductDescription"].ToString();

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "IsFavorite")
                {
                    productUIDetail.IsFavorite = Convert.ToBoolean(int.Parse(productUIDetails.Rows[countproductDetails]["Value"].ToString()));
                }

                productUIDetail.FamilyId = int.Parse(productUIDetails.Rows[countproductDetails]["FamilyId"].ToString());
                productUIDetail.Family = productUIDetails.Rows[countproductDetails]["Family"].ToString();
                productUIDetail.SolutionId = int.Parse(productUIDetails.Rows[countproductDetails]["SolutionId"].ToString());
                productUIDetail.Solution = productUIDetails.Rows[countproductDetails]["Solution"].ToString();

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "Subsolution")
                {
                    productUIDetail.Subsolution = productUIDetails.Rows[countproductDetails]["Value"].ToString();
                }

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "IsResource")
                {
                    productUIDetail.IsResource = Convert.ToBoolean(int.Parse(productUIDetails.Rows[countproductDetails]["Value"].ToString()));
                }

                if (productUIDetails.Rows[countproductDetails]["Name"].ToString() == "LearnMore")
                {
                    productUIDetail.LearnMore = productUIDetails.Rows[countproductDetails]["Value"].ToString();
                }
            }

            return JsonConvert.SerializeObject(productUIDetail);
        }

        public string DoPutPersonasProductSettings(int _ProductId, string _Name, string _Value)
        {

            ProductSetting productSetting = new ProductSetting();

            productSetting.ProductId = _ProductId;
            productSetting.Name = _Name;
            productSetting.Value = _Value;


            return JsonConvert.SerializeObject(productSetting);
        }

		/* 
         * A method that DELETES the Newly Created User.
         * INPUT    : string _newUsername
         * OUTPUT   : 
         */
		public void DoDeleteNewUser(string _newUsername)
		{
			dbManager.executeQuery("delete [" + Properties["identityDatabase"] + "].[Ident].PasswordHistory\n"
				+ "where userid in (select userid from[" + Properties["identityDatabase"] + "].[Ident].userlogin\n"
				+ "where loginname = '" + _newUsername + "')\n"
				+ "delete[" + Properties["identityDatabase"] + "].[Ident].[UserLogin]\n"
				+ "where loginname = '" + _newUsername + "'\n"
				+ "select TOP 1 * from [" + Properties["identityDatabase"] + "].[Ident].PasswordHistory\n"
				+ "where userid in (select userid from[" + Properties["identityDatabase"] + "].[Ident].userlogin\n"
				+ "where loginname = '" + _newUsername + "')");
		}

		/* 
         * A method that returns the string payload to execute PATCH PatchUserLogins API.
         * INPUT    : 
         * OUTPUT   : GET securityQuestionsApiResponse in string format (or null for any Exception) 
         */
		public string DoPatchUserLoginsPayload()
		{
			UserLogin expectedUserLoginResponse;
			List<UserLogin> expectedUserLoginResponseList = new List<UserLogin>();
			string userLoginsUser, payload, newUserToken;

			for (int countAddUserLogin = 0; countAddUserLogin < 2; countAddUserLogin++)
			{
				userLoginsUser = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
				payload = DoPostEmailNotificationPayload(userLoginsUser);

				EndPointUrl = HostUrl + Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(userLoginsUser)
					+ "&newUserRegistrationToken=" + DoPostNewUserToken(payload, 401);
				newUserToken = JsonConvert.DeserializeObject<ValidateUserResponse>(GetHttpWebResponseValue(
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: ""))).ValidateUserToken;

				payload = DoPostSetPasswordPayload(userLoginsUser, null, newUserToken);

				EndPointUrl = HostUrl + Properties["SetPassword"];
				GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

				payload = DoGetUserLoginUser(userLoginsUser);
				EndPointUrl = HostUrl + Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;
				
				// Extract API's JSON Payload to be used as Expected Response
				expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(GetHttpWebResponseValue(
					GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "")));

				expectedUserLoginResponseList.Add(expectedUserLoginResponse);

				DoDeleteNewUser(userLoginsUser);
			}
			return JsonConvert.SerializeObject(expectedUserLoginResponseList);
		}

        public string DoGetProfile()
        {

            Profile profile = new Profile();
            //List<TelecommunicationNumber> listTelecommunicationNumber = new List<TelecommunicationNumber>();
            //TelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
            //ContactMechanismUsageType contactMechanismUsageType = new ContactMechanismUsageType();
            //PartyRole partyRole = new PartyRole();

            DataTable getProfiles = dbManager.executeQuery("SELECT DISTINCT pcm.PartyContactMechanismId, cm.ContactMechanismID, tm.CountryCode, tm.AreaCode, tm.PhoneNumber, 'Telecommunications Number' AS AddressType,	"
                + " cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeID, cmut.ParentContactMechanismUsageTypeID, cmut.Name, "
                + " i.PartyRoleId, i.PartyId, i.RoleTypeId, j.Name as PName, "
                + " d.PartyId, b.RealPageId, d.FirstName, d.MiddleName, d.LastName, d.Suffix, d.Title, d.PreferredContactMethodId "
                + " FROM Enterprise.ContactMechanismUsage cmu "
                + " INNER JOIN[Identity].Enterprise.ContactMechanismUsageType cmut ON cmu.ContactMechanismUsageTypeID = cmut.ContactMechanismUsageTypeID"
                + " INNER JOIN[Identity].Enterprise.PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
                + " INNER JOIN[Identity].Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
                + " INNER JOIN[Identity].Enterprise.TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID "
                + " INNER JOIN[Identity].Enterprise.Party p ON p.PartyId = pcm.PartyId "
                + " INNER JOIN[Identity].[Enterprise].[Party] b "
                + " INNER JOIN[Identity].[Person].[Person] d "
                + " INNER JOIN[Identity].[Ident].[UserLogin] a "
                + " INNER JOIN[Identity].[Enterprise].[PartyRole] i "
                + " INNER JOIN[Identity].[Enterprise].[RoleType] j "
                + " ON j.PartyRoleTypeId = i.RoleTypeId "
                + " ON i.PartyId = a.PartyId "
                + " ON a.PartyId = d.PartyId "
                + " ON b.PartyId = a.PartyId "
                + " ON b.PartyId = pcm.PartyId "
                + " WHERE b.partyid = (select partyid from [Identity].[Ident].[UserLogin] where loginname = 'james@test.com') "
                + "AND (pcm.ThruDate IS NULL OR pcm.ThruDate > GETUTCDATE());");


            profile.TelecommunicationNumber = new List<TelecommunicationNumber>();
            TelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
            telecommunicationNumber.contactMechanismUsageType = new ContactMechanismUsageType();
            PartyRole partyRole = new PartyRole();

            for (int countTelecommunicationNumber = 0; countTelecommunicationNumber < getProfiles.Rows.Count; countTelecommunicationNumber++)
            {
                telecommunicationNumber.PartyContactMechanismId = int.Parse(getProfiles.Rows[countTelecommunicationNumber]["PartyContactMechanismId"].ToString());
                telecommunicationNumber.ContactMechanismId = int.Parse(getProfiles.Rows[countTelecommunicationNumber]["ContactMechanismId"].ToString());
                telecommunicationNumber.CountryCode = getProfiles.Rows[countTelecommunicationNumber]["CountryCode"].ToString();
                telecommunicationNumber.AreaCode = getProfiles.Rows[countTelecommunicationNumber]["AreaCode"].ToString();
                telecommunicationNumber.PhoneNumber = getProfiles.Rows[countTelecommunicationNumber]["PhoneNumber"].ToString();

                telecommunicationNumber.contactMechanismUsageType = new ContactMechanismUsageType();

                telecommunicationNumber.contactMechanismUsageType.ContactMechanismUsageTypeId = int.Parse(getProfiles.Rows[0]["ContactMechanismUsageTypeId"].ToString());
                telecommunicationNumber.contactMechanismUsageType.ParentContactMechanismUsageTypeId = int.Parse(getProfiles.Rows[0]["ParentContactMechanismUsageTypeId"].ToString());
                telecommunicationNumber.contactMechanismUsageType.Name = getProfiles.Rows[0]["Name"].ToString();

                profile.TelecommunicationNumber.Add(telecommunicationNumber);
                telecommunicationNumber = new TelecommunicationNumber();

            }
            profile.PartyRole = new PartyRole();

            profile.PartyRole.PartyRoleId = int.Parse(getProfiles.Rows[0]["PartyRoleId"].ToString());
            profile.PartyRole.PartyId = int.Parse(getProfiles.Rows[0]["PartyId"].ToString());
            profile.PartyRole.RoleTypeId = int.Parse(getProfiles.Rows[0]["RoleTypeId"].ToString());
            profile.PartyRole.Name = getProfiles.Rows[0]["PName"].ToString();

            profile.PartyId = int.Parse(getProfiles.Rows[0]["PartyId"].ToString());
            profile.RealPageId = new Guid(getProfiles.Rows[0]["RealPageId"].ToString());
            profile.FirstName = getProfiles.Rows[0]["FirstName"].ToString();
            profile.MiddleName = getProfiles.Rows[0]["MiddleName"].ToString();
            profile.LastName = getProfiles.Rows[0]["LastName"].ToString();
            profile.Suffix = getProfiles.Rows[0]["Suffix"].ToString();
            profile.Title = getProfiles.Rows[0]["Title"].ToString();
            profile.PreferredContactMethodId = int.Parse(getProfiles.Rows[0]["PreferredContactMethodId"].ToString());

            return JsonConvert.SerializeObject(profile);
        }

        public string DoGetProductType()
        {
            List<ProductType> listProductType = new List<ProductType>();
            ProductType productType = new ProductType();

            dbManager = new DatabaseController(Properties["dbConnection"]);
            DataTable productTypeDetails = dbManager.executeQuery("SELECT * FROM [Identity].[Enterprise].[ProductType] ORDER BY ProductTypeId ASC");


            //for (int countproductType = 0; countproductType < productTypeDetails.Rows.Count; countproductType++)
            //{
            productType.ProductTypeGuid = new Guid(productTypeDetails.Rows[0]["ProductTypeGuid"].ToString());
            productType.ProductTypeId = int.Parse(productTypeDetails.Rows[0]["ProductTypeId"].ToString());
            productType.Name = productTypeDetails.Rows[0]["Name"].ToString();
            productType.Description = productTypeDetails.Rows[0]["Description"].ToString();
            //if (productTypeDetails.Rows[0]["ParentProductTypeId"].ToString() != null)
            //{
            //    productType.ParentProductTypeId = int.Parse(productTypeDetails.Rows[countproductType]["ParentProductTypeId"].ToString());
            //    productType.ParentProductTypeName = productTypeDetails.Rows[countproductType]["ParentProductTypeName"].ToString();
            //}
            //else
            //{
            //    productType.ParentProductTypeId = null;
            //    productType.ParentProductTypeName = null;                
            //}
            //}

            return JsonConvert.SerializeObject(productType);
        }

    }
}
