using Newtonsoft.Json;
using RPBooksApiTestingFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using RPBooksApiTestingFramework.Controllers;
using Xunit;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace GreenBook.Tests.LandingApiTests.Access
{
    public class AccessRights : TestController
    {
        private readonly ITestOutputHelper XunitTestOutPut;
        TestUtilities reusable;

        public AccessRights(ITestOutputHelper _xUnitTestOutput)
        {
            this.XunitTestOutPut = _xUnitTestOutput;
            reusable = new TestUtilities(this);
        }

        [Fact, Trait("", "Happy Path")]
        public void GetRights()
        {
            EndPointUrl = HostUrl + Properties["Access"];
            EndPointUrl = EndPointUrl.Replace("{routeId}", Properties["routeId"]);
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get, jsonPayload: "");
            ObjectOutput<RouteSecurity, IErrorData> routeSecurityResponse = JsonConvert.DeserializeObject<ObjectOutput<RouteSecurity, IErrorData>>(ResponseString);
            
            Assert.NotNull(routeSecurityResponse.obj.RouteId);
            Assert.NotNull(routeSecurityResponse.obj.Rights);
            
            //Additional Asserts
            Assert.True(routeSecurityResponse.obj.RouteId == Properties["routeId"]);
            Assert.True(routeSecurityResponse.obj.Rights.Count > 0);
            
            foreach(var rights in routeSecurityResponse.obj.Rights)
            Assert.NotNull(rights);
        }
    }
}
