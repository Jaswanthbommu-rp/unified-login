using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Extensions
{
    public static class SaasuToIdentityServerMappingExtensions
    {
        #region Public Methods

        public static IdentityServer3.Core.Models.Scope ToIdentityServerModel(this Scope scope)
        {
            if (scope == null) return null;

            return new IdentityServer3.Core.Models.Scope()
            {
                AllowUnrestrictedIntrospection = scope.AllowUnrestrictedIntrospection,
                ClaimsRule = scope.ClaimsRule,
                Description = scope.Description,
                DisplayName = scope.DisplayName,
                Emphasize = scope.Emphasize,
                Enabled = scope.Enabled,
                IncludeAllClaimsForUser = scope.IncludeAllClaimsForUser,
                Name = scope.Name,
                Required = scope.Required,
                ShowInDiscoveryDocument = scope.ShowInDiscoveryDocument,
                Type = (IdentityServer3.Core.Models.ScopeType)scope.Type
            };
        }

        public static IEnumerable<IdentityServer3.Core.Models.ScopeClaim> ToIdentityServerModels(this IEnumerable<ScopeClaim> scopeClaims)
        {
            if (scopeClaims == null) return null;

            var returnList = new List<IdentityServer3.Core.Models.ScopeClaim>();
            scopeClaims.ToList().ForEach(c => returnList.Add(c.ToIdentityServerModel()));
            return returnList;
        }

        public static IEnumerable<IdentityServer3.Core.Models.Secret> ToIdentityServerModels(this IEnumerable<ScopeSecret> scopeSecrets)
        {
            if (scopeSecrets == null) return null;

            var returnList = new List<IdentityServer3.Core.Models.Secret>();
            scopeSecrets.ToList().ForEach(c => returnList.Add(c.ToIdentityServerModel()));
            return returnList;
        }

        #endregion

        #region Private Methods

        private static IdentityServer3.Core.Models.Secret ToIdentityServerModel(this ScopeSecret secret)
        {
            if (secret == null) return null;

            return new IdentityServer3.Core.Models.Secret()
            {
                Description = secret.Description,
                Expiration = secret.Expiration,
                Type = secret.Type,
                Value = secret.Value
            };
        }

        private static IdentityServer3.Core.Models.ScopeClaim ToIdentityServerModel(this ScopeClaim scopeClaim)
        {
            if (scopeClaim == null) return null;

            return new IdentityServer3.Core.Models.ScopeClaim()
            {
                AlwaysIncludeInIdToken = scopeClaim.AlwaysIncludeInIdToken,
                Description = scopeClaim.Description,
                Name = scopeClaim.Name
            };
        }

        #endregion
    }
}
