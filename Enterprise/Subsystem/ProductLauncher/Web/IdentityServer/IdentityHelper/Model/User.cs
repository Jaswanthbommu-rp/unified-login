namespace RP.Enterprise.Subsystem.ProductLauncher.Service.IdentityHelper.Model
{
    public class User
    {
        public long UserId { get; set; }
        public string LoginId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public bool IsActive { get; set; }
        public string PasswordHash { get; set; }
        public string IdentityProvider { get; set; }
    }
}
