using System.Net;
using Xunit;
using Xunit.Abstractions;
using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using Microsoft.Office.Interop.Excel;
using System.Collections.Generic;
using GreenBook.Attributes;
using GreenBook.Models;

namespace GreenBook.Tests
{
    public class Persons : TestController
    {
        private string payload;
        JsonController jsonManager = new JsonController();
		TestUtilities reusable;
		private readonly ITestOutputHelper XunitTestOutPut;
        private string realPageId = "";
        string[] loginUser;


        //List<string> ls = new List<string>;

        public Persons(ITestOutputHelper _xUnitTestOutput)
		{
			reusable = new TestUtilities(this);
			this.XunitTestOutPut = _xUnitTestOutput;
            //loginUser = Properties["LoginUser"].Split('|');
            //_accessToken = GetClientToken(Properties["identityClientUrl"], loginUser[0], loginUser[1]);
        }

        // Persons=/api/Persons
        [Fact, Trait("", "Happy Path")]
        public void PostPersons()
        {
            IPerson per = new Person();
            string firstName = "";
            string lastName = "";
            string middleName = "";
            string suffix = "";
            string title = "";
            string persona = "";
   
            // Set up Payload
            payload = reusable.DoPostPutPersonsPayload();
            Person personPayload = JsonConvert.DeserializeObject<Person>(payload);
            personPayload.RealPageId = new Guid();
            personPayload.PersonaType = "persona";
            payload = JsonConvert.SerializeObject(personPayload);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["Persons"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

            // Extract API's JSON Response			
            Person.PersonOutputResult personResponse = JsonConvert.DeserializeObject<Person.PersonOutputResult>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(personResponse.RealPageId);
            Assert.True(personResponse.RealPageId.ToString().Length > 0, "personResponse.RealPageId.ToString().Length > 0");
        }

        // Persons=/api/Persons
        //[Theory]
        //[Trait("Data-Driven", "Happy Path")]
        [InlineData("PostPersons", "AutoFirstName", "AutoLastName", "AutoMiddeName", "Jr.", "Mr.")]
        [InlineData("PostPersonsWithOutFirstName", "", "AutoLastName" , "AutoMiddeName", "Jr.", "Mr.")]
        [InlineData("PostPersonsWithOutLastName", "AutoFirstName", "", "AutoMiddeName", "Jr.", "Mr.")]
        [InlineData("PostPersonsWithOutMiddleName", "AutoFirstName", "AutoLastName", "", "Jr.", "Mr.")]
        [InlineData("PostPersonsWithOutSuffix", "AutoFirstName", "", "AutoMiddeName", "", "Mr.")]
        [InlineData("PostPersonsWithOutTitle", "AutoFirstName", "", "AutoMiddeName", "Jr.", "")]
        public void PostPersonsHappyPathDD(string testCase, string firstName, string lastName, string middleName, string suffix, string title)
		{
            // Set up Payload
            payload = reusable.DoPostPutPersonsPayload(firstName, lastName, middleName, suffix, title);
            Person personPayload = JsonConvert.DeserializeObject<Person>(payload);
			personPayload.RealPageId = new Guid();
			payload = JsonConvert.SerializeObject(personPayload);
			XunitTestOutPut.WriteLine("Payload:\n" + payload);

			// Set up the API URL
			EndPointUrl = HostUrl + Properties["Persons"];

			// Execute API
			XunitTestOutPut.WriteLine("Calling " + HttpVerb.Post + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Post, jsonPayload: payload);

			// Extract API's JSON Response			
			Person.PersonOutputResult personResponse = JsonConvert.DeserializeObject<Person.PersonOutputResult>(ResponseString);
			XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

			// Assert
			Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
			Assert.NotNull(personResponse.RealPageId);
			Assert.True(personResponse.RealPageId.ToString().Length > 0, "personResponse.RealPageId.ToString().Length > 0");
		}

        //[Theory]
        //[Trait("Data-Driven", "Happy Path")]
        [InlineData("PutPersonsUpdateFirstName", "AutoFirstnamePUT", "AutoLastName", "AutoMiddeName", "Jr.", "Mr.")]
        [InlineData("PutPersonsUpdateLastName", "AutoFirstName", "AutoLastNamePUT", "AutoMiddeName", "Jr.", "Mr.")]
        [InlineData("PutPersonsUpdateMiddleName", "AutoFirstName", "AutoLastName", "AutoMiddeNamePUT", "Jr.", "Mr.")]
        [InlineData("PutPersonsUpdateSuffix", "AutoFirstName", "AutoLastName", "AutoMiddeName", "Junior", "Mr.")]
        [InlineData("PutPersonsUpdatetTitle", "AutoFirstName", "AutoLastName", "AutoMiddeName", "Jr.", "Mister")]
        [InlineData("PutPersons", "AutoFirstname", "AutoLastName", "AutoMiddeName", "Jr.", "Mr.")]
        public void PutPersonsHappyPath(string testCase, string firstName, string lastName, string middleName, string suffix, string title)
		{
            // Set up Payload            
            payload = reusable.DoPostPutPersonsPayload(firstName, lastName, middleName, suffix, title, HttpVerb.Put);
            XunitTestOutPut.WriteLine("Payload:\n" + payload);

            // Set up the API URL
            EndPointUrl = HostUrl + Properties["Persons"] + "/" + JsonConvert.DeserializeObject<Person>(payload).RealPageId;

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Put + " at " + EndPointUrl);
			GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload: payload);

