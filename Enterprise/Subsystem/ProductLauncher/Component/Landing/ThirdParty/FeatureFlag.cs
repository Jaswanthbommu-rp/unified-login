using LaunchDarkly.Sdk.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.ThirdParty
{
    public static class FeatureFlag
    {
        public static bool GetUserCompanyAssociationFeatureFlag()
        {
            bool flagValue = true;

            var configReader = new AppSettingsReader();
            var ldRpUri = new Uri(configReader.GetValue("launchdarkly:RelayProxyUrl", typeof(string)).ToString());
            var cfg = LaunchDarkly.Sdk.Server.Configuration.Builder(configReader.GetValue("launchdarkly:SdkKey", typeof(string)).ToString())
                .ServiceEndpoints(Components.ServiceEndpoints().RelayProxy(ldRpUri))
                .Build();

            var ldClient = new LdClient(cfg);
            flagValue = ldClient.BoolVariation("user-company-association", LaunchDarkly.Sdk.User.WithKey("app"), false);


            return flagValue;
        }
    }
}
