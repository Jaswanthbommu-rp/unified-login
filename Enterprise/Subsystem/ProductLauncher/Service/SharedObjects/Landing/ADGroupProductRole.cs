namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class ADGroupProductRole
    {
        public int ADGroupProductRoleId { get; set; }
        public int ADGroupId { get; set; }
        public int ProductId { get; set; }
        public string RoleName { get; set; }
        public bool IsAdminRole { get; set; }
    }
}
