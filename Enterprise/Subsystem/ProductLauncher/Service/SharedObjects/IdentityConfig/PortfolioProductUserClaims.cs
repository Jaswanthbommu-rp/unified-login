namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
    public class PortfolioProductUserClaims
    {
        public int Id { get; set; }
        public int PortfolioId { get; set; }
        public string ClientId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public virtual Scope Scope { get; set; }
    }
}
