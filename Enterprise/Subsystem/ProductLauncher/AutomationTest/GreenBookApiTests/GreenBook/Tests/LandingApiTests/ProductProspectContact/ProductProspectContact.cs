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

namespace GreenBook.Tests.LandingApiTests.ProductProspectContact
{
    public class ProductProspectContact : TestController
    {
        public ProductProspectContact(ITestOutputHelper _xUnitTestOutput)
        {
            this.XunitTestOutPut = _xUnitTestOutput;

            reusable = new TestUtilities(this);

            string[] userData = Properties["ProductProspectContactUser"].Split('|');
            _accessToken = GetClientToken(Properties["identityClientUrl"], userData[0], userData[1]);
            personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();

            //EndPointUrl = HostUrl + Properties["ProductProspectContact"] + "/rights?editorPersonaId=" + personaId;
            //GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");


            newUsername = Guid.NewGuid().ToString().Remove(7) + "@ApiTest.com";
        }

        private string payload = "", personaId, userPersonaId, rightId, newUsername, expProductProperty, expProductRole;
        JsonController jsonManager = new JsonController();
        TestUtilities reusable;
        private readonly ITestOutputHelper XunitTestOutPut;
        private Tuple<string, string, string, bool, bool> dataSet;

        // ProductOneSite=/api/products/onesite

        //[Theory]
        //[Trait("Data-Driven", "ProductProspectContactCenterProperties-HappyPath")]
        //[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly")]
        //[InlineData("TC_001", "401", "Regular User with Email")]
        [InlineData("TC_002", "404", "Regular User without Email","false")]
        //[InlineData("TC_003", "402", "System Administrator")]
        public void GetProductPCCPropertiesforAdd(string testCaseId, string userTypeId, string userType,
            bool assignedOnly = true)
        {
            switch (userTypeId)
            {
                case "404":
                    dataSet = reusable.createUserWithPropertyAndRole("10", userTypeId,
                        newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
                    break;
                default:
                    dataSet = reusable.createUserWithPropertyAndRole("10", userTypeId, newUsername, HttpVerb.Post);
                    break;
            }
            userPersonaId = dataSet.Item1;
            expProductProperty = dataSet.Item2;

            // Set up the API URL
            EndPointUrl =
                $"{HostUrl}{Properties["ProductProspectContact"]}/properties?userPersonaId={userPersonaId}&assignedOnly={assignedOnly}&editorPersonaId={personaId}";

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
        //[Trait("Data-Driven", "ProductProspectContactPropertiesEdit-HappyPath")]
        //[InlineData("TestCaseID", "userTypeId", "userType", "assignedOnly", "disablePropertyRole")]
        //[InlineData("TC_001", "401", "Regular User with Email")]
        [InlineData("TC_002", "404", "Regular User without Email")]
        public void GetProductPCCPropertiesforEdit(string testCaseId, string userTypeId, string userType,
            bool assignedOnly = true, bool disablePropertyRole = false)
        {
            switch (userTypeId)
            {
                case "401":
                    dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId,
                        Properties["enterpriseUsernameForProductUpdate"], HttpVerb.Put, false, disablePropertyRole);
                    break;
                case "404":
                    dataSet = reusable.createUserWithPropertyAndRole("1", userTypeId,
                        Properties["enterpriseUsernameForProductUpdateWithoutEmail"], HttpVerb.Put, false,
                        disablePropertyRole);
                    break;
            }
            userPersonaId = dataSet.Item1;
            expProductProperty = dataSet.Item2;

            // Set up the API URL
            EndPointUrl =
                $"{HostUrl}{Properties["ProductProspectContact"]}/properties?userPersonaId={userPersonaId}&assignedOnly={assignedOnly}&editorPersonaId={personaId}";

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

       // [Theory]
        //[Trait("Data-Driven", "ProductProspectContactCenterProperties")]
        //[InlineData("TestCaseID", "editorPersonaId", "userPersonaId", "ExpResponse", "ExpResponseRecords")]
        [InlineData("TC_001", "$editorPersonaId", "0", "OK","Yes")]
        [InlineData("TC_002", "0", "$userPersonaId", "BadRequest","No")]
        [InlineData("TC_003", "0", "0", "BadRequest", "No")]
        [InlineData("TC_004", "$editorPersonaId", "$userPersonaId", "OK", "Yes")]
        [InlineData("TC_005", "Invalid", "$userPersonaId", "BadRequest", "No")]
        [InlineData("TC_006", "$editorPersonaId", "Invalid", "BadRequest", "No")]
        [InlineData("TC_007", "Invalid", "Invalid", "BadRequest", "No")]
        public void GetProductProspectContactPropertiesDataDriven(string testCaseId, string editorPersonaId,string userPersonaId, string expResponse, string expObject)
        {
            XunitTestOutPut.WriteLine("Executing TestCase : " + testCaseId);
            string userTypeId = "404";

            if (editorPersonaId == "$editorPersonaId")
            {
                editorPersonaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();
            }
            if (userPersonaId == "$userPersonaId")
            {
                dataSet = reusable.createUserWithPropertyAndRole("10", userTypeId,newUsername.Replace("@ApiTest.com", userTypeId), HttpVerb.Post);
                userPersonaId = dataSet.Item1;
            }

            EndPointUrl =
                $"{HostUrl}{Properties["ProductProspectContact"]}/properties?userPersonaId={userPersonaId}&editorPersonaId={editorPersonaId}";
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
                    List<ProductProperty> productProperties =
                        JsonConvert.DeserializeObject<List<ProductProperty>>(
                            JsonConvert.SerializeObject(resProductProperties.Records));

                    switch (expObject)
                    {
                        case "Yes":
                            Assert.True(productProperties != null);
                            Assert.True(resProductProperties.TotalRows > 0);
                            Assert.True(productProperties.Count == resProductProperties.TotalRows);

                            foreach (ProductProperty productProperty in productProperties)
                            Assert.NotNull(productProperty);
                            
                            Assert.NotNull(resProductProperties.RowsPerPage);
                            Assert.NotNull(resProductProperties.SkipRows);
                            Assert.NotNull(resProductProperties.CurrentPage);
                            Assert.NotNull(resProductProperties.TotalPages);
                            Assert.NotNull(resProductProperties.TotalRows);
                            //Assert.NotNull(resProductProperties.Additional);
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
    }
}