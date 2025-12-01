#nullable enable
using UnifiedLogin.SharedObjects.Product.Ops;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class OpsUserDataDto
    {
        public string? Id { get; set; }
        public string? LoginName { get; set; }
        public string? Status { get; set; }
        public OpsUserType UserType { get; set; }
        public OpsAssetGroupDto? Asset { get; set; }
    }
}
