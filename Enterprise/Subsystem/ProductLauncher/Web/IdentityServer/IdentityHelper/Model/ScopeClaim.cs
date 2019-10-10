namespace RP.Enterprise.Subsystem.ProductLauncher.Service.IdentityHelper.Model
{
    public class ScopeClaim
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool AlwaysIncludeInIdToken { get; set; }
        public int ScopeId { get; set; }

        public virtual Scope Scope { get; set; }
    }
}
