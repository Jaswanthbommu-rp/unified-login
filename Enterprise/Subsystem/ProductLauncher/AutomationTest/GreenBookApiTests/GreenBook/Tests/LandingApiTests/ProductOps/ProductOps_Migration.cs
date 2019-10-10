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
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;
using System.Linq;

namespace GreenBook.Tests.LandingApiTests.ProductOps
{
    public class ProductOps_Migration : TestController
    {
        private string payload = "", personaId, userPersonaId, newUsername, expProductProperty, expProductRole;
        JsonController jsonManager = new JsonController();
        TestUtilities reusable;
        private readonly ITestOutputHelper XunitTestOutPut;
        private Tuple<string, string, string, bool, bool> dataSet;

        public ProductOps_Migration(ITestOutputHelper _xUnitTestOutput)
        {
            this.XunitTestOutPut = _xUnitTestOutput;
            reusable = new TestUtilities(this);
            
            string[] userData = Properties["ProductOpsUser"].Split('|');
            _accessToken = GetClientToken(Properties["identityClientUrl"], userData[0], userData[1]);
            personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();
        }

        [Fact, Trait("Migration", "Happy Path")]
        public void GetOps_MigrationUsers()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}/?editorPersonaId={personaId}&datafilter.pages.startRow=0&datafilter.pages.resultsPerPage=10000";
            EndPointUrl = EndPointUrl.Replace("{product}", "ops");

            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: _accessToken, httpVerb: HttpVerb.Get);

            Assert.True(ResponseHttpStatusCode == HttpStatusCode.OK);
            Assert.NotNull(ResponseString);

            ListResponse res = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            Assert.NotNull(res);
            IList<MigrationUser>  response = res.Records.Select(u => { return JsonConvert.DeserializeObject<MigrationUser>(u.ToString());}).ToList();
            Assert.NotNull(response);

            Assert.True(response.Count() > 0);

            //Cast<MigrationUser>().ToList();
      
            //foreach(var resp in response)
            //{
            //Assert.NotNull(resp.CompanyInstanceSourceId);
            //Assert.NotNull(resp.UserId);
            //Assert.NotNull(resp.FirstName);
            //Assert.NotNull(resp.MiddleName);
            //Assert.NotNull(resp.LastName);
            //Assert.NotNull(resp.Email);
            //Assert.NotNull(resp.Username);
            //Assert.NotNull(resp.Title);
            //Assert.NotNull(resp.Status);
            //Assert.NotNull(resp.Phone);
            //Assert.NotNull(resp.LastActivity);
            //Assert.True(resp.Properties.Count > 0);

            // foreach(var prop in resp.Properties)
            //{
            //Assert.True(prop.PropertyInstanceSourceId != null);
            //}
            //}

            // Total Records
            var totalRecCount = response.Count;
            var lastNameCount = (from rec in response where (rec.LastName != null && rec.LastName != "") select rec).Count();
            var middleNameCount = (from rec in response where (rec.MiddleName != null && rec.MiddleName != "") select rec).Count();
            var firstNameCount = (from rec in response where (rec.FirstName != null && rec.FirstName != "")select rec).Count();
            var titleCount = (from rec in response where (rec.Title != null && rec.Title != "") select rec).Count();
            var phoneCount = (from rec in response where (rec.Phone != null && rec.Phone != "") select rec).Count();
            var userNameCount = (from rec in response where (rec.Username != null && rec.Username != "") select rec).Count();
            var userIdCount = (from rec in response where (rec.UserId != null && rec.UserId != "") select rec).Count();

            var propertiesCount = (from rec in response where (rec.UserId != null && rec.UserId != "") select rec).Count();
        }

        [Fact, Trait("Migration", "Happy Path")]
        public void GetOps_MigrationUsers_ResultsPerPage()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}?editorPersonaId={personaId}";
            EndPointUrl = EndPointUrl.Replace("{product}", "ops");
            reusable.TestPaginationResultsPerPageMigrationTool(EndPointUrl, 15);
            reusable.TestPaginationResultsPerPageMigrationTool(EndPointUrl, 90);
            reusable.TestPaginationResultsPerPageMigrationTool(EndPointUrl, 950);
        }

        [Fact, Trait("Migration", "Happy Path")]
        public void GetOps_MigrationUsers_StartRow()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}?editorPersonaId={personaId}";
            EndPointUrl = EndPointUrl.Replace("{product}", "ops");
            reusable.TestPaginationStartRowMigrationTool(EndPointUrl, 0);
        }
    }
}
