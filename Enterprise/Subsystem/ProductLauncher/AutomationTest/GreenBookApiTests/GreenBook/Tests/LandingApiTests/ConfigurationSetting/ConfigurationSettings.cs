using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RPBooksApiTestingFramework;
using RPBooksApiTestingFramework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace GreenBook.Tests.LandingApiTests.ConfigurationSettings
{
    public class ConfigurationSettings : TestController
    {
        private readonly ITestOutputHelper XunitTestOutPut;
        TestUtilities reusable;
        private string loggedinUserName;
        string realpageId;
        string personaId;
        string partyId;
        string orgPartyId;
        string payload;

        public ConfigurationSettings(ITestOutputHelper _xUnitTestOutput)
        {
            this.XunitTestOutPut = _xUnitTestOutput;
            reusable = new TestUtilities(this);

            //loggedinUserName = reusable.GetLoggedInUserName();
            //realpageId = reusable.GetRealPageId();
            //personaId = reusable.GetPersonaId();
            partyId = reusable.GetPartyId();
            orgPartyId = reusable.GetOrganizationPartyId();
            string payload;

        }

        //[Fact,Trait("","Happy Path")]
        public void GetConfigurationSettings()
        {
            EndPointUrl = HostUrl + Properties["ConfigurationSetting"]+"?PartyId="+partyId;
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            ObjectListOutput<ConfigurationSetting, IErrorData> response = JsonConvert.DeserializeObject<ObjectListOutput<ConfigurationSetting, IErrorData>>(ResponseString);

            Assert.NotNull(response);

            foreach(var res in response.list)
            {
                Assert.NotNull(res.MasterConfigurationSettingId);
                Assert.NotNull(res.SettingName);
                Assert.NotNull(res.Value);
            }

            Assert.True(response.Status.Success == true);
            Assert.True(response.Status.ErrorCode == "");
            Assert.True(response.Status.ErrorMsg == "");
        }


        //[Fact, Trait("", "Happy Path")]
        public void PutConfigurationSettings()
        {
            ConfigurationSetting con = new ConfigurationSetting();

            EndPointUrl = HostUrl + Properties["ConfigurationSetting"] + "?PartyId=" + partyId;
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);

            ObjectListOutput<ConfigurationSetting, IErrorData> response = JsonConvert.DeserializeObject<ObjectListOutput<ConfigurationSetting, IErrorData>>(ResponseString);

            Assert.NotNull(response);
            //TODO : GET the setting name and then change accordingly
            var settingName = response.list[1].SettingName;

            foreach (var res in response.list)
            {
                if (settingName == res.SettingName)
                {
                    con.MasterConfigurationSettingId = res.MasterConfigurationSettingId;
                    if (res.Value == "Light")
                        con.Value = "Dark";
                    else
                        con.Value = "Light";
                }
            }

            payload = JsonConvert.SerializeObject(con);

            // Calling the PUT API
            EndPointUrl = HostUrl + Properties["ConfigurationSetting"] + "?PartyId=" + partyId;
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Put, jsonPayload:payload);

            ObjectOutput<ConfigurationSetting, IErrorData> responsePut = JsonConvert.DeserializeObject<ObjectOutput<ConfigurationSetting, IErrorData>>(ResponseString);

            Assert.NotNull(responsePut);

            Assert.True(responsePut.obj.MasterConfigurationSettingId == con.MasterConfigurationSettingId);
            Assert.True(responsePut.obj.SettingName == con.SettingName);
            Assert.True(responsePut.obj.Value == con.Value);
            
            Assert.True(responsePut.Status.Success == true);
            Assert.True(responsePut.Status.ErrorCode == "");
            Assert.True(responsePut.Status.ErrorMsg == "");
        }



        //[Fact, Trait("", "Happy Path")]
        public void GetOrgnizationSettings()
        {
            EndPointUrl = HostUrl + Properties["ConfigurationSetting"] + "?PartyId=" + orgPartyId;
            EndPointUrl = EndPointUrl.Replace("configurationsettings","organizationsettings");
            GetHttpWebResponse(endPointUrl: EndPointUrl, authHeader: "", httpVerb: HttpVerb.Get);
            
            ObjectListOutput<ConfigurationSetting, IErrorData> response = JsonConvert.DeserializeObject<ObjectListOutput<ConfigurationSetting, IErrorData>>(ResponseString);

            Assert.NotNull(response);

            foreach (var res in response.list)
            {
                Assert.NotNull(res.MasterConfigurationSettingId);
                Assert.NotNull(res.SettingName);
                Assert.NotNull(res.Value);
            }

            Assert.True(response.Status.Success == true);
            Assert.True(response.Status.ErrorCode == "");
            Assert.True(response.Status.ErrorMsg == "");
        }
    }
}
