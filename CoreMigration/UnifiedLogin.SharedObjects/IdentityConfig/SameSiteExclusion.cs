namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public class SameSiteExclusion
    {
        public string ComparatorLeft { get; set; }
        public string SameSiteValueLeft { get; set; }
        public string LogicalOperator { get; set; }
        public string ComperatorRight { get; set; }
        public string SameSiteValueRight { get; set; }
    }
}
