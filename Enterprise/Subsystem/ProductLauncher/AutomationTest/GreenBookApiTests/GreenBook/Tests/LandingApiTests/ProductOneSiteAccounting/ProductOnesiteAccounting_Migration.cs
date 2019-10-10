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

namespace GreenBook.Tests.LandingApiTests.ProductOneSite
{
    public class ProductOnesiteAccounting_Migration : TestController
    {

        ITestOutputHelper XunitTestOutPut;
        TestUtilities reusable;
        string personaId;

        public ProductOnesiteAccounting_Migration(ITestOutputHelper _xUnitTestOutput)
        {
            this.XunitTestOutPut = _xUnitTestOutput;
            reusable = new TestUtilities(this);

            string[] userData = Properties["ProductOnesiteAccountingUser"].Split('|');
            _accessToken = GetClientToken(Properties["identityClientUrl"], userData[0], userData[1]);
            personaId = JsonConvert.DeserializeObject<Persona>(reusable.DoGetPersona()).PersonaId.ToString();
        }

        [Fact, Trait("Migration", "Happy Path")]
        public void GetOnesiteAccounting_Migration()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}/?editorPersonaId={personaId}&datafilter.pages.startRow=0&datafilter.pages.resultsPerPage=10000";
            EndPointUrl = EndPointUrl.Replace("{product}", "onesiteaccounting");

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
        public void GetOnesiteAccounting_MigrationUsers_ResultsPerPage()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}?editorPersonaId={personaId}";
            EndPointUrl = EndPointUrl.Replace("{product}", "onesiteaccounting");
            reusable.TestPaginationResultsPerPageMigrationTool(EndPointUrl, 15);
            reusable.TestPaginationResultsPerPageMigrationTool(EndPointUrl, 90);
            reusable.TestPaginationResultsPerPageMigrationTool(EndPointUrl, 950);
        }

        [Fact, Trait("Migration", "Happy Path")]
        public void GetOnesiteAccounting_MigrationUsers_StartRow()
        {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}?editorPersonaId={personaId}";
            EndPointUrl = EndPointUrl.Replace("{product}", "onesiteaccounting");
            reusable.TestPaginationStartRowMigrationTool(EndPointUrl, 0);
        }

        //[Fact, Trait("", "Happy Path")]
        public void GetOnsesiteAccounting_MigrationUsers_Filter()
        {
            List<string> ls = new List<string>();
            ls.Add("Migrated");
            ls.Add("NonMigrated");
            ls.Add("All");

            int migCount=0, nonMigCount=0, allCount=0;

            foreach(var filter in ls)
            {
            EndPointUrl = $"{HostUrl}{Properties["Migration-GETAllUsers"]}?editorPersonaId={personaId}";
            var filters = "&datafilter.filterBy={\"filter\":\""+ filter +"\"}\"&datafilter.pages.resultsPerPage=10000";
            EndPointUrl = EndPointUrl + filters;
            //https://my2qa.corp.realpage.com/api/products/rentersinsurance/migration-users?editorPersonaId=18593&datafilter.filterBy=%7B%22filter%22%3A%22nonmigrated%22%7D&datafilter.pages.resultsPerPage=1000
                XunitTestOutPut.WriteLine("Calling " + HttpVerb.Get + " at " + EndPointUrl);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: _accessToken, httpVerb: HttpVerb.Get);

            Assert.True(ResponseHttpStatusCode == HttpStatusCode.OK);
            Assert.NotNull(ResponseString);

            ListResponse res = JsonConvert.DeserializeObject<ListResponse>(ResponseString);
            Assert.NotNull(res);
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

            Assert.True(allCount == (migCount + nonMigCount));
            Assert.True(allCount != migCount);
            Assert.True(migCount != nonMigCount);
            Assert.True(nonMigCount != allCount);
        }


    }
}

