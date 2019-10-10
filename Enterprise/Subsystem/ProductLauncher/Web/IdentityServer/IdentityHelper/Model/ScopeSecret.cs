using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.IdentityHelper.Model
{
    public class ScopeSecret
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public int ScopeId { get; set; }
        public virtual Scope Scope { get; set; }
    }
}