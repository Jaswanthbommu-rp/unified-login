namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public class ClientUserClaim
    {
        public string ClaimName { get;set; }
        public string SamlAttributeName { get; set; }
        public int? ProductId { get; set; }
    }
}
