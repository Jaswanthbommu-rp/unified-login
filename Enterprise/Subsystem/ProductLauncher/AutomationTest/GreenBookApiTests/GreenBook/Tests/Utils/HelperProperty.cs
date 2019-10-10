using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using OsNextApiTestingFramework;
using OsNextApiTestingFramework.Controllers;
using RPNextAffordable.Models;

namespace RPNextAffordable.Utils
{
    class HelperProperty : TestBase
    {
        /// <summary>
        /// ChangeProperty : To Navigate to the Property provided in the input 
        /// audited has values 1- Records the entry in the PMC Audit Table, 0 - No entry in the Audit table
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="audited"></param>
        /// <returns>"ChangePropertyMessage (messageId, messageText)"</returns>
        public ChangePropertyMessage ChangeProperty(string propertyName, int audited = 0)
        {
            string siteId="";
            if (propertyName == "All My Properties")
            {
                EndPointUrl = HostUrl + "/api/core/common/shell";
                var responseShell = GetHttpWebResponse(EndPointUrl, AuthHeader, HttpVerb.GET);
                Assert.IsNotNull(responseShell, "Web API Response");
                var responseValueShell = getHttpWebResponseValue(responseShell);
                var shellModel = JsonConvert.DeserializeObject<ShellModel>(responseValueShell);
                Assert.IsNotNull(shellModel, "ShellModel Response");
                siteId = shellModel.shellInfoResult.shellInfo.pmcid.ToString();
            }
            else
            {
                //siteId = GetPropertyId(propertyName);
                EndPointUrl = HostUrl + "/api/core/common/changeproperty";
                var responseChangeProperty = GetHttpWebResponse(EndPointUrl, AuthHeader, HttpVerb.GET);
                Assert.IsNotNull(responseChangeProperty, "Web API Response");
                var responseValueChangeProperty = getHttpWebResponseValue(responseChangeProperty);
                var changePropertyModel = JsonConvert.DeserializeObject<ChangePropertyModel>(responseValueChangeProperty);
                foreach (var changeProp in changePropertyModel.list)
                {
                    siteId = changeProp.siteID.ToString();
                }
            }

            EndPointUrl = HostUrl + "/api/core/common/changeproperty";
            EndPointUrl = EndPointUrl + "/" + siteId + "/" + audited;
            var response = GetHttpWebResponse(EndPointUrl, AuthHeader, HttpVerb.POST);
            Assert.IsNotNull(response);
            var responseValue = getHttpWebResponseValue(response);
            var objPostChangePropertyMessage = JsonConvert.DeserializeObject<ChangePropertyMessage>(responseValue);
            Assert.IsNotNull(objPostChangePropertyMessage, "objPutChangePropertyMessage");
            return objPostChangePropertyMessage;
        }
    }
}

