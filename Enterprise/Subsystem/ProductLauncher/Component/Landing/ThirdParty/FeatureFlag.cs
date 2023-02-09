using LaunchDarkly.Sdk.Server;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using System;
using LaunchDarkly.Sdk.Server.Interfaces;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.ThirdParty
{
    public static class FeatureFlag
    {
        public static ILdClient LdClient
        {
            set {
                _ldClient = value;

            }
        }

        private static ILdClient _ldClient;

        public static bool GetUserCompanyAssociationFeatureFlag()
        {
            var flagValue = false;
            try
            {
                if (_ldClient == null)
                {
                    var ldRpUri = new Uri(ConfigReader.GetLaunchdarklyRelayProxyUrl);
                    var cfg = LaunchDarkly.Sdk.Server.Configuration.Builder(ConfigReader.GetLaunchdarklySdkKey)
                        .ServiceEndpoints(Components.ServiceEndpoints().RelayProxy(ldRpUri))
                        .Build();

                    _ldClient = new LdClient(cfg);
                }

                flagValue = _ldClient.BoolVariation("user-company-association", LaunchDarkly.Sdk.User.WithKey("app"), false);
            }
            catch (Exception ex)
            {
                // bypass in unit tests
            }

            return flagValue;
        }
    }
}
