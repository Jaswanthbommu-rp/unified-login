using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.IdentityHelper.Model
{
     public partial class Client
    {
        public int ClientId { get; set; }
        public bool Enabled { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientUri { get; set; }
        public string LogoUri { get; set; }
        public bool RequireConsent { get; set; }
        public bool AllowRememberConsent { get; set; }
        public int Flow { get; set; }
        public bool AllowClientCredentialsOnly { get; set; }
        public string LogoutUri { get; set; }
        public bool LogoutSessionRequired { get; set; }
        public bool RequireSignOutPrompt { get; set; }
        public bool AllowAccessToAllScopes { get; set; }
        public int IdentityTokenLifetime { get; set; }
        public int AccessTokenLifetime { get; set; }
        public int AuthorizationCodeLifetime { get; set; }
        public int AbsoluteRefreshTokenLifetime { get; set; }
        public int SlidingRefreshTokenLifetime { get; set; }
        public int RefreshTokenUsage { get; set; }
        public bool UpdateAccessTokenOnRefresh { get; set; }
        public int RefreshTokenExpiration { get; set; }
        public int AccessTokenType { get; set; }
        public bool EnableLocalLogin { get; set; }
        public bool IncludeJwtId { get; set; }
        public bool AlwaysSendClientClaims { get; set; }
        public bool PrefixClientClaims { get; set; }
        public bool AllowAccessToAllGrantTypes { get; set; }

        public IEnumerable<ClientSecret> ClientSecrets { get; set; }
        public IEnumerable<ClientRedirectUri> ClientRedirectUris { get; set; }
        public IEnumerable<ClientScope> ClientScopes { get; set; }

    }
}
