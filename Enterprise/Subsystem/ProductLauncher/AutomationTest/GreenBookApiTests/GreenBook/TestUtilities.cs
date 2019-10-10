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
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Ops;
using System.Threading;
using GreenBook.Models;
using Xunit;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;

namespace GreenBook
{
    class TestUtilities
    {
        JsonController jsonManager = new JsonController();
        DatabaseController dbManager;
        TestController testController;

        internal TestUtilities(TestController _testController)
        {
            testController = _testController;
            dbManager = new DatabaseController(testController.DbConnString);
        }

        public string DoGetUserRoleTypeId(string userType = "0")
        {
            // regular user - 401
            // regular user (no email) - 404
            // realpage system administrator - 400

            if (userType.ToLower() == "regular user")
                userType = "401";
            else if (userType.ToLower() == "regular user (no email)")
                userType = "404";
            else if (userType.ToLower() == "realpage system administrator")
                userType = "400";

            return userType;


            /*

                        testController.EndPointUrl = testController.HostUrl + testController.Properties["RoleType"] + WebUtility.UrlEncode("User Role");
                        testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

                        ObjectListOutput<RoleType, ErrorData> roleType = JsonConvert.DeserializeObject<ObjectListOutput<RoleType, ErrorData>>(testController.ResponseString);

                        if (userType.ToLower() == "regular user")
                        {
                            userType = roleType.list.First().PartyRoleTypeId.ToString();
                        }
                        else if (userType.ToLower() == "realpage system administrator")
                        {
                            userType = roleType.list[1].PartyRoleTypeId.ToString();
                        }
                        else if (userType.ToLower() == "regular user (no email)")
                        {
                            userType = roleType.list.Last().PartyRoleTypeId.ToString();
                        }

                        return userType;
            */
        }

        /* 
         * A method that returns GET securityQuestionsApiResponse for ForgotPassword flow APIs.
         * INPUT    : 
         * OUTPUT   : GET securityQuestionsApiResponse in string format (or null for any Exception) 
         */
        public string DoGetSecurityQuestionsApiResponseForPayload(string username = "")
        {
            username = string.IsNullOrEmpty(username) ? testController.Properties["enterpriseUsernameForForgotPassword"] : username;
            testController.EndPointUrl = testController.HostUrl + testController.Properties["GetSecurityQuestions"] + WebUtility.UrlEncode(username);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            return testController.ResponseString;
        }

        /* 
         * A method that returns the payload for POST /api/credential/ChangePassword.
         * INPUT    : string _EnterpriseUserName, string _ActivityToken, string _Password, string _CorrectAnswerToken, int _PasswordLength = 8
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
        public string DoPostChangePasswordPayload(string _GetSecurityQuestionsApiResponse, string _ActivityToken = "ValidActivityToken", string _Password = "ValidPassword", string _CorrectAnswerToken = "ValidCorrectAnswerToken", int _PasswordLength = 8)
        {
            string payload = DoPostVerifySecurityAnswersPayload(_GetSecurityQuestionsApiResponse);

            UserSecurityAnswer verifySecurityAnswersPayload = JsonConvert.DeserializeObject<UserSecurityAnswer>(payload);

            testController.EndPointUrl = testController.HostUrl + testController.Properties["VerifySecurityAnswers"];
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);
            SecurityAnswerResponse verifySecurityAnswersModel = JsonConvert.DeserializeObject<SecurityAnswerResponse>(testController.ResponseString);

            ChangePassword changePasswordRequest = new ChangePassword();
            changePasswordRequest.EnterpriseUserName = verifySecurityAnswersPayload.EnterpriseUserName;
            changePasswordRequest.ActivityToken = _ActivityToken != "ValidActivityToken" ? _ActivityToken : verifySecurityAnswersPayload.ActivityToken;
            changePasswordRequest.NewPassword = _Password != "ValidPassword" ? _Password : string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(_PasswordLength);
            changePasswordRequest.CorrectAnswerToken = _CorrectAnswerToken != "ValidCorrectAnswerToken" ? _CorrectAnswerToken : verifySecurityAnswersModel.CorrectAnswerToken;

            return JsonConvert.SerializeObject(changePasswordRequest);
        }

        /* 
         * A method that returns the payload for POST /api/credential/VerifySecurityAnswers.
         * INPUT    : string _EnterpriseUserName = "", string _ActivityToken = ""
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
        public string DoPostVerifySecurityAnswersPayload(string _GetSecurityQuestionsApiResponse, string _ActivityToken = "validActivityToken", string _EnterpriseUsername = "")
        {
            SecurityQuestionResponse getSecurityQuestions = JsonConvert.DeserializeObject<SecurityQuestionResponse>(_GetSecurityQuestionsApiResponse);
            if (!string.IsNullOrEmpty(getSecurityQuestions.ErrorReason))
            {
                if (getSecurityQuestions.ErrorReason.Contains("locked"))
                {
                    getSecurityQuestions = JsonConvert.DeserializeObject<SecurityQuestionResponse>(DoResetMaximumActivityAttempts(_EnterpriseUsername, testController.EndPointUrl, HttpVerb.Get));
                }
            }
            UserSecurityAnswer postSecurityAnswer = new UserSecurityAnswer();
            postSecurityAnswer.EnterpriseUserName = getSecurityQuestions.EnterpriseUserName;
            postSecurityAnswer.ActivityToken = _ActivityToken == "validActivityToken" ? getSecurityQuestions.ActivityToken : _ActivityToken;

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
            var personaId = JsonConvert.DeserializeObject<Persona>(DoGetPersona()).PersonaId;
            var realpageId = JsonConvert.DeserializeObject<Persona>(DoGetPersona(personaId)).RealPageId;
            testController.EndPointUrl = testController.HostUrl + testController.Properties["UserLogin"] + "/Status?statusTypeName=Unlocked&realPageId="
                + realpageId;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: "");

            testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: _HttpVerb, jsonPayload: _JsonPayload);
            return testController.ResponseString;
        }

        /* 
         * A method that creates a NewUser after infinite reset attempts 
         * and returns a new string response.
		 * INPUT    : string _EnterpriseUserName, string _EndPointUrl, HttpVerb _HttpVerb, string _JsonPayload
         * OUTPUT   : JSON Response in string format (or null for any Exception) 
         */
        public string DoPostNewUserForApiTests(string _EnterpriseUserName, string _EndPointUrl, HttpVerb _HttpVerb, string _JsonPayload = "")
        {
            string securityQuestionAnswerPayload = DoPostSetUserSecurityQuestionsPayload(_EnterpriseUserName);
            testController.EndPointUrl = testController.HostUrl + testController.Properties["SetUserSecurityQuestions"];
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: securityQuestionAnswerPayload);
            SetUserSecurityQuestionsResponse securityQuestionAnswerResponse
                = JsonConvert.DeserializeObject<SetUserSecurityQuestionsResponse>(testController.ResponseString);
            if (testController.EndPointUrl.Contains(testController.Properties["VerifySecurityAnswers"]) && _HttpVerb == HttpVerb.Post)
            {
                testController.EndPointUrl = testController.HostUrl + testController.Properties["GetSecurityQuestions"] + WebUtility.UrlEncode(securityQuestionAnswerResponse.EnterpriseUserName);
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
                string getSecurityQuestionsApiResponse = testController.ResponseString;

                _JsonPayload = DoPostVerifySecurityAnswersPayload(getSecurityQuestionsApiResponse);
            }

            testController.EndPointUrl = _EndPointUrl.Replace(WebUtility.UrlEncode(_EnterpriseUserName), WebUtility.UrlEncode(securityQuestionAnswerResponse.EnterpriseUserName));
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: _HttpVerb, jsonPayload: _JsonPayload);

            return testController.ResponseString;
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
        public string DoPostSetPasswordPayload(string _EnterpriseUserName, string _ActivityToken, string _Password = "AutoPassword", int _PasswordLength = 8)
        {
            SetPassword setPasswordRequest = new SetPassword();
            setPasswordRequest.ActivityToken = _ActivityToken;
            setPasswordRequest.EnterpriseUserName = _EnterpriseUserName;
            setPasswordRequest.NewPassword = string.IsNullOrEmpty(_Password) || _Password != "AutoPassword" ? _Password : string.Concat("Ab0!", Convert.ToBase64String(Guid.NewGuid().ToByteArray())).Remove(_PasswordLength);

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
            ObjectListOutput<ProfileDetail, IErrorData> personResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProfileDetail, IErrorData>>(GetOrganizationPersons(testController.CurrentlyLoggedInUser));

            long portfolioId = personResponse.list[0].PartyId;

            testController.EndPointUrl = testController.HostUrl + testController.Properties["PasswordPolicy"] + portfolioId;

            // Execute API
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response
            return JsonConvert.DeserializeObject<ObjectOutput<PasswordPolicy, ErrorData>>(testController.ResponseString);
        }





        /* 
         * A method that returns the payload for POST and PUT /api/Persons.
         * INPUT    : string _FirstName, string _LastName, string _MiddleName = "", string _Suffix = "", string _Title = ""
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
        public string DoPostPutPersonsPayload(string _FirstName = "AutoFirstName", string _LastName = "AutoLastName", string _MiddleName = "AutoMiddleName", string _Suffix = "Jr.", string _Title = "Mr.", HttpVerb _HttpVerb = HttpVerb.Post)
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
                //person.PersonaType = _persona;
            }
            else if (_HttpVerb == HttpVerb.Put)
            {
                testController.EndPointUrl = testController.HostUrl + testController.Properties["Persons"] + "/" + (JsonConvert.DeserializeObject<Person>(DoGetUserLoginUser(testController.Properties["enterpriseUsername6"])).RealPageId);
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                ObjectOutput<Person, IErrorData> personResponse = JsonConvert.DeserializeObject<ObjectOutput<Person, IErrorData>>(testController.ResponseString);

                person.PartyId = personResponse.obj.PartyId;
                person.RealPageId = personResponse.obj.RealPageId;
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
        public string DoPutUserProfileDetailPayload(string _newLoginName, string _userType = "regular user")
        {
            UserLogin newUserLogin = JsonConvert.DeserializeObject<UserLogin>(DoGetUserLoginUser(_newLoginName));
            ProfileDetail profileDetail = new ProfileDetail();
            profileDetail.userLogin.UserId = newUserLogin.UserId;
            profileDetail.userLogin.PartyId = newUserLogin.PartyId;
            profileDetail.userLogin.RealPageId = newUserLogin.RealPageId;
            profileDetail.userLogin.LoginName = _newLoginName;
            profileDetail.FirstName = "AutoFirstName_Edited";
            profileDetail.LastName = "AutoLastName_Edited";
            profileDetail.PartyId = newUserLogin.PartyId;
            profileDetail.RealPageId = newUserLogin.RealPageId;
            profileDetail.userLogin.FromDate = DateTime.Now;
            profileDetail.userLogin.IsActive = true;
            profileDetail.userLogin.Is3rdPartyIDP = false;

            // UserType			
            profileDetail.UserTypeId = int.Parse(DoGetUserRoleTypeId(_userType));

            return JsonConvert.SerializeObject(profileDetail);
        }

        /* 
         * A method that returns the string-format JSON that has RealPageId for GET and POST /api/persons/{realPageId}/electronicaddress.
         * INPUT    : 
         * OUTPUT   : JSON in string format (or null for any Exception) 
         */
        public string DoGetUserLoginUser(string _EnterpriseUsername)
        {

            testController.EndPointUrl = testController.HostIdentityUrl + testController.Properties["UserLoginUser"] + "?enterpriseUserName="
                + WebUtility.UrlEncode(_EnterpriseUsername);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            return testController.ResponseString;
        }

        public string GetLoggedInUserName()
        {
            string[] loggedInUser = testController.Properties["LoginUser"].Split('|');
            return loggedInUser[0];
        }

        public string GetRealPageId(string _EnterpriseUsername = "")
        {
            if (_EnterpriseUsername == "")
                _EnterpriseUsername = GetLoggedInUserName();

            // GB - GET /API/Persons
            testController.EndPointUrl = testController.HostUrl + testController.Properties["Persons"] + "?datafilter.filterBy={\"name\":\"" + _EnterpriseUsername + "\"}";
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, httpVerb: HttpVerb.Get, authHeader: "", jsonPayload: "");
            Thread.Sleep(20000);
            ObjectListOutput<ProfileDetail, IErrorData> personResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProfileDetail, IErrorData>>(testController.ResponseString);
            return personResponse.list[0].RealPageId.ToString();
        }

        public string GetPersonaId(string _EnterpriseUsername = "")
        {
            var realPageId = "";
            if (_EnterpriseUsername == "")
                realPageId = GetRealPageId();
            else
                realPageId = GetRealPageId(_EnterpriseUsername);

            testController.EndPointUrl = testController.HostUrl + testController.Properties["Person"] + "/persona" + "/{realPageId}";
            testController.EndPointUrl = testController.EndPointUrl.Replace("{realPageId}", GetRealPageId());
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);


            //ObjectOutput<IPersona, IErrorData> output = JsonConvert.DeserializeObject<ObjectOutput<IPersona, IErrorData>>(testController.ResponseString);
            //Persona persona = JsonConvert.DeserializeObject<Persona>(testController.ResponseString);

            dynamic output = JsonConvert.DeserializeObject<dynamic>(testController.ResponseString);
            return output.data.personaId;

            //return output.data.organization.partyId;


            /*
                        testController.EndPointUrl = testController.HostUrl + testController.Properties["LandingOrganization"] + "person" + "/{realPageId}";
                        testController.EndPointUrl = testController.EndPointUrl.Replace("{realPageId}", realPageId);

                        // Execute API
                        testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

                        // Extract API's JSON Response

                        LandingOrganizationModel personResponse = JsonConvert.DeserializeObject<LandingOrganizationModel>(testController.ResponseString);

                        //API - Get password Policy            
                        // Set up the API URL
                        string partyId = personResponse.data[0].partyId.ToString();
                          return partyId;
            */
        }

        public string GetPartyId(string _EnterpriseUsername = "")
        {
            /*
            var realPageId = "";
            if (_EnterpriseUsername == "")
                realPageId = GetRealPageId();
            else
                realPageId = GetRealPageId(_EnterpriseUsername);

            testController.EndPointUrl = testController.HostUrl + testController.Properties["Person"] + "/persona" + "/{realPageId}";
            testController.EndPointUrl = testController.EndPointUrl.Replace("{realPageId}", GetRealPageId());
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            //ObjectOutput<IPersona, IErrorData> output = JsonConvert.DeserializeObject<ObjectOutput<IPersona, IErrorData>>(testController.ResponseString);
            //Persona persona = JsonConvert.DeserializeObject<Persona>(testController.ResponseString);

            dynamic output = JsonConvert.DeserializeObject<dynamic>(testController.ResponseString);
            //return output.data.organization.partyId;

            */
            //**

            if (_EnterpriseUsername == "")
                _EnterpriseUsername = GetLoggedInUserName();

            // GB - GET /API/Persons
            testController.EndPointUrl = testController.HostUrl + testController.Properties["Persons"] + "?datafilter.filterBy={\"name\":\"" + _EnterpriseUsername + "\"}";
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, httpVerb: HttpVerb.Get, authHeader: "", jsonPayload: "");

            Thread.Sleep(5000);
            ObjectListOutput<ProfileDetail, IErrorData> personResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProfileDetail, IErrorData>>(testController.ResponseString);
            return personResponse.list[0].PartyId.ToString();


            //return output.data.organization.partyId;

            /*
                        testController.EndPointUrl = testController.HostUrl + testController.Properties["LandingOrganization"] + "person" + "/{realPageId}";
                        testController.EndPointUrl = testController.EndPointUrl.Replace("{realPageId}", realPageId);

                        // Execute API
                        testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

                        // Extract API's JSON Response

                        LandingOrganizationModel personResponse = JsonConvert.DeserializeObject<LandingOrganizationModel>(testController.ResponseString);

                        //API - Get password Policy            
                        // Set up the API URL
                        string partyId = personResponse.data[0].partyId.ToString();
                          return partyId;
            */
        }

        public string GetOrganizationPartyId(string _EnterpriseUsername = "")
        {
            var realPageId = "";
            if (_EnterpriseUsername == "")
                realPageId = GetRealPageId();
            else
                realPageId = GetRealPageId(_EnterpriseUsername);

            testController.EndPointUrl = testController.HostUrl + testController.Properties["Person"] + "/persona" + "/{realPageId}";
            testController.EndPointUrl = testController.EndPointUrl.Replace("{realPageId}", GetRealPageId());
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);


            //ObjectOutput<IPersona, IErrorData> output = JsonConvert.DeserializeObject<ObjectOutput<IPersona, IErrorData>>(testController.ResponseString);
            //Persona persona = JsonConvert.DeserializeObject<Persona>(testController.ResponseString);

            dynamic output = JsonConvert.DeserializeObject<dynamic>(testController.ResponseString);
            return output.data.organization.partyId;
        }

        public string GetOrganizationPersons(string _EnterpriseUsername)
        {
            var realPageId = GetRealPageId(testController.CurrentlyLoggedInUser);
            testController.EndPointUrl = testController.HostUrl + testController.Properties["LandingOrganization"] + "person" + "/{realPageId}";
            testController.EndPointUrl = testController.EndPointUrl.Replace("{realPageId}", realPageId);

            // Execute API
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response
            //LandingOrganizationModel personResponse = JsonConvert.DeserializeObject<LandingOrganizationModel>(testController.ResponseString);
            return testController.ResponseString;
        }

        public string GetPersons(string _EnterpriseUsername)
        {
            // GB - GET /API/Persons
            testController.EndPointUrl = testController.HostUrl + testController.Properties["Persons"] + "?datafilter.filterBy={\"name\":\"" + _EnterpriseUsername + "\"}";
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, httpVerb: HttpVerb.Get, authHeader: "", jsonPayload: "");

            //ObjectListOutput<ProfileDetail, IErrorData> personResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProfileDetail, IErrorData>>(testController.ResponseString);
            return testController.ResponseString;
        }



        /* 
         * A method that returns the string-format JSON that has RealPageId for GET and POST /api/persons/{realPageId}/electronicaddress.
         * INPUT    : 
         * OUTPUT   : JSON in string format (or null for any Exception) 
         */
        public string DoGetUserLogins(Guid _RealPageId)
        {
            testController.EndPointUrl = testController.HostUrl + testController.Properties["UserLogins"]
                + WebUtility.UrlEncode(_RealPageId.ToString());
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            return testController.ResponseString;
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
                dbManager.executeQuery("SELECT TOP 1 * from [" + testController.Properties["identityDatabase"] + "].[Ident].[Users] where IdentityProvider like 'IDP%'");

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
                dbManager.executeQuery("SELECT TOP 1 * from [" + testController.Properties["identityDatabase"] + "].[Ident].[Clients]");

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[ClientSecrets] WHERE ClientId = " + _ClientId);

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[ClientRedirectUris] WHERE ClientId = " + _ClientId);

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[ClientScopes] WHERE ClientId = " + _ClientId);

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[ClientPostLogoutRedirectUris] WHERE ClientId = " + _ClientId);

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[Tokens] "
                + "where SubjectCode = (SELECT TOP 1 SubjectCode FROM [" + testController.Properties["identityDatabase"] + "].[Ident].[Tokens]) "
                + "AND TokenType = (SELECT TOP 1 TokenType FROM [" + testController.Properties["identityDatabase"] + "].[Ident].[Tokens])");

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[Consents] "
                + "where SubjectCode = (SELECT TOP 1 SubjectCode FROM [" + testController.Properties["identityDatabase"] + "].[Ident].[Consents])");

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
        + "FROM	[" + testController.Properties["identityDatabase"] + "].[Ident].[PortfolioProductUser] PPU WITH(NOLOCK) "
        + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Ident].[PortfolioProductUserClaims] PPUC WITH(NOLOCK) ON PPU.PortfolioProductUserID = PPUC.PortfolioProductUserID "
        + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Ident].[Portfolio] P WITH(NOLOCK) ON P.PortfolioId = PPU.PortfolioID "
        + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Ident].[PortfolioProduct] PP WITH(NOLOCK) ON PPU.PortfolioId = PP.PortfolioID "
        + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Ident].[Product] PR WITH(NOLOCK) ON PP.ProductId = Pr.ProductId "
        + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Ident].[Clients] C WITH(NOLOCK) ON PR.ClientId = C.ClientId");

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[ScopeClaims]");

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[Scopes]");

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[ScopeSecrets]");

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[IdentityProviderType]");

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
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Ident].[IdentityProviderSettingType]");

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
            testController.EndPointUrl = testController.HostIdentityUrl + testController.Properties["IdentityProviderType"] + "?enterpriseUserName=" + WebUtility.UrlEncode(testController.Properties["enterpriseUsername"]);

            // Execute API
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            IdentityProviderTypeOutput identityProviderTypeOutput = JsonConvert.DeserializeObject<IdentityProviderTypeOutput>(testController.ResponseString);
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
                dbManager.executeQuery("SELECT TOP 1 * from [" + testController.Properties["identityDatabase"] + "].[Ident].[Activity] "
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
                testController.EndPointUrl = testController.HostUrl + testController.Properties["ContactMechanismUsageTypes"] + "?ContactMechanismUsageTypeName=" + Uri.EscapeDataString("Email Notification");
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                ObjectListOutput<ContactMechanismUsageType, IErrorData> ContactMechanismUsageTypes
                    = JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(testController.ResponseString);

                linkElectronicAddress.PartyContactMechanism.FromDate = DateTime.Now;
                linkElectronicAddress.PartyContactMechanism.ThruDate = DateTime.Now.AddYears(1);
                linkElectronicAddress.ContactMechanismUsageType.ContactMechanismUsageTypeId = ContactMechanismUsageTypes.list[0].ContactMechanismUsageTypeId;
                linkElectronicAddress.ElectronicAddress.AddressType = ContactMechanismUsageTypes.list[0].Name;
                linkElectronicAddress.ElectronicAddress.AddressString = Guid.NewGuid().ToString().Remove(6) + "@apiTest.com";
            }
            else if (_HttpVerb == HttpVerb.Put)
            {
                UserLogin userLogin = JsonConvert.DeserializeObject<UserLogin>(DoGetUserLoginUser(testController.CurrentlyLoggedInUser));
                testController.EndPointUrl = testController.HostUrl + testController.Properties["ElectronicAddress"].Replace("{realPageId}", GetRealPageId(testController.CurrentlyLoggedInUser));
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                ObjectListOutput<ElectronicAddress, IErrorData> electronicAddress
                    = JsonConvert.DeserializeObject<ObjectListOutput<ElectronicAddress, IErrorData>>(testController.ResponseString);

                linkElectronicAddress.PartyContactMechanism = new PartyContactMechanism();
                linkElectronicAddress.PartyContactMechanism.PartyContactMechanismId = electronicAddress.list[0].PartyContactMechanismId;
                //linkElectronicAddress.PartyContactMechanism.PartyId = userLogin.PartyId;
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
            testController.EndPointUrl = testController.HostUrl + testController.Properties["Profiles"] + GetRealPageId(testController.CurrentlyLoggedInUser);

            //Execute API
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Extract API's JSON Response
            ObjectOutput<Profile, IErrorData> profileOutput = JsonConvert.DeserializeObject<ObjectOutput<Profile, IErrorData>>(testController.ResponseString);


            Profile profile = new Profile();

            //profile.telecommunicationNumber = profileOutput.obj.telecommunicationNumber;
            //profile.partyRole = new PartyRole();
            //profile.partyRole.PartyRoleId = profileOutput.obj.partyRole.PartyRoleId;
            //profile.partyRole.PartyId = profileOutput.obj.partyRole.PartyId;
            //profile.partyRole.RoleTypeId = profileOutput.obj.partyRole.RoleTypeId;
            //profile.partyRole.Name = profileOutput.obj.partyRole.Name;
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
                testController.EndPointUrl = testController.HostUrl + testController.Properties["ContactMechanismUsageTypes"] + "?ContactMechanismUsageTypeName=" + Uri.EscapeDataString("Contact Mechanism Type");
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                ObjectListOutput<ContactMechanismUsageType, IErrorData> ContactMechanismUsageTypes
                    = JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(testController.ResponseString);

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
                /*
				DataTable postalAddressDetails = new DataTable();
				string selectPostalAddresDetails = "SELECT TOP 1 cmb.[ContactMechanismBoundaryId]\n"
					+ ", cmb.[FromDate] as [ContactMechanismBoundaryFromDate], cmb.[ThruDate] as [ContactMechanismBoundaryThruDate]\n"
					+ ", gb.[GeographicBoundaryId], gb.[Name] as [GeographicBoundaryName], gb.[GeographicBoundaryCode], gb.[Abbreviation]\n"
					+ ", gbt.[GeographicBoundaryTypeId], gbt.[Name] as [GeographicBoundaryTypeName]\n"
					+ "from[" + testController.Properties["identityDatabase"] + "].Enterprise.PartyContactMechanism pcm\n"
					+ "JOIN[" + testController.Properties["identityDatabase"] + "].Enterprise.Party p ON p.PartyId = pcm.PartyId\n"
					+ "join [" + testController.Properties["identityDatabase"] + "].Enterprise.ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId\n"
					+ "join[" + testController.Properties["identityDatabase"] + "].Enterprise.ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID\n"
					+ "JOIN[" + testController.Properties["identityDatabase"] + "].Enterprise.GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId\n"
					+ "JOIN[" + testController.Properties["identityDatabase"] + "].Enterprise.GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId\n"
					+ "WHERE p.RealPageId = (select RealPageId from [" + testController.Properties["identityDatabase"] + "].Enterprise.Party\n"
					+ "WHERE partyid = (select partyid from [" + testController.Properties["identityDatabase"] + "].ident.userLogin\n"
					+ "WHERE loginName = '" + _PostalAddressUsername + "')) \n"
					+ "AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE())\n"
					+ "ORDER BY gb.[GeographicBoundaryId] desc";

				postalAddressDetails = dbManager.executeQuery(selectPostalAddresDetails);
*/
                var realPageId = GetRealPageId(testController.CurrentlyLoggedInUser);
                testController.EndPointUrl = testController.HostUrl + testController.Properties["PostalAddress"].Replace("{realPageId}", realPageId);

                // Execute API
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

                // Extract API's JSON Response

                ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress, IErrorData> postalAddress
                    = JsonConvert.DeserializeObject<ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.PostalAddress, IErrorData>>(testController.ResponseString);

                /*
                                UserLogin userLogin = JsonConvert.DeserializeObject<UserLogin>(DoGetUserLoginUser(testController.CurrentlyLoggedInUser));
                                testController.EndPointUrl = testController.HostUrl + testController.Properties["PostalAddress"].Replace("{realPageId}", userLogin.RealPageId.ToString());
                                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                                ObjectListOutput<PostalAddress, IErrorData> postalAddress
                                = JsonConvert.DeserializeObject<ObjectListOutput<PostalAddress, IErrorData>>(testController.ResponseString);
                    */
                linkPostalAddress.PartyContactMechanism.PartyContactMechanismId = postalAddress.list[0].PartyContactMechanismId;
                linkPostalAddress.PartyContactMechanism.PartyId = postalAddress.list[0].PartyContactMechanismId;
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

                //linkPostalAddress.ContactMechanismBoundary.ContactMechanismBoundaryId = int.Parse(postalAddress.list[0].party.ToString());
                //linkPostalAddress.ContactMechanismBoundary.GeographicBoundaryId = int.Parse(postalAddressDetails.Rows[0]["GeographicBoundaryId"].ToString());

                if (postalAddress.list[0].ContactMechanismId.ToString().Length < 1)
                {
                    linkPostalAddress.ContactMechanismBoundary.FromDate = DateTime.Now;
                }
                else
                {
                    linkPostalAddress.ContactMechanismBoundary.FromDate = Convert.ToDateTime(postalAddress.list[0].ContactMechanismUsageTypeId.ToString());
                }

                if (postalAddress.list[0].contactMechanismUsageType.ToString().Length < 1)
                {
                    linkPostalAddress.ContactMechanismBoundary.ThruDate = DateTime.Now;
                }
                else
                {
                    linkPostalAddress.ContactMechanismBoundary.ThruDate = DateTime.Now;
                }

                GeographicBoundary newGeographicBoundary = new GeographicBoundary();
                /*
                                for (int countGeographicBoundary = 0; countGeographicBoundary < postalAddress.list.Count; countGeographicBoundary++)
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
                */
                return JsonConvert.SerializeObject(linkPostalAddress);
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
                    + "FROM [" + testController.Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsageType] WHERE Name = '" + _Name + "')";
            }

            DataTable contactMechanismUsageType =
                dbManager.executeQuery("SELECT * from [" + testController.Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsageType]" + _Name);

            return contactMechanismUsageType;
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
                //userlogin.LoginName = string.Concat(Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + "@test.com");
                userlogin.LoginName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
                userlogin.FromDate = Convert.ToDateTime(_fromDate);
                userlogin.ThruDate = null;

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
                userlogin.ThruDate = null;

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
                + "FROM[" + testController.Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsageType] cmut "
                + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsage] cmu "
                + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
                + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Enterprise].[TelecommunicationsNumber] tele "
                + "ON pcm.ContactMechanismId = tele.ContactMechanismId "
                + "ON pcm.partycontactmechanismid = cmu.partycontactmechanismid "
                + "ON cmut.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID "
                + " where pcm.partyid = (select partyid from[" + testController.Properties["identityDatabase"] + "].[Ident].[UserLogin] where loginname = '" + _TelecommunicationNumberUsername + "') "
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
                testController.EndPointUrl = testController.HostUrl + testController.Properties["ContactMechanismUsageTypes"] + "?ContactMechanismUsageTypeName=" + Uri.EscapeDataString("Phone Type");
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                ObjectListOutput<ContactMechanismUsageType, IErrorData> ContactMechanismUsageTypes
                    = JsonConvert.DeserializeObject<ObjectListOutput<ContactMechanismUsageType, IErrorData>>(testController.ResponseString);

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
                    + "FROM    [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsageType cmut "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsage cmu ON cmut.ContactMechanismUsageTypeId = cmu.ContactMechanismUsageTypeId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ElectronicAddress ea ON ea.ContactMechanismID = cm.ContactMechanismID "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].Party p ON p.PartyId = pcm.PartyId "
                    + "WHERE   p.RealPageId = '" + _RealPageId + "' AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE()) UNION ALL "
                    + "SELECT  pcm.PartyContactMechanismId,	cm.ContactMechanismID,pa.StreetAddress1 AS AddressString,"
                    + "'Street Address' AS AddressType,	cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId,"
                    + "cmut.ParentContactMechanismUsageTypeID,cmut.Name "
                    + "FROM    [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsageType cmut "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsage cmu ON cmut.ContactMechanismUsageTypeId = cmu.ContactMechanismUsageTypeId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].[StreetAddress] pa ON pa.ContactMechanismID = cm.ContactMechanismID "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].Party p ON p.PartyId = pcm.PartyId "
                    + "WHERE   p.RealPageId = '" + _RealPageId + "' "
                    + "AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE()) UNION ALL "
                    + "SELECT  pcm.PartyContactMechanismId,	cm.ContactMechanismID,gb.Name AS AddressString, "
                    + "gbt.Name AS AddressType,	cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId,"
                    + "cmut.ParentContactMechanismUsageTypeID,cmut.Name "
                    + "FROM    [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsageType cmut "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsage cmu ON cmut.ContactMechanismUsageTypeId = cmu.ContactMechanismUsageTypeId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanismBoundary cmb ON cmb.ContactMechanismId = cm.ContactMechanismID "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].GeographicBoundary gb ON gb.GeographicBoundaryId = cmb.GeographicBoundaryId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].GeographicBoundaryType gbt ON gbt.GeographicBoundaryTypeId = gb.GeographicBoundaryTypeId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].Party p ON p.PartyId = pcm.PartyId "
                    + "WHERE   p.RealPageId = '" + _RealPageId + "' AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE()) UNION ALL "
                    + "SELECT  pcm.PartyContactMechanismId,	cm.ContactMechanismID,"
                    + "CONCAT(tm.CountryCode, tm.AreaCode, tm.PhoneNumber) AS AddressString,"
                    + "'Telecommunications Number' AS AddressType,"
                    + "cmu.ContactMechanismUsageTypeID AS ContactMechanismUsageTypeId,"
                    + "cmut.ParentContactMechanismUsageTypeID,	cmut.Name "
                    + "FROM    [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsageType cmut "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanismUsage cmu ON cmut.ContactMechanismUsageTypeId = cmu.ContactMechanismUsageTypeId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].PartyContactMechanism pcm ON pcm.PartyContactMechanismId = cmu.PartyContactMechanismID "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].ContactMechanism cm ON cm.ContactMechanismID = pcm.ContactMechanismId "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].TelecommunicationsNumber tm ON tm.ContactMechanismID = cm.ContactMechanismID "
                    + "JOIN [" + testController.Properties["identityDatabase"] + "].[Enterprise].Party p ON p.PartyId = pcm.PartyId WHERE   p.RealPageId = '" + _RealPageId + "' "
                    + "AND(pcm.ThruDate IS NULL OR pcm.ThruDate >= GETUTCDATE())");

            return contactMechanismList;
        }

        /* 
         * A method that returns the payload for POST /api/emailnotification/newprofile.
         * INPUT    : string _EnterpriseUserName, string _OrganizationRealPageId
         * OUTPUT   : Payload in string format (or null for any Exception) 
         */
        public string DoPostNewUserPayload(string _EnterpriseUserName, string _FirstName = "AutoFirstName", string _LastName = "AutoLastName", string _Persona = "", string userType = "regular user")
        {
            string PostNewUserRequest = jsonManager.LoadJsonAsString(testController.DataPath + "PostNewUserRequest.json");

            return PostNewUserRequest.Replace("<firstName>", _FirstName).Replace("<lastName>", _LastName)
                .Replace("<password>", userType.ToLower() == "regular user (no email)" ? "P@ssw0rd" : "")
                .Replace("<userTypeId>", int.Parse(DoGetUserRoleTypeId(userType)).ToString())
                .Replace("<loginName>", _EnterpriseUserName).Replace("<productId>", "1").Replace("<role1>", "5502065").Replace("<role2>", "2622510").Replace("<property1>", "7063696");
        }

        public string DoPostNewUserPerfPayload(string _EnterpriseUserName, string _FirstName = "AutoFirstName", string _LastName = "AutoLastName", string _Persona = "", string userType = "regular user")
        {
            string PostNewUserRequest = jsonManager.LoadJsonAsString(testController.DataPath + "PostNewUserRequestPerf.json");

            return PostNewUserRequest.Replace("<firstName>", _FirstName).Replace("<lastName>", _LastName)
                .Replace("<password>", userType.ToLower() == "regular user (no email)" ? "P@ssw0rd" : "")
                .Replace("<userTypeId>", int.Parse(DoGetUserRoleTypeId(userType)).ToString())
                .Replace("<loginName>", _EnterpriseUserName);
        }


        /* 
         * A method that returns the payload for POST /api/newuser.
         * INPUT    : string _Payload
         * OUTPUT   : NewUserToken in string format (or null for any Exception) 
         */
        public CreateUserResponse<ErrorData> DoPostNewUser(string _Payload, int _UserType = 0)
        {
            testController.EndPointUrl = testController.HostUrl + testController.Properties["User"] + "?userType=" + _UserType;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: _Payload);
            return JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(testController.ResponseString);
        }

        /* 
         * A method that returns the payload for POST /api/newuser.
         * INPUT    : string _Payload
         * OUTPUT   : NewUserToken in string format (or null for any Exception) 
         */
        public string DoPostNewUserToken(string _Payload, int _UserType = 0)
        {
            testController.EndPointUrl = testController.HostUrl + testController.Properties["User"] + "?userType=" + _UserType;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: _Payload);
            var personaId = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(testController.ResponseString).PersonaId;
            var realpageId = JsonConvert.DeserializeObject<Persona>(DoGetPersona(personaId)).RealPageId;

            // Execute API
            testController.EndPointUrl = testController.HostUrl + testController.Properties["ProfileDetails"] + "?realPageId=" + realpageId;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            // Extract API's JSON Response			
            var profileResponse = JsonConvert.DeserializeObject<dynamic>(testController.ResponseString);
            return profileResponse.data.verificationActivityToken;
        }

        public string DoGetProfileDetailsPayload(string _realpageId = "")
        {
            ProfileDetail profileDetail = new ProfileDetail();
            profileDetail.Avatar = null;

            //Userlogin
            testController.EndPointUrl = testController.HostUrl + testController.Properties["UserLogins"] + _realpageId;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            UserLogin userLoginResponse = JsonConvert.DeserializeObject<UserLogin>(testController.ResponseString);

            profileDetail.userLogin.UserId = userLoginResponse.UserId;
            profileDetail.userLogin.PartyId = userLoginResponse.PartyId;
            profileDetail.userLogin.RealPageId = userLoginResponse.RealPageId;
            profileDetail.userLogin.LoginName = userLoginResponse.LoginName;
            profileDetail.userLogin.LoginNameType = userLoginResponse.LoginNameType;
            profileDetail.userLogin.IsActive = userLoginResponse.IsActive;
            profileDetail.userLogin.IsLocked = userLoginResponse.IsLocked;
            //profileDetail.userLogin.IsTainted = userLoginResponse.IsTainted;
            profileDetail.userLogin.IsPending = userLoginResponse.IsPending;
            profileDetail.userLogin.IsExpired = userLoginResponse.IsExpired;
            profileDetail.userLogin.PasswordModifiedDate = userLoginResponse.PasswordModifiedDate;
            profileDetail.userLogin.FromDate = userLoginResponse.FromDate;
            profileDetail.userLogin.ThruDate = userLoginResponse.ThruDate;
            //profileDetail.userLogin.StatusSetDate = userLoginResponse.StatusSetDate;
            profileDetail.userLogin.LastLogin = userLoginResponse.LastLogin;
            profileDetail.userLogin.IsSuperUser = userLoginResponse.IsSuperUser;
            profileDetail.userLogin.Status = userLoginResponse.Status;
            profileDetail.userLogin.Password = userLoginResponse.Password;
            profileDetail.userLogin.UserRoleType = userLoginResponse.UserRoleType;
            profileDetail.userLogin.Is3rdPartyIDP = userLoginResponse.Is3rdPartyIDP;

            //Organization
            testController.EndPointUrl = testController.HostUrl + testController.Properties["Organization"].Replace("{realPageId}", _realpageId);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectListOutput<OrganizationTestModel, ErrorData> organizationResponse = JsonConvert.DeserializeObject<ObjectListOutput<OrganizationTestModel, ErrorData>>(testController.ResponseString);

            //for (int countOrganization = 0; countOrganization < organizationResponse.list.Count; countOrganization++)
            //{ 
            profileDetail.organization.Add(organizationResponse.list.First());
            //}

            //Contactmechanism
            testController.EndPointUrl = testController.HostUrl + testController.Properties["ContactMechanism"].Replace("{realPageId}", _realpageId);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectListOutput<CommonAddress, IErrorData> contactMechanismResponse = JsonConvert.DeserializeObject<ObjectListOutput<CommonAddress, IErrorData>>(testController.ResponseString);

            profileDetail.contactMechanism.Add(contactMechanismResponse.list.First());

            //SummaryCount
            testController.EndPointUrl = testController.HostUrl + testController.Properties["ProductFamilies"] + "?personRealPageId=" + _realpageId;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectListOutput<ProductFamily, IErrorData> productFamiliesResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProductFamily, IErrorData>>(testController.ResponseString);

            //PartyRole
            testController.EndPointUrl = testController.HostUrl + testController.Properties["Profiles"] + _realpageId;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectOutput<Profile, IErrorData> profileResponse = JsonConvert.DeserializeObject<ObjectOutput<Profile, IErrorData>>(testController.ResponseString);

            profileDetail.PartyRole.PartyRoleId = profileResponse.obj.PartyRole.PartyRoleId;
            profileDetail.PartyRole.PartyId = profileResponse.obj.PartyRole.PartyId;
            profileDetail.PartyRole.RoleTypeId = profileResponse.obj.PartyRole.RoleTypeId;
            profileDetail.PartyRole.Name = profileResponse.obj.PartyRole.Name;

            //TelecommunicationNumber
            testController.EndPointUrl = testController.HostUrl + testController.Properties["TelecommunicationNumber"].Replace("{realPageId}", _realpageId);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            TelecommunicationNumber telecommunicationNumberResponse = JsonConvert.DeserializeObject<TelecommunicationNumber>(testController.ResponseString);

            //User
            testController.EndPointUrl = testController.HostUrl + testController.Properties["User"] + "/" + _realpageId;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectOutput<ProfileDetail, ErrorData> userResponse = JsonConvert.DeserializeObject<ObjectOutput<ProfileDetail, ErrorData>>(testController.ResponseString);

            profileDetail.Password = userResponse.obj.Password;
            profileDetail.NotificationEmail = userResponse.obj.NotificationEmail;
            profileDetail.AuthenticationType = userResponse.obj.AuthenticationType;
            profileDetail.UserTypeId = userResponse.obj.UserTypeId;
            profileDetail.PartyId = userResponse.obj.PartyId;
            profileDetail.RealPageId = userResponse.obj.RealPageId;
            profileDetail.FirstName = userResponse.obj.FirstName;
            profileDetail.MiddleName = userResponse.obj.MiddleName;
            profileDetail.LastName = userResponse.obj.LastName;
            profileDetail.Suffix = userResponse.obj.Suffix;
            profileDetail.Title = userResponse.obj.Title;
            profileDetail.PreferredContactMethodId = userResponse.obj.PreferredContactMethodId;

            return JsonConvert.SerializeObject(profileDetail);
        }



        public string DoPostUserSelectedSecurityQuestions(int _SecurityQuestionId = 0, string _Answer = "", int _CountSecurityQuestions = 3)
        {

            testController.EndPointUrl = testController.HostUrl + testController.Properties["UserSelectedSecurityQuestions"];
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectListOutput<SecurityQuestion, ErrorData> SecurityQuestionResponse
                = JsonConvert.DeserializeObject<ObjectListOutput<SecurityQuestion, ErrorData>>(testController.ResponseString);


            SecurityQuestionAnswer securityQuestionanswer = new SecurityQuestionAnswer();
            List<SecurityQuestionAnswer> listSecurityQuestionAnswers = new List<SecurityQuestionAnswer>();


            for (int countSecurityQuestions = 0; countSecurityQuestions < _CountSecurityQuestions; countSecurityQuestions++)
            {

                securityQuestionanswer.SecurityQuestionId = SecurityQuestionResponse.list[countSecurityQuestions].SecurityQuestionId;
                securityQuestionanswer.Answer = "Real";
                listSecurityQuestionAnswers.Add(securityQuestionanswer);
                securityQuestionanswer = new SecurityQuestionAnswer();

            }

            return JsonConvert.SerializeObject(listSecurityQuestionAnswers);
        }

        public string DoPostSetUserSecurityQuestionsPayload(string _EnterpriseUserName = "", string _ActivityToken = "", int _CountSecurityQuestions = 3)
        {

            testController.EndPointUrl = testController.HostUrl + testController.Properties["UserAllSecurityQuestions"] + WebUtility.UrlEncode(_EnterpriseUserName);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            UserAllSecurityQuestionResponse usersAllSecurityQuestionResponse
                = JsonConvert.DeserializeObject<UserAllSecurityQuestionResponse>(testController.ResponseString);


            UserSecurityAnswer postSecurityAnswer = new UserSecurityAnswer();
            postSecurityAnswer.EnterpriseUserName = _EnterpriseUserName;
            postSecurityAnswer.ActivityToken = _ActivityToken;
            postSecurityAnswer.SecurityQuestionAnswers = new List<SecurityQuestionAnswer>();
            SecurityQuestionAnswer postSecurityQuestionAnswer = new SecurityQuestionAnswer();

            for (int countSecurityQuestions = 0; countSecurityQuestions < _CountSecurityQuestions; countSecurityQuestions++)
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
            dbManager = new DatabaseController(testController.Properties["dbConnection"]);
            DataTable telecommunicationnumberdetails = dbManager.executeQuery("SELECT pcm.PartyContactMechanismId , cmut.ContactMechanismUsageTypeID, "
                + "tele.[ContactMechanismID], tele.[CountryCode], tele.[AreaCode], tele.[PhoneNumber], "
                + "cmut.[ContactMechanismUsageTypeID], cmut.[ParentContactMechanismUsageTypeID], cmut.[Name] "
                + "FROM[" + testController.Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsageType] cmut "
                + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Enterprise].[ContactMechanismUsage] cmu "
                + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Enterprise].[PartyContactMechanism] pcm "
                + "INNER JOIN[" + testController.Properties["identityDatabase"] + "].[Enterprise].[TelecommunicationsNumber] tele "
                + "ON pcm.ContactMechanismId = tele.ContactMechanismId "
                + "ON pcm.partycontactmechanismid = cmu.partycontactmechanismid "
                + "ON cmut.ContactMechanismUsageTypeID = cmu.ContactMechanismUsageTypeID "
                + " where pcm.partyid = (select partyid from[" + testController.Properties["identityDatabase"] + "].[Ident].[UserLogin] where loginname = 'james@test.com') "
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

            DataTable productUIDetails = dbManager.executeQuery("SELECT pr.ProductGUID, pr.ProductId, pst.Name, ps.Value, pr.Name AS ProductName, pr.Description AS ProductDescription, ptf.ProductTypeId AS FamilyId,"
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
                payload = DoPostNewUserPayload(userLoginsUser);

                testController.EndPointUrl = testController.HostUrl + testController.Properties["User"] + "/Validate?enterpriseUserName=" + WebUtility.UrlEncode(userLoginsUser)
                    + "&newUserRegistrationToken=" + DoPostNewUserToken(payload, 401);
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                newUserToken = JsonConvert.DeserializeObject<ValidateUserResponse>(testController.ResponseString).ValidateUserToken;

                payload = DoPostSetPasswordPayload(userLoginsUser, null, newUserToken);

                testController.EndPointUrl = testController.HostUrl + testController.Properties["SetPassword"];
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

                payload = DoGetUserLoginUser(userLoginsUser);
                testController.EndPointUrl = testController.HostUrl + testController.Properties["UserLogins"] + JsonConvert.DeserializeObject<UserLogin>(payload).RealPageId;

                // Extract API's JSON Payload to be used as Expected Response
                testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                expectedUserLoginResponse = JsonConvert.DeserializeObject<UserLogin>(testController.ResponseString);

                expectedUserLoginResponseList.Add(expectedUserLoginResponse);


            }
            return JsonConvert.SerializeObject(expectedUserLoginResponseList);
        }

        /* 
         * A method that returns the string-format JSON that has RealPageId for GET and POST /api/persons/{realPageId}/electronicaddress.
         * INPUT    : 
         * OUTPUT   : JSON in string format (or null for any Exception) 
         */
        public string DoGetPersona(long _PersonaId = 0)
        {
            if (_PersonaId <= 0)
            {
                testController.EndPointUrl = testController.HostUrl + testController.Properties["Persona"];
            }
            else
            {
                testController.EndPointUrl = testController.HostUrl + testController.Properties["Persona"]
                    + "?personaId=" + _PersonaId;
            }
            //Thread.Sleep(5000);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            return testController.ResponseString;
        }

        public string DoGetProfile(string _realpageId = "")
        {

            Profile profile = new Profile();

            //Userlogin
            testController.EndPointUrl = testController.HostUrl + testController.Properties["UserLogins"] + _realpageId;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            UserLogin userLoginResponse = JsonConvert.DeserializeObject<UserLogin>(testController.ResponseString);

            profile.userLogin.UserId = userLoginResponse.UserId;
            profile.userLogin.PartyId = userLoginResponse.PartyId;
            profile.userLogin.RealPageId = userLoginResponse.RealPageId;
            profile.userLogin.LoginName = userLoginResponse.LoginName;
            profile.userLogin.LoginNameType = userLoginResponse.LoginNameType;
            profile.userLogin.IsActive = userLoginResponse.IsActive;
            profile.userLogin.IsLocked = userLoginResponse.IsLocked;
            //profile.userLogin.IsTainted = userLoginResponse.IsTainted;
            profile.userLogin.IsPending = userLoginResponse.IsPending;
            profile.userLogin.IsExpired = userLoginResponse.IsExpired;
            profile.userLogin.PasswordModifiedDate = userLoginResponse.PasswordModifiedDate;
            profile.userLogin.FromDate = userLoginResponse.FromDate;
            profile.userLogin.ThruDate = userLoginResponse.ThruDate;
            //profile.userLogin.StatusSetDate = userLoginResponse.StatusSetDate;
            profile.userLogin.LastLogin = userLoginResponse.LastLogin;
            profile.userLogin.IsSuperUser = userLoginResponse.IsSuperUser;
            profile.userLogin.Status = userLoginResponse.Status;
            profile.userLogin.Password = userLoginResponse.Password;
            profile.userLogin.UserRoleType = userLoginResponse.UserRoleType;
            profile.userLogin.Is3rdPartyIDP = userLoginResponse.Is3rdPartyIDP;

            //TelecommunicationNumber
            testController.EndPointUrl = testController.HostUrl + testController.Properties["TelecommunicationNumber"].Replace("{realPageId}", _realpageId);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectListOutput<TelecommunicationNumber, IErrorData> telecommunicationNumberResponse = JsonConvert.DeserializeObject<ObjectListOutput<TelecommunicationNumber, IErrorData>>(testController.ResponseString);

            profile.TelecommunicationNumber = new List<TelecommunicationNumber>();
            TelecommunicationNumber telecommunicationNumber = new TelecommunicationNumber();
            profile.TelecommunicationNumber.Add(telecommunicationNumberResponse.list[0]);

            //PartyRole
            testController.EndPointUrl = testController.HostUrl + testController.Properties["ProfileDetails"];
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectOutput<ProfileDetailTestModel, IErrorData> profileDetailResponse = JsonConvert.DeserializeObject<ObjectOutput<ProfileDetailTestModel, IErrorData>>(testController.ResponseString);

            profile.PartyRole = new PartyRole();
            profile.PartyRole.PartyRoleId = profileDetailResponse.obj.PartyRole.PartyRoleId;
            profile.PartyRole.PartyId = profileDetailResponse.obj.PartyRole.PartyId;
            profile.PartyRole.RoleTypeId = profileDetailResponse.obj.PartyRole.RoleTypeId;
            profile.PartyRole.Name = profileDetailResponse.obj.PartyRole.Name;

            //User
            testController.EndPointUrl = testController.HostUrl + testController.Properties["User"] + "/" + _realpageId;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectOutput<ProfileDetailTestModel, ErrorData> userResponse = JsonConvert.DeserializeObject<ObjectOutput<ProfileDetailTestModel, ErrorData>>(testController.ResponseString);

            profile.PartyId = userResponse.obj.PartyId;
            profile.RealPageId = userResponse.obj.RealPageId;
            profile.FirstName = userResponse.obj.FirstName;
            profile.MiddleName = userResponse.obj.MiddleName;
            profile.LastName = userResponse.obj.LastName;
            profile.Suffix = userResponse.obj.Suffix;
            profile.Title = userResponse.obj.Title;
            profile.PreferredContactMethodId = userResponse.obj.PreferredContactMethodId;

            return JsonConvert.SerializeObject(profile);
        }

        public string DoGetProductType()
        {
            List<ProductType> listProductType = new List<ProductType>();
            ProductType productType = new ProductType();

            dbManager = new DatabaseController(testController.Properties["dbConnection"]);
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

        public Tuple<string, string, string> createUser(string prodId = null)
        {
            /*
	        Product IDs :
            -------------
            Prospect Contact Center   - 10
	        RealPage Accounting       -  8
	        Renters Insurance         - 15
	        Spend Management          - 13 (OPS)
	        Landing                   - 3
	        Active Building           - 17
	        Asset Optimization        - 4
	        ClientPortal              - 14
	        Lead2Lease                - 6
	        OneSite                   - 1
	        Resident & Utility Management - 18
	        Social                    - 11
	        Vendor Services           - 16
	        Websites & Syndication    - 9 (Marketing Center)
	        Yieldstar                 - 7
            */

            string personaId;
            string payload = "";
            string propertyUrl = "";
            string roleUrl = "";
            string expProductrole = "";
            string expProductProperty = "";

            // Access Products --->
            personaId = JsonConvert.DeserializeObject<Persona>(DoGetPersona()).PersonaId.ToString();
            switch (prodId)
            {
                case "1": // ProductOneSite
                    propertyUrl = testController.HostUrl + testController.Properties["ProductOneSite"] + "/user/properties?userPersonaId=0&assignedOnly=false&editorPersonaId=" + personaId;
                    roleUrl = testController.HostUrl + testController.Properties["ProductOneSite"] + "/user/roles?userPersonaId=0&assignedOnly=false&editorPersonaId=" + personaId;
                    break;
                case "9": // ProductMarketingCenter
                    propertyUrl = testController.HostUrl + testController.Properties["ProductMarketingCenter"] + "/properties?userPersonaId=0&editorPersonaId=" + personaId;
                    roleUrl = testController.HostUrl + testController.Properties["ProductMarketingCenter"] + "roles?userPersonaId=0&editorPersonaId=" + personaId;
                    break;
                case "8": // ProductOnesiteAccounting
                    propertyUrl = testController.HostUrl + testController.Properties["ProductOneSiteAccounting"] + "/user/properties?userPersonaId=0&editorPersonaId=" + personaId;
                    roleUrl = testController.HostUrl + testController.Properties["ProductOneSiteAccounting"] + "/user/roles?userPersonaId=0&editorPersonaId=" + personaId;
                    break;
            }

            //var obj = Activator.CreateInstance(Type.GetType(classNamefor));

            //var prop = obj.GetType().GetProperty(propName);
            //var val = prop.GetValue(obj);


            // Execute API
            testController.EndPointUrl = propertyUrl;
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Deserialization for Response Object
            ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
            // Deserialization for Record Object


            List<ProductProperty> productProperties = new List<ProductProperty>();
            productProperties = JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(resProductProperties.Records));

            // Access Roles  --->    
            //personaId = JsonConvert.DeserializeObject<Persona>(DoGetPersona()).PersonaId.ToString();
            testController.EndPointUrl = roleUrl;

            // Execute API
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            // Deserialization for Response Object
            ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
            // Deserialization for Record Object
            List<ProductRole> productRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));
            // TODO : Deserialization for Additonal Object


            // JSON Request for EDIT --->
            ProfileDetail profileDetailRequest = new ProfileDetail();
            profileDetailRequest.userLogin = new UserLogin();
            profileDetailRequest.organization = new List<Organization>();

            profileDetailRequest.userLogin.LoginName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
            profileDetailRequest.userLogin.FromDate = DateTime.Now;
            profileDetailRequest.userLogin.IsActive = true;

            profileDetailRequest.FirstName = "AutomationFirst";
            profileDetailRequest.LastName = "AutomationLast";

            profileDetailRequest.productBatch = new List<ProductBatch>();
            profileDetailRequest.productBatch.Add(new ProductBatch());
            profileDetailRequest.productBatch[0].ProductId = Convert.ToInt32(prodId);
            profileDetailRequest.productBatch[0].StatusTypeId = 5;

            profileDetailRequest.productBatch[0].InputJson = new RolePropertyList();
            profileDetailRequest.productBatch[0].InputJson.RoleList = new List<string>();
            expProductrole = productRoles[0].ID;
            profileDetailRequest.productBatch[0].InputJson.RoleList.Add(expProductrole);
            profileDetailRequest.productBatch[0].InputJson.PropertyList = new List<string>();
            expProductProperty = productProperties[0].ID;
            profileDetailRequest.productBatch[0].InputJson.PropertyList.Add(expProductProperty);

            profileDetailRequest.Password = "Real@123";

            ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData> roleType;

            testController.EndPointUrl = testController.HostUrl + testController.Properties["RoleType"] + WebUtility.UrlEncode("User Role");
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            roleType = JsonConvert.DeserializeObject<ObjectListOutput<
                RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData>>(testController.ResponseString);


            // Set up the API URL for creating a User
            testController.EndPointUrl = testController.HostUrl + testController.Properties["User"] + "?userType=" + roleType.list[2].PartyRoleTypeId;

            // Execute API
            payload = JsonConvert.SerializeObject(profileDetailRequest);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(testController.ResponseString);

            string userPersonaId = createUserResponse.PersonaId.ToString();
            Tuple<string, string, string> postUserTuple = new Tuple<string, string, string>(userPersonaId, expProductProperty, expProductrole);

            return postUserTuple;
        }

        /// <summary>
        /// Create a User assigning a Property and a Role
        /// </summary>
        /// <param name="prodId"></param>
        /// <param name="userType"></param>
        /// <returns></returns>
        public Tuple<string, string, string, bool, bool> createUserWithPropertyAndRole(string prodId, string userType, string username, HttpVerb httpVerb, bool isPropertyGroup = false, bool disablePropertyRole = false)
        {
            /*
            Product IDs :
            -------------
            Prospect Contact Center   - 10
            RealPage Accounting       -  8
            Renters Insurance         - 15
            Spend Management          - 13 (OPS)
            Landing                   - 3
            Active Building           - 17
            Asset Optimization        - 4
            ClientPortal              - 14
            Lead2Lease                - 6
            OneSite                   - 1
            Resident & Utility Management - 18
            Social                    - 11
            Vendor Services           - 16
            Websites & Syndication    - 9 (Marketing Center)
            Yieldstar                 - 7
			Product Learning Portal   - 19
            */

            /*
             * Regular User with Email - UserTypeId - 401
             * Regular User No Email - UserTypeId - 404
             * System Administrator - UserTypeId - 402 
             */


            string personaId;
            string payload = "";
            string propertyUrl = "";
            string roleUrl = "";
            string expProductrole = "";
            string expProductProperty = "";
            List<ProductProperty> productProperties = new List<ProductProperty>();
            List<AssetGroup> productPropertiesOps = new List<AssetGroup>();

            // Access Products --->
            personaId = JsonConvert.DeserializeObject<Persona>(DoGetPersona()).PersonaId.ToString();
            bool isInsuranceExpired = true, IsVendorRecommendationChanges = true;
            ProfileDetail profileDetailRequest = new ProfileDetail();
            profileDetailRequest.productBatch = new List<ProductBatch>();
            if (userType != "402" && disablePropertyRole == false)
            {
                profileDetailRequest.productBatch.Add(new ProductBatch());
                profileDetailRequest.productBatch[0].InputJson = new RolePropertyList();

                switch (prodId)
                {
                    case "1": // ProductOneSite
                        propertyUrl = testController.HostUrl + testController.Properties["ProductOneSite"] + "/user/properties?userPersonaId=0&assignedOnly=false&editorPersonaId=" + personaId;
                        roleUrl = testController.HostUrl + testController.Properties["ProductOneSite"] + "/user/roles?userPersonaId=0&assignedOnly=false&editorPersonaId=" + personaId;
                        break;
                    case "9": // ProductMarketingCenter
                        propertyUrl = testController.HostUrl + testController.Properties["ProductMarketingCenter"] + "/properties?userPersonaId=0&editorPersonaId=" + personaId;
                        roleUrl = testController.HostUrl + testController.Properties["ProductMarketingCenter"] + "/roles?userPersonaId=0&editorPersonaId=" + personaId;
                        break;
                    case "8": // ProductOnesiteAccounting
                        propertyUrl = testController.HostUrl + testController.Properties["ProductOneSiteAccounting"] + "/user/properties?userPersonaId=0&editorPersonaId=" + personaId;
                        roleUrl = testController.HostUrl + testController.Properties["ProductOneSiteAccounting"] + "/user/roles?userPersonaId=0&editorPersonaId=" + personaId;
                        break;
                    case "13": // OPS
                        propertyUrl = $"{testController.HostUrl}{testController.Properties["ProductOps"]}/assets?userPersonaId=0&includeDisabled=false&editorPersonaId={personaId}";
                        roleUrl = $"{testController.HostUrl}{testController.Properties["ProductOps"]}/roles?userPersonaId=0&includeDisabled=false&editorPersonaId={personaId}";
                        break;
                    case "14": // ClientPortal
                        propertyUrl = $"{testController.HostUrl}{testController.Properties["ProductClientPortal"]}/properties?userPersonaId=0&editorPersonaId={personaId}";
                        roleUrl = $"{testController.HostUrl}{testController.Properties["ProductClientPortal"]}/roles?userPersonaId=0&editorPersonaId={personaId}";
                        break;
                    case "16": // VendorServices
                        if (isPropertyGroup)
                        {
                            propertyUrl = $"{testController.HostUrl}{testController.Properties["ProductVendorServices"]}/propertyGroups?userPersonaId=0&editorPersonaId={personaId}";
                        }
                        else
                        {
                            propertyUrl = $"{testController.HostUrl}{testController.Properties["ProductVendorServices"]}/properties?userPersonaId=0&editorPersonaId={personaId}";
                        }
                        roleUrl = $"{testController.HostUrl}{testController.Properties["ProductVendorServices"]}/roles?userPersonaId=0&editorPersonaId={personaId}";
                        profileDetailRequest.productBatch[0].InputJson.IsInsuranceExpired = isInsuranceExpired;
                        profileDetailRequest.productBatch[0].InputJson.IsVendorRecommendationChanges = IsVendorRecommendationChanges;
                        break;
                }
                profileDetailRequest.productBatch[0].InputJson.PropertyList = new List<string>();
                profileDetailRequest.productBatch[0].InputJson.RoleList = new List<string>();

                if (propertyUrl.Length > 0)
                {
                    // Execute API
                    testController.EndPointUrl = propertyUrl;
                    testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

                    // Deserialization for Response Object
                    ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
                    productProperties = JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(resProductProperties.Records));

                    expProductProperty = productProperties.Count > 0 ? productProperties[0].ID : "";
                    profileDetailRequest.productBatch[0].InputJson.PropertyList.Add(expProductProperty);
                }

                // Access Roles  --->  
                if (roleUrl.Length > 0)
                {
                    // Execute API
                    testController.EndPointUrl = roleUrl;
                    testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
                    // Deserialization for Response Object
                    ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
                    // Deserialization for Record Object
                    List<ProductRole> productRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));
                    // TODO : Deserialization for Additonal Object
                    expProductrole = productRoles[0].ID;
                    profileDetailRequest.productBatch[0].InputJson.RoleList.Add(expProductrole);
                }
                profileDetailRequest.productBatch[0].ProductId = Convert.ToInt32(prodId);
                profileDetailRequest.productBatch[0].StatusTypeId = 5;
            }

            // JSON Request for EDIT --->
            if (httpVerb == HttpVerb.Put)
            {
                UserLogin newUserLogin = JsonConvert.DeserializeObject<UserLogin>(DoGetUserLoginUser(username));
                profileDetailRequest.userLogin.UserId = newUserLogin.UserId;
                profileDetailRequest.userLogin.PartyId = newUserLogin.PartyId;
                profileDetailRequest.userLogin.RealPageId = newUserLogin.RealPageId;
                profileDetailRequest.PartyId = newUserLogin.PartyId;
                profileDetailRequest.RealPageId = newUserLogin.RealPageId;
                profileDetailRequest.userLogin.Is3rdPartyIDP = false;
            }
            profileDetailRequest.organization = new List<Organization>();

            profileDetailRequest.userLogin.LoginName = username;
            profileDetailRequest.userLogin.FromDate = DateTime.Now;
            profileDetailRequest.userLogin.IsActive = true;

            profileDetailRequest.FirstName = Guid.NewGuid().ToString().Remove(7);
            profileDetailRequest.LastName = Guid.NewGuid().ToString().Remove(7);

            profileDetailRequest.UserTypeId = int.Parse(userType);

            if (userType == "404")
            { profileDetailRequest.Password = "Real@123"; }

            // Set up the API URL for creating a User
            testController.EndPointUrl = testController.HostUrl + testController.Properties["User"];

            // Execute API
            payload = JsonConvert.SerializeObject(profileDetailRequest);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: httpVerb, jsonPayload: payload);

            // Extract API's JSON Response
            string userPersonaId = "0";
            if (httpVerb == HttpVerb.Post)
            {
                CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(testController.ResponseString);
                userPersonaId = createUserResponse.PersonaId.ToString();
            }
            else if (httpVerb == HttpVerb.Put)
            {
                testController._accessToken = testController.GetClientToken(testController.Properties["identityClientUrl"], username, "P@ssw0rd1");
                userPersonaId = JsonConvert.DeserializeObject<Persona>(DoGetPersona()).PersonaId.ToString();
                testController._accessToken = testController.GetClientToken(testController.Properties["identityClientUrl"], "james@test.com", "P@ssw0rd");
            }

            Thread.Sleep(90000);
            return new Tuple<string, string, string, bool, bool>(userPersonaId, expProductProperty, expProductrole, isInsuranceExpired, IsVendorRecommendationChanges);
        }
        /// <summary>
        /// Create a User without assigning a Property and a Role
        /// </summary>
        /// <param name="prodId"></param>
        /// <param name="userType"></param>
        /// <returns></returns>
	    public string createUserWithoutPropertyAndRole(string prodId, string userType)
        {
            /*
            Product IDs :
            -------------
            Prospect Contact Center   - 10
            RealPage Accounting       -  8
            Renters Insurance         - 15
            Spend Management          - 13 (OPS)
            Landing                   - 3
            Active Building           - 17
            Asset Optimization        - 4
            ClientPortal              - 14
            Lead2Lease                - 6
            OneSite                   - 1
            Resident & Utility Management - 18
            Social                    - 11
            Vendor Services           - 16
            Websites & Syndication    - 9 (Marketing Center)
            Yieldstar                 - 7
            */

            /*
             * Regular User No Email - UserTypeId - 404
             * Regular User with Email - UserTypeId - 401
             * System Administrator - UserTypeId - 402 
             */


            string personaId;
            string payload = "";
            string propertyUrl = "";
            string roleUrl = "";

            // Access Products --->
            personaId = JsonConvert.DeserializeObject<Persona>(DoGetPersona()).PersonaId.ToString();

            switch (prodId)
            {
                case "1": // ProductOneSite
                    propertyUrl = testController.HostUrl + testController.Properties["ProductOneSite"] + "/user/properties?userPersonaId=0&assignedOnly=false&editorPersonaId=" + personaId;
                    roleUrl = testController.HostUrl + testController.Properties["ProductOneSite"] + "/user/roles?userPersonaId=0&assignedOnly=false&editorPersonaId=" + personaId;
                    break;
                case "9": // ProductMarketingCenter
                    propertyUrl = testController.HostUrl + testController.Properties["ProductMarketingCenter"] + "/properties?userPersonaId=0&editorPersonaId=" + personaId;
                    roleUrl = testController.HostUrl + testController.Properties["ProductMarketingCenter"] + "roles?userPersonaId=0&editorPersonaId=" + personaId;
                    break;
                case "8": // ProductOnesiteAccounting
                    propertyUrl = testController.HostUrl + testController.Properties["ProductOneSiteAccounting"] + "/user/properties?userPersonaId=0&editorPersonaId=" + personaId;
                    roleUrl = testController.HostUrl + testController.Properties["ProductOneSiteAccounting"] + "/user/roles?userPersonaId=0&editorPersonaId=" + personaId;
                    break;
            }

            //var obj = Activator.CreateInstance(Type.GetType(classNamefor));

            //var prop = obj.GetType().GetProperty(propName);
            //var val = prop.GetValue(obj);
            /*

                        // Execute API
                        testController.EndPointUrl = propertyUrl;
                        testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

                        // Deserialization for Response Object
                        ListResponse resProductProperties = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
                        // Deserialization for Record Object


                        List<ProductProperty> productProperties = new List<ProductProperty>();
                        productProperties = JsonConvert.DeserializeObject<List<ProductProperty>>(JsonConvert.SerializeObject(resProductProperties.Records));

                        // Access Roles  --->    
                        //personaId = JsonConvert.DeserializeObject<Persona>(DoGetPersona()).PersonaId.ToString();
                        testController.EndPointUrl = roleUrl;

                        // Execute API
                        testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

                        // Deserialization for Response Object
                        ListResponse resProductRoles = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
                        // Deserialization for Record Object
                        List<ProductRole> productRoles = JsonConvert.DeserializeObject<List<ProductRole>>(JsonConvert.SerializeObject(resProductRoles.Records));
                        // TODO : Deserialization for Additonal Object

                */
            // JSON Request for EDIT --->
            ProfileDetail profileDetailRequest = new ProfileDetail();
            profileDetailRequest.userLogin = new UserLogin();
            profileDetailRequest.organization = new List<Organization>();

            profileDetailRequest.userLogin.LoginName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
            profileDetailRequest.userLogin.FromDate = DateTime.Now;
            profileDetailRequest.userLogin.IsActive = true;

            profileDetailRequest.FirstName = "AutomationFirst";
            profileDetailRequest.LastName = "AutomationLast";


            profileDetailRequest.productBatch = new List<ProductBatch>();
            profileDetailRequest.productBatch.Add(new ProductBatch());

            //profileDetailRequest.productBatch[0].ProductId = Convert.ToInt32(prodId);
            //profileDetailRequest.productBatch[0].StatusTypeId = 5;

            //profileDetailRequest.productBatch[0].InputJson = new RolePropertyList();

            //profileDetailRequest.productBatch[0].InputJson.PropertyList = new List<string>();
            //expProductProperty = productProperties[0].ID;
            //profileDetailRequest.productBatch[0].InputJson.PropertyList.Add(expProductProperty);

            //profileDetailRequest.productBatch[0].InputJson.RoleList = new List<string>();
            //expProductrole = productRoles[0].ID;
            //profileDetailRequest.productBatch[0].InputJson.RoleList.Add(expProductrole);


            profileDetailRequest.Password = "Real@123";

            ObjectListOutput<RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig.RoleType, ErrorData> roleType;

            testController.EndPointUrl = testController.HostUrl + testController.Properties["RoleType"] + WebUtility.UrlEncode("User Role");
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            roleType = JsonConvert.DeserializeObject<ObjectListOutput<RoleType, ErrorData>>(testController.ResponseString);


            // Set up the API URL for creating a User
            testController.EndPointUrl = testController.HostUrl + testController.Properties["User"];

            // Execute API
            payload = JsonConvert.SerializeObject(profileDetailRequest);
            testController.GetHttpWebResponse(endPointUrl: testController.EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response
            CreateUserResponse<ErrorData> createUserResponse = JsonConvert.DeserializeObject<CreateUserResponse<ErrorData>>(testController.ResponseString);

            string userPersonaId = createUserResponse.PersonaId.ToString();

            return userPersonaId;
        }

        //**********************************************************************

        
        public void TestPaginationResultsPerPageMigrationTool(string url, int pageSize = 50)
        {
            string _EndPointUrl = "";

            if (url.ToLower().Contains("vendorcompliance") || url.ToLower().Contains("residentportal") || url.ToLower().Contains("onsite"))
            { _EndPointUrl = url + $"&datafilter.pages.startRow=1&datafilter.pages.resultsPerPage={pageSize}"; }
            else
            { _EndPointUrl = url + $"&datafilter.pages.startRow=0&datafilter.pages.resultsPerPage={pageSize}"; }

            testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            Assert.Equal(HttpStatusCode.OK, testController.ResponseHttpStatusCode);
            var responseData = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
            var recCount = responseData.Records.Count();

            if (recCount < pageSize )
            {
                pageSize = recCount / 2;

                if (url.ToLower().Contains("vendorcompliance") || url.ToLower().Contains("residentportal") || url.ToLower().Contains("onsite"))
                { _EndPointUrl = url + $"&datafilter.pages.startRow=1&datafilter.pages.resultsPerPage={pageSize}"; }
                else
                { _EndPointUrl = url + $"&datafilter.pages.startRow=0&datafilter.pages.resultsPerPage={pageSize}"; }

                //Act
                testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
                Assert.Equal(HttpStatusCode.OK, testController.ResponseHttpStatusCode);
                responseData = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
                recCount = responseData.Records.Count();
            }
            Assert.True(recCount == pageSize);
        }


        public void TestPaginationStartRowMigrationTool(string url, int startRow)
        {
            string _EndPointUrl = "";
            int pageSize = 10;
            //int startRow = 0;

            if (url.ToLower().Contains("onesite") || url.ToLower().Contains("accounting") || url.ToLower().Contains("marketingcenter") || url.ToLower().Contains("rentersinsurance"))
            { startRow = 0; }
            else if (url.ToLower().Contains("lead2lease") || url.ToLower().Contains("vendorcompliance") || url.ToLower().Contains("residentportal") || url.ToLower().Contains("rum") || url.ToLower().Contains("onsite"))
            { startRow = 1; }
            else if (url.ToLower().Contains("ops") || url.ToLower().Contains("onsite")) // For OPS it is page number
            { startRow = 1; }

            _EndPointUrl = url + $"&datafilter.pages.startRow={startRow}&datafilter.pages.resultsPerPage={pageSize}";
            testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

            Assert.Equal(HttpStatusCode.OK, testController.ResponseHttpStatusCode);
            var responseData1 = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
            var recCount1 = responseData1.Records.Count();
            
            if (recCount1 < pageSize)
            {
                pageSize = recCount1 / 2;
                _EndPointUrl = url + $"&datafilter.pages.startRow={startRow}&datafilter.pages.resultsPerPage={pageSize}";

                //Act
                testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
                Assert.Equal(HttpStatusCode.OK, testController.ResponseHttpStatusCode);
                responseData1 = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
                recCount1 = responseData1.Records.Count();
            }

            IList<MigrationUser> response1 = responseData1.Records.Select(u => { return JsonConvert.DeserializeObject<MigrationUser>(u.ToString()); }).ToList();

            if (url.ToLower().Contains("ops") || url.ToLower().Contains("onsite"))
            {
                int e = startRow;
                startRow = pageSize;
                pageSize = e;
            }
            else
            { startRow = startRow + (pageSize - 1); }

            _EndPointUrl = url + $"&datafilter.pages.startRow={startRow}&datafilter.pages.resultsPerPage={pageSize}";
            testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
            Assert.Equal(HttpStatusCode.OK, testController.ResponseHttpStatusCode);
            var responseData2 = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
            IList<MigrationUser> response2 = responseData2.Records.Select(u => { return JsonConvert.DeserializeObject<MigrationUser>(u.ToString()); }).ToList();

            Assert.True(response1[response1.Count - 1].LastName == response2[0].LastName);
            Assert.True(response1[response1.Count - 1].FirstName == response2[0].FirstName);
            Assert.True(response1[response1.Count - 1].MiddleName == response2[0].MiddleName);
            Assert.True(response1[response1.Count - 1].Username == response2[0].Username);
            Assert.True(response1[response1.Count - 1].Email == response2[0].Email);
            Assert.True(response1[response1.Count - 1].UserId == response2[0].UserId);
        }

        public Tuple <int,int,int> TestFiltersMigrationTool(string url)
        {
            string _EndPointUrl = "";

            List<string> ls = new List<string>();
            ls.Add("Migrated");
            ls.Add("NonMigrated");
            ls.Add("All");

            int migCount = 0, nonMigCount = 0, allCount = 0;

            foreach (var filter in ls)
            {
                int startRow = 0;
                string filters = "";

                if (url.ToLower().Contains("onesite") || url.ToLower().Contains("accounting") || url.ToLower().Contains("marketingcenter") || url.ToLower().Contains("rentersinsurance"))
                { startRow = 0; }
                else if (url.ToLower().Contains("lead2lease") || url.ToLower().Contains("vendorcompliance") || url.ToLower().Contains("residentportal") || url.ToLower().Contains("rum") || url.ToLower().Contains("onsite"))
                { startRow = 1; }
                else if (url.ToLower().Contains("ops") || url.ToLower().Contains("onsite")) // For OPS it is page number
                { startRow = 1; }

                // var filters = "&datafilter.filterBy=" + WebUtility.UrlEncode("{\"filter\":") + filter + WebUtility.UrlEncode("\"}")+"&datafilter.pages.startRow=0&datafilter.pages.resultsPerPage=10000";
                if (url.ToLower().Contains("residentportal"))
                filters = "&datafilter.filterBy={\"filter\":\"" + filter + "\"}" + "&datafilter.pages.startRow="+startRow+"&datafilter.pages.resultsPerPage=1000";
                else
                filters = "&datafilter.filterBy={\"filter\":\"" + filter + "\"}" + "&datafilter.pages.startRow=" + startRow + "&datafilter.pages.resultsPerPage=10000";

                _EndPointUrl = url + filters;
                testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

                Assert.True(testController.ResponseHttpStatusCode == HttpStatusCode.OK);
                Assert.NotNull(testController.ResponseString);

                ListResponse res = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
                Assert.NotNull(res);
                Assert.True(res.IsError == false);
                Assert.True(res.ErrorReason == "");

                IList<MigrationUser> response = res.Records.Select(u => { return JsonConvert.DeserializeObject<MigrationUser>(u.ToString()); }).ToList();
                Assert.NotNull(response);
                Assert.True(response.Count() > 0);


                if (filter == "Migrated")
                    migCount = response.Count();
                else if (filter == "NonMigrated")
                    nonMigCount = response.Count();
                else if (filter == "All")
                    allCount = response.Count();
            }
            Tuple<int, int, int> userCount = new Tuple<int, int, int>(allCount, nonMigCount, migCount);
            return userCount;
        }

        public void TestMigrateUserMigrationTool(string url, string PutUrl)
        {
            int startRow = 0;
            string filters="";

            if (url.ToLower().Contains("onesite") || url.ToLower().Contains("accounting") || url.ToLower().Contains("marketingcenter") || url.ToLower().Contains("rentersinsurance"))
            { startRow = 0; }
            else if (url.ToLower().Contains("lead2lease") || url.ToLower().Contains("vendorcompliance") || url.ToLower().Contains("residentportal") || url.ToLower().Contains("rum") || url.ToLower().Contains("onsite"))
            { startRow = 1; }
            else if (url.ToLower().Contains("ops") || url.ToLower().Contains("onsite")) // For OPS it is page number
            { startRow = 1; }

            bool flag = true;
            string filter = "NonMigrated";
            string _EndPointUrl = "";

            if (url.ToLower().Contains("residentportal"))
                filters = "&datafilter.filterBy={\"filter\":\"" + filter + "\"}" + "&datafilter.pages.startRow=" + startRow + "&datafilter.pages.resultsPerPage=500";
            else
                filters = "&datafilter.filterBy={\"filter\":\"" + filter + "\"}" + "&datafilter.pages.startRow=" + startRow + "&datafilter.pages.resultsPerPage=10000";


            _EndPointUrl = url + filters;
            testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            Assert.True(testController.ResponseHttpStatusCode == HttpStatusCode.OK);
            Assert.NotNull(testController.ResponseString);

            ListResponse res = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
            Assert.NotNull(res);
            IList<MigrationUser> response = res.Records.Select(u => { return JsonConvert.DeserializeObject<MigrationUser>(u.ToString()); }).ToList();
            Assert.NotNull(response);
            Assert.True(response.Count() > 0);

            MigrationUser expMigrateUser = response[0];
            
            filter = "Migrated";
            _EndPointUrl = "";

            if (url.ToLower().Contains("residentportal"))
                filters = "&datafilter.filterBy={\"filter\":\"" + filter + "\"}" + "&datafilter.pages.startRow=" + startRow + "&datafilter.pages.resultsPerPage=1000";
            else
                filters = "&datafilter.filterBy={\"filter\":\"" + filter + "\"}" + "&datafilter.pages.startRow=" + startRow + "&datafilter.pages.resultsPerPage=10000";

            _EndPointUrl = url + filters;
            testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            Assert.True(testController.ResponseHttpStatusCode == HttpStatusCode.OK);
            Assert.NotNull(testController.ResponseString);

            res = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
            Assert.NotNull(res);
            response = res.Records.Select(u => { return JsonConvert.DeserializeObject<MigrationUser>(u.ToString()); }).ToList();
            Assert.NotNull(response);
            Assert.True(response.Count() > 0);

            foreach(var rec in response)
            {
                if (rec.UserId == expMigrateUser.UserId)
                {
                    flag = false;
                    Assert.True("Fail" == "User Exists in both Migrated and NonMigrated Filter");
                }
            }

            if (flag == true)
            {
                flag = false;

                List<MigrateUser> reqMigrateUser = new List<MigrateUser>();
                MigrateUser mu = new MigrateUser();
                
                mu.UserId = expMigrateUser.UserId;
                mu.UnifiedLoginUserName = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
                mu.UsingUnifiedLogin = true;

                reqMigrateUser.Add(mu);
                string payload = JsonConvert.SerializeObject(reqMigrateUser);

                _EndPointUrl = PutUrl;
                testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);
                Assert.True(testController.ResponseHttpStatusCode == HttpStatusCode.OK);
                Assert.NotNull(testController.ResponseString);

                MigrateResponse resMigrate = JsonConvert.DeserializeObject<MigrateResponse>(testController.ResponseString);

                //Assert.True(resMigrate.Message == "successful");

                if (url.ToLower().Contains("residentportal"))
                { Thread.Sleep(5000); }
                else
                Assert.True(resMigrate.Status == true);

                filter = "Migrated";
                _EndPointUrl = "";

                if (url.ToLower().Contains("residentportal"))
                    filters = "&datafilter.filterBy={\"filter\":\"" + filter + "\"}" + "&datafilter.pages.startRow=" + startRow + "&datafilter.pages.resultsPerPage=1000";
                else
                    filters = "&datafilter.filterBy={\"filter\":\"" + filter + "\"}" + "&datafilter.pages.startRow=" + startRow + "&datafilter.pages.resultsPerPage=10000";

                _EndPointUrl = url + filters;
                testController.GetHttpWebResponse(endPointUrl: _EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

                Assert.True(testController.ResponseHttpStatusCode == HttpStatusCode.OK);
                Assert.NotNull(testController.ResponseString);

                res = JsonConvert.DeserializeObject<ListResponse>(testController.ResponseString);
                Assert.NotNull(res);
                response = res.Records.Select(u => { return JsonConvert.DeserializeObject<MigrationUser>(u.ToString()); }).ToList();
                Assert.NotNull(response);
                Assert.True(response.Count() > 0);

                foreach (var rec in response)
                {
                    if (rec.UserId == expMigrateUser.UserId)
                    {
                        flag = true;
                        Assert.True(rec.CompanyInstanceSourceId == expMigrateUser.CompanyInstanceSourceId);
                        Assert.True(rec.FirstName == expMigrateUser.FirstName);
                        Assert.True(rec.MiddleName == expMigrateUser.MiddleName);
                        Assert.True(rec.LastName == expMigrateUser.LastName);
                        Assert.True(rec.Email == expMigrateUser.Email);

                        if (url.ToLower().Contains("rum"))
                            Assert.True(rec.Username == reqMigrateUser[0].UnifiedLoginUserName);
                        else
                            Assert.True(rec.Username == expMigrateUser.Username);

                        Assert.True(rec.Title == expMigrateUser.Title);
                        Assert.True(rec.Status == expMigrateUser.Status);
                        Assert.True(rec.Phone == expMigrateUser.Phone);
                        Assert.True(rec.LastActivity == expMigrateUser.LastActivity);
                        Assert.True(rec.Properties.Count == expMigrateUser.Properties.Count);
                        break;
                    }
                }
            }
        }






    }
}