            // Extract API's JSON Response            
            Person personResponse = JsonConvert.DeserializeObject<Person>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Expected JSON Response
            realPageId = JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(Properties["enterpriseUsername6"])).RealPageId.ToString();
            EndPointUrl = HostUrl + Properties["Persons"] + "/" + realPageId;
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            ObjectOutput<Person, IErrorData> getPersonResponse = JsonConvert.DeserializeObject<ObjectOutput<Person, IErrorData>>(ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(personResponse.PartyId);
            Assert.True(personResponse.PartyId == getPersonResponse.obj.PartyId, "personResponse.PartyId == getPersonResponse.obj.PartyId");
            Assert.NotNull(personResponse.RealPageId);
            Assert.True(personResponse.RealPageId == getPersonResponse.obj.RealPageId, "personResponse.RealPageId == getPersonResponse.obj.RealPageId");
            Assert.NotNull(personResponse.FirstName);
            Assert.True(personResponse.FirstName == getPersonResponse.obj.FirstName, "personResponse.FirstName == getPersonResponse.obj.FirstName");
            Assert.NotNull(personResponse.MiddleName);
            Assert.True(personResponse.MiddleName == getPersonResponse.obj.MiddleName, "personResponse.MiddleName == getPersonResponse.obj.MiddleName");
            Assert.NotNull(personResponse.LastName);
            Assert.True(personResponse.LastName == getPersonResponse.obj.LastName, "personResponse.LastName == getPersonResponse.obj.LastName");
            Assert.NotNull(personResponse.Suffix);
            Assert.True(personResponse.Suffix == getPersonResponse.obj.Suffix, "personResponse.Suffix == getPersonResponse.obj.Suffix");
            Assert.NotNull(personResponse.Title);
            Assert.True(personResponse.Title == getPersonResponse.obj.Title, "personResponse.Title == getPersonResponse.obj.Title");
            Assert.NotNull(personResponse.PreferredContactMethodId);
            Assert.True(personResponse.PreferredContactMethodId == getPersonResponse.obj.PreferredContactMethodId, "personResponse.PreferredContactMethodId == getPersonResponse.obj.PreferredContactMethodId");
        }

