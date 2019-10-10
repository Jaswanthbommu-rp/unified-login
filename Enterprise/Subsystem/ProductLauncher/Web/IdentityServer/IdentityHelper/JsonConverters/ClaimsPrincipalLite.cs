namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.JsonConverters
{
    public class ClaimsPrincipalLite
    {
        public string AuthenticationType { get; set; }
        public ClaimLite[] Claims { get; set; }
    }
}
