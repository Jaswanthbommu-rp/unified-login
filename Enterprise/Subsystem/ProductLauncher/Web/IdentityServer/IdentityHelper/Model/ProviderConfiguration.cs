using Microsoft.Owin.Security;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.IdentityHelper.Model
{
    public class ProviderConfiguration 
    {
        public int ProviderPortfolioId { get; set; }
        public int PortfolioIdId { get; set; }
        public int AuthenticationMode { get; set; }
        public bool ValidateIssuer { get; set; }
        public string ProviderName { get; set; }
        public string Description { get; set; }
        public string AuthenticationType { get; set; }
        public string Caption { get; set; }
        public string ProviderClientId { get; set; }
        public string AuthorityUri { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        public string RedirectUri { get; set; }
        public string TokenValidationAuthenticationType { get; set; }
        public string Scope { get; set; }
        public string OktaEntityId { get; set; }
        public string OktaMetadataLocation { get; set; }
    }
}