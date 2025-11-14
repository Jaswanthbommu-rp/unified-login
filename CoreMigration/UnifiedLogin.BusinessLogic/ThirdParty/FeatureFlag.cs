using LaunchDarkly.Sdk.Server;
using UnifiedLogin.SharedObjects.Helper;
using System;
using LaunchDarkly.Sdk.Server.Interfaces;

namespace UnifiedLogin.BusinessLogic.ThirdParty
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
                    var config = Configuration.Default(ConfigReader.GetLaunchdarklySdkKey);
                    _ldClient = new LdClient(config);
                }

                var context = LaunchDarkly.Sdk.Context.Builder("app").Build();
                flagValue = _ldClient.BoolVariation("user-company-association", context, false);
            }
            catch (Exception ex)
            {
                // bypass in unit tests
            }

            return flagValue;
        }
    }
}
