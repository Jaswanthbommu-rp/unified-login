using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.IdentityHelper.Model
{
    public class Token
    {
        public string TokenKey { get; set; }
        public int TokenType { get; set; }
        public string ClientCode { get; set; }
        public string SubjectCode { get; set; }
        public DateTimeOffset Expiry { get; set; }
        public string JsonCode { get; set; }
        public string AuthCodeChallenge { get; set; }
        public string AuthCodeChallengeMethod { get; set; }
        public bool? IsOpenId { get; set; }
        public string Nonce { get; set; }
        public string RedirectUri { get; set; }
        public string SessionId { get; set; }
        public bool? WasConsentShown { get; set; }
    }
}