        //[Theory]
        //[Trait("Data-Driven", "Happy Path")]
        [InlineData("GetPersonsByValidRealPageId")]
        public void GetPersonsRealPageIdHappyPath(string testCase)
        {
            // Set up the API URL
            EndPointUrl = HostUrl + Properties["Persons"] + "/" +
				JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUser(CurrentlyLoggedInUser)).RealPageId.ToString();

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: payload);

            // Extract API's JSON Response
            ObjectOutput<Person, IErrorData> personResponse = JsonConvert.DeserializeObject<ObjectOutput<Person, IErrorData>>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.NotNull(personResponse.obj.PartyId);
            Assert.NotNull(personResponse.obj.RealPageId);
            Assert.NotNull(personResponse.obj.FirstName);
            Assert.NotNull(personResponse.obj.MiddleName);
            Assert.NotNull(personResponse.obj.LastName);
            Assert.NotNull(personResponse.obj.Suffix);
            Assert.NotNull(personResponse.obj.Title);
            Assert.NotNull(personResponse.obj.PreferredContactMethodId);
        }

        //[Theory]
        //[Trait("Data-Driven", "Negative Case"),]
        [InlineData("GetPersonsByInvalidRealPageId")]
        public void GetPersonsRealPageIdNegativeCases(string testCase)
        {
            // Set up the API URL
            EndPointUrl = HostUrl + Properties["Persons"] + "/" + "0";              

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: payload);

            //// Extract API's JSON Response
            
            Person.PersonOutputResult personResponse = JsonConvert.DeserializeObject<Person.PersonOutputResult>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
            Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
        }


        //[Theory]
        //[Trait("Data-Driven", "Happy Path")]
        [InlineData("GetPersons")]
        public void GetPersonsHappyPath(string testCase)
        {
            // Set up the API URL
            //EndPointUrl = HostUrl + Properties["Persons"] + "?realPageId=" + JsonConvert.DeserializeObject<UserLogin>(Properties["enterpriseUsername6"]).RealPageId.ToString();
            EndPointUrl = HostUrl + Properties["Persons"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: payload);

            //// Extract API's JSON Response
            
            GetPersonsModel personsResponse = JsonConvert.DeserializeObject<GetPersonsModel>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.OK == ResponseHttpStatusCode, "HttpStatusCode.OK == ResponseHttpStatusCode");
            Assert.True(personsResponse.data != null);
            Assert.True(personsResponse.status.success == true);
        }

        /*

        [Fact, Trait("", "Negative Case")]
        public void GetPersonsInvalidRealPageId()
        {
            // Set up the API URL
            reusable.DoGetUserLoginUserForGET();
            //EndPointUrl = HostUrl + Properties["Persons"] + "?realPageId=" + JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUserForGET()).RealPageId.ToString();
            EndPointUrl = HostUrl + Properties["Persons"];

            // Execute API
            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: payload);

            //// Extract API's JSON Response
            
            GetPersonsModel personResponse = JsonConvert.DeserializeObject<GetPersonsModel>(ResponseString);
            XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);

            // Assert
            Assert.True(HttpStatusCode.BadRequest == ResponseHttpStatusCode, "HttpStatusCode.BadRequest == ResponseHttpStatusCode");
            Assert.True(ResponseString.Contains("The request is invalid."), "ResponseString.Contains(\"The request is invalid.\")");
        }

    */

        //[Theory]
        //[ExcelData("GetPersons.xlsx")]
        public void ExcelXlsTests(string tcid, string payload, string expResponse, string expObject)
        {
            string actual=null;
            string expected=null;

             DateTime today = DateTime.Today; // As DateTime
             string str_today = today.ToString("MM/dd/yyyy");

             Logger.Debug("Test Case ID " + " ---------> " + tcid);

            string payload1 = payload;
            // Set up the API 
            //payload1 = payload.Replace("{", WebUtility.UrlEncode("{")).Replace("}", WebUtility.UrlEncode("}")).Replace("\"", WebUtility.UrlEncode("\"")).Replace(" ", "").Replace(":",WebUtility.UrlEncode(":"));
            reusable.DoGetUserLoginUser("kiran@local.com");
            //EndPointUrl = HostUrl + Properties["Persons"] + "?realPageId=" + JsonConvert.DeserializeObject<UserLogin>(reusable.DoGetUserLoginUserForGET()).RealPageId.ToString();
            EndPointUrl = HostUrl + Properties["Persons"] + "?" + payload1;
            
             // Execute API
             XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
             GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");

             //// Extract API's JSON Response
             
             ObjectListOutput<ProfileDetailTestModel, ErrorData> personsResponse = JsonConvert.DeserializeObject<ObjectListOutput<ProfileDetailTestModel, ErrorData>>(ResponseString);
             XunitTestOutPut.WriteLine("\n\nHTTP Status Code: " + ResponseHttpStatusCode + "\n\n" + ResponseString);
 
                         
            // Validation for Filter Parameters
            if (expResponse == "NA" && ResponseHttpStatusCode == HttpStatusCode.OK)
            {
                if (personsResponse.list.Count == 0 && expObject == "No")
                {
                    Assert.False("No data found in the returned object as expected" == "No data found in the returned object as expected");
                }
                else if (personsResponse.list.Count == 0 && expObject == "Yes")
                {
                    Assert.False("No data found to test the code. Insert data and retry" == "No data found to test the code. Insert data and retry");
                }
                else if (personsResponse.list.Count != 0 && expObject == "No")
                {
                    Assert.False("Data found to test the code which is not expected. remove the data and retry" == "Data found to test the code which is not expected. remove the data and retry");
                }
                else if (!string.IsNullOrEmpty(Payload.Trim()))
                {

                    var resPayload = Payload.Trim();
                    resPayload = resPayload.Replace("{", "").Replace("}", "");
                    string[] strg = resPayload.Split('&');
                    string[] filter = strg[0].Split('=');
                    string[] filterPair = filter[1].Split(',');

                    var filterDict = new Dictionary<string, string>();
                    foreach (var str in filterPair)
                    {
                        var s = str.Split(':');
                        filterDict.Add(s[0], s[1]);
                    }


                    // Assert Filters
                    foreach (KeyValuePair<string, string> pair in filterDict)
                    {
                        foreach (var ud in personsResponse.list)
                        {
                            switch (pair.Key)
                            {
                                case "user":
                                    actual = (ud.userLogin.IsSuperUser).ToString();
                                    if (pair.Value == "superuser")
                                        expected = "true";
                                    break;
                            }
                            Logger.Info("-------------> " + actual + " :::: " + expected);
                            Assert.NotNull(expected);
                            Assert.NotNull(actual);
                            Assert.True(expected.ToLower().Contains(actual.ToLower()), "expected.ToLower().Contains(actual.ToLower())");
                        }
                    }
                }
            }
        }


                            /*
                            //Set up the api url 
                            EndPointUrl = HostUrl + Properties["GetGetSeniorListView"];
                            if (Payload != "")
                                EndPointUrl = EndPointUrl + "?" + Payload;

                            GetHttpWebResponse(EndPointUrl, HttpVerb.Get);
                            Assert.AreEqual(HttpStatusCode.OK, ResponseHttpStatusCode);

                            JsonConvert.DeserializeObject<GetGetSeniorListViewResponse>(ResponseString);

                            // Validation for Query String Parameters
                            if (expResponse != "NA")
                            {
                                if (expResponse == "OK")
                                {
                                    Assert.IsTrue(ResponseHttpStatusCode == HttpStatusCode.OK);

                                    var responseData = JsonConvert.DeserializeObject<GetGetSeniorListViewResponse>(ResponseString);

                                    if (expObject == "Yes")
                                    {
                                        Assert.IsTrue(responseData.records != null);
                                        Assert.IsTrue(responseData.totalRecords > 0);
                                    }
                                    else
                                        Assert.IsTrue(responseData.totalRecords == 0);
                                }
                                else if (expResponse == "BadRequest")
                                    Assert.IsTrue(ResponseHttpStatusCode == HttpStatusCode.BadRequest);
                                else if (expResponse == "NotFound")
                                    Assert.IsTrue(ResponseHttpStatusCode == HttpStatusCode.NotFound);
                                else if (expResponse == "Forbidden")
                                    Assert.IsTrue(ResponseHttpStatusCode == HttpStatusCode.Forbidden);
                                else if (expResponse == "InternalServerError")
                                    Assert.IsTrue(ResponseHttpStatusCode == HttpStatusCode.InternalServerError);
                                else
                                    Assert.IsTrue("Test Data Sheet Response Issue" ==
                                                  "Please Add the Appropriate expected response and execute");
                            }

                            // Validation for Filter and Sort Parameters
                            if (expResponse == "NA" && ResponseHttpStatusCode == HttpStatusCode.OK)
                            {
                                var responseData = JsonConvert.DeserializeObject<GetGetSeniorListViewResponse>(ResponseString);

                                if (responseData.totalRecords == 0 && expObject == "No")
                                {
                                    Assert.AreEqual("No data found in the returned object as expected",
                                        "No data found in the returned object as expected");
                                }
                                else if (responseData.totalRecords == 0 && expObject == "Yes")
                                {
                                    Assert.Fail("No data found to test the code. Insert data and retry");
                                }
                                else if (responseData.totalRecords != 0 && expObject == "No")
                                {
                                    Assert.Fail("Data found to test the code which is not expected. remove the data and retry");
                                }
                                else if (!string.IsNullOrEmpty(Payload.Trim()))
                                {
                                    var resPayload = Payload.Trim();
                                    resPayload = resPayload.Replace("searchParameters.", "");
                                    var dict = new Dictionary<string, string>();
                                    string[] strg = resPayload.Split('&');
                                    foreach (var str in strg)
                                    {
                                        var s = str.Split('=');
                                        dict.Add(s[0], s[1]);
                                    }

                                    foreach (KeyValuePair<string, string> pair in dict)
                                    {
                                        foreach (var ud in responseData.records)
                                        {
                                            switch (pair.Key)
                                            {
                                                case "careFeeStatus":
                                                    actual = ud.currentAssessmentCareFeeStatus;
                                                    if (pair.Value == "All")
                                                        expected = "AcceptedPendingInterimRejected ";
                                                    else if (pair.Value == "No Care Fee Status")
                                                        expected = "";
                                                    else
                                                        expected = pair.Value;
                                                    break;

                                                case "census":
                                                    actual = ud.residentAbsent.ToString();
                                                    if (pair.Value == "All")
                                                        expected = "truefalse";
                                                    else if (pair.Value == "All Present")
                                                        expected = "false";
                                                    else
                                                        expected = "true";
                                                    break;

                                                case "currentAssessment":
                                                    actual = ud.currentAssessmentStatus;
                                                    if (pair.Value == "All")
                                                        expected = "ApprovePending Review ";
                                                    else if (pair.Value == "Assessment pending review")
                                                        expected = "Pending Review";
                                                    else if (pair.Value == "Approved Assessment")
                                                        expected = "Approve";
                                                    else    //if (pair.Value == "No Assessment")
                                                        expected = "";
                                                    break;

                                                case "nameOrUnit":
                                                    actual = ud.fullName + ud.unitNumber;
                                                    expected = pair.Value;
                                                    break;

                                                case "nextAssessment":
                                                    actual = ud.nextAssessmentDate;


                                                    expected = ud.nextAssessmentDate;
                                                    break;
                                                /* Commented Bug # 
                                                if (pair.Value == "All")
                                                    expected = ud.nextAssessmentDate;
                                                else if (pair.Value == "Due in 7 days")
                                                {
                                                    if ((Utils.UtilsDate.StringToShortDateFormat(actual)) > (Utils.UtilsDate.StringToShortDateFormat(str_today)) && (Utils.UtilsDate.StringToShortDateFormat(actual)) < (Utils.UtilsDate.StringToShortDateFormat(str_today)).AddDays(7))
                                                    {
                                                        actual = "Pass";
                                                        expected = "Pass";
                                                    }
                                                    else
                                                    expected = "Fail";
                                                }
                                                else if (pair.Value == "Due in 30 days")
                                                {
                                                    if ((Utils.UtilsDate.StringToShortDateFormat(actual)) > (Utils.UtilsDate.StringToShortDateFormat(str_today)) && (Utils.UtilsDate.StringToShortDateFormat(actual)) < (Utils.UtilsDate.StringToShortDateFormat(str_today)).AddDays(30))
                                                    {
                                                        actual = "Pass";
                                                        expected = "Pass";
                                                    }
                                                    else
                                                    expected = "Fail";
                                                }
                                                else if (pair.Value == "Past due")
                                                {
                                                    if ((Utils.UtilsDate.StringToShortDateFormat(actual)) < (Utils.UtilsDate.StringToShortDateFormat(str_today)))
                                                    {
                                                        actual = "Pass";
                                                        expected = "Pass";
                                                    }
                                                    else
                                                    expected = "Fail";
                                                }

                                                else if (pair.Value == "In Progress")
                                                {
                                                    actual = ud.nextAssessmentStatus;
                                                    expected = "In Progress";
                                                }

                                                else if (pair.Value == "Future effective date")
                                                {
                                                    if ((Utils.UtilsDate.StringToShortDateFormat(actual)) > (Utils.UtilsDate.StringToShortDateFormat(str_today)))
                                                    {
                                                        actual = "Pass";
                                                        expected = "Pass";
                                                    }
                                                    else
                                                    expected = "Fail";
                                                }
                                                else
                                                    Assert.Fail("Please check the Inputted Data");
                                                break;


                                                case "residentType":
                                                    actual = "logged issue";
                                                    expected = "logged issue";
                                                    // Commented Bug#
                                                    //actual = ud.residentType;
                                                    //expected = pair.Value;
                                                    break;

                                                case "templateName":
                                                    actual = ud.currentAssessmentTemplate;
                                                    if (pair.Value == "All")
                                                        expected = ud.currentAssessmentTemplate;
                                                    else
                                                        expected = pair.Value;
                                                    break;

                                                case "serviceGroupId":
                                                    actual = ud.serviceGroup;
                                                    if (pair.Value != "-1")
                                                    {
                                                        // source values {Templatelist, Servicegroups}
                                                        var sourceServ = "Servicegroups";
                                                        // activeStatus values {0, 1}
                                                        var activeStatusServ = 0;

                                                        var PayloadServ = "?source=" + sourceServ + "&activeStatus=" + activeStatusServ;
                                                        EndPointUrl = HostUrl + Properties["GetGetMasterDataList"] + PayloadServ;
                                                        GetHttpWebResponse(EndPointUrl, HttpVerb.Get);
                                                        Assert.AreEqual(HttpStatusCode.OK, ResponseHttpStatusCode);

                                                        var responseServ = JsonConvert.DeserializeObject<GetGetMasterDataListResponse>(ResponseString);
                                                        foreach (var res in responseServ.records)
                                                        {
                                                            if (res.masterDataID == pair.Key)
                                                            {
                                                                expected = res.masterDataValue;
                                                            }
                                                        }
                                                    }
                                                    else
                                                        expected = ud.serviceGroup;
                                                    break;
                                            }

                                            Logger.Info("-------------> " + actual + " :::: " + expected);
                                            Assert.IsNotNull(expected, "expected != null");
                                            Assert.IsNotNull(actual, "actual != null");
                                            if (pair.Value == "All" && pair.Key != "nameOrUnit")
                                                Assert.IsTrue(expected.ToLower().Contains(actual.ToLower()), "expected.ToLower().Contains(actual.ToLower())");
                                            else
                                                Assert.IsTrue(actual.ToLower().Contains(expected.ToLower()), "actual.ToLower().Contains(expected.ToLower())");
                                        }
                                    }
                                }
                            }

                */

                        



    }
}
