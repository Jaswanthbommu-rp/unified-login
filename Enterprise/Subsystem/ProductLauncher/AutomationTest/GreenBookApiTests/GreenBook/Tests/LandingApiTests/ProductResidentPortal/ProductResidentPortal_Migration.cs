using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration;

namespace GreenBook.Tests.LandingApiTests.ProductResidentPortal
{
    public class ProductResidentPortal_Migration : TestController
    {

        ITestOutputHelper XunitTestOutPut;
        TestUtilities reusable;
        string personaId;

        public ProductResidentPortal_Migration(ITestOutputHelper _xUnitTestOutput)
        {
            this.XunitTestOutPut = _xUnitTestOutput;
            reusable = new TestUtilities(this);

            string[] userData = Properties["ProductResidentPortalUser"].Split('|');
            _accessToken = GetClientToken(Properties["identityClientUrl"], userData[0], userData[1]);
            personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();
        }

        [Fact, Trait("Migration", "Happy Path")]
        public void GetResidentPortal_Migration()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}/?editorPersonaId={personaId}&datafilter.pages.startRow=1&datafilter.pages.resultsPerPage=500";
            EndPointUrl = EndPointUrl.Replace("{product}", "residentportal");

            XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: _accessToken, httpVerb: HttpVerb.Get);

            Assert.True(ResponseHttpStatusCode == HttpStatusCode.OK);
            Assert.NotNull(ResponseString);

            ListResponse res = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            Assert.NotNull(res);
            IList<MigrationUser> response = res.Records.Select(u => { return JsonConvert.DeserializeObject<MigrationUser>(u.ToString()); }).ToList();
            Assert.NotNull(response);
            Assert.True(response.Count() > 0);

            // Total Records
            var totalRecCount = response.Count;
            var lastNameCount = (from rec in response where (rec.LastName != null && rec.LastName != "") select rec).Count();
            var middleNameCount = (from rec in response where (rec.MiddleName != null && rec.MiddleName != "") select rec).Count();
            var firstNameCount = (from rec in response where (rec.FirstName != null && rec.FirstName != "") select rec).Count();
            var titleCount = (from rec in response where (rec.Title != null && rec.Title != "") select rec).Count();
            var phoneCount = (from rec in response where (rec.Phone != null && rec.Phone != "") select rec).Count();
            var userNameCount = (from rec in response where (rec.Username != null && rec.Username != "") select rec).Count();
            var userIdCount = (from rec in response where (rec.UserId != null && rec.UserId != "") select rec).Count();

            var propertiesCount = (from rec in response where (rec.UserId != null && rec.UserId != "") select rec).Count();
        }


        [Fact, Trait("Migration", "Happy Path")]
        public void GetResidentPortal_MigrationUsers_ResultsPerPage()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}?editorPersonaId={personaId}";
            EndPointUrl = EndPointUrl.Replace("{product}", "residentportal");
            reusable.TestPaginationResultsPerPageMigrationTool(EndPointUrl, 15);
            reusable.TestPaginationResultsPerPageMigrationTool(EndPointUrl, 90);
            //reusable.TestPaginationResultsPerPageMigrationTool(EndPointUrl, 950);
        }

        [Fact, Trait("Migration", "Happy Path")]
        public void GetResidentPortal_MigrationUsers_StartRow()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}?editorPersonaId={personaId}";
            EndPointUrl = EndPointUrl.Replace("{product}", "residentportal");
            reusable.TestPaginationStartRowMigrationTool(EndPointUrl, 0);
        }

        [Fact, Trait("Migration", "WorkFlow")]
        public void ResidentPortal_MigrationUsers_FilterCheckWithPut()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}?editorPersonaId={personaId}";
            EndPointUrl = EndPointUrl.Replace("{product}", "residentportal");

            int bmMigCount = 0, bmNonMigCount = 0, bmAllCount = 0;
            int amMigCount = 0, amNonMigCount = 0, amAllCount = 0;

            Tuple<int, int, int> bmCount = reusable.TestFiltersMigrationTool(EndPointUrl);
            bmAllCount = bmCount.Item1;
            bmNonMigCount = bmCount.Item2;
            bmMigCount = bmCount.Item3;

            Assert.True(bmAllCount > 0);
            Assert.True(bmNonMigCount > 0);
            Assert.True(bmMigCount > 0);
            //Assert.True(bmAllCount == bmMigCount + bmNonMigCount);
/*
            string PutUrl = $"{HostUrl}{Properties["Migration-PUTMigrateUsers"]}";
            PutUrl = PutUrl.Replace("{product}", "residentportal");
            reusable.TestMigrateUserMigrationTool(EndPointUrl, PutUrl);
*/
            Tuple<int, int, int> amCount = reusable.TestFiltersMigrationTool(EndPointUrl);
            amAllCount = amCount.Item1;
            amNonMigCount = amCount.Item2;
            amMigCount = amCount.Item3;

            Assert.True(amAllCount > 0);
            Assert.True(amNonMigCount > 0);
            Assert.True(amMigCount > 0);
            //Assert.True(amAllCount == amMigCount + amNonMigCount);

            //Assert.True(bmAllCount == amAllCount);
            Assert.True(bmNonMigCount == amNonMigCount + 1);
            Assert.True(bmMigCount == amMigCount - 1);
        }


    }
}

