using LaunchDarkly.Sdk.Server;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.ThirdParty
{
    public static class FeatureFlag
    {
        private static LdClient _ldClient;

        public static bool GetUserCompanyAssociationFeatureFlag()
        {
            if (_ldClient == null)
            {
                var ldRpUri = new Uri(ConfigReader.GetLaunchdarklyRelayProxyUrl);
                var cfg = LaunchDarkly.Sdk.Server.Configuration.Builder(ConfigReader.GetLaunchdarklySdkKey)
                    .ServiceEndpoints(Components.ServiceEndpoints().RelayProxy(ldRpUri))
                    .Build();

                _ldClient = new LdClient(cfg);
            }

            var flagValue = _ldClient.BoolVariation("user-company-association", LaunchDarkly.Sdk.User.WithKey("app"), false);

            return flagValue;
        }
    }
}
