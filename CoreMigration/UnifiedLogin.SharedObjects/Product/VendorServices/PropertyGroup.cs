namespace UnifiedLogin.SharedObjects.Product.VendorServices
{
    public class PropertyGroup
    {
        public int? Id { get; set; }
        public AccessTypeEnum Type { get; set; }
        public bool IsAssigned { get; set; }
    }

    /// <summary>
    /// Access type enum
    /// </summary>
    public enum AccessTypeEnum
    {
        Property,
        Ownergroup,
        Region,
        Division,
        Client
    }
}
