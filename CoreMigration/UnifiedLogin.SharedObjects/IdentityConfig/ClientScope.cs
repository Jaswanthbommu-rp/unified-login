using System;
namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public class ClientScope
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public string Scope { get; set; }
    }
}
