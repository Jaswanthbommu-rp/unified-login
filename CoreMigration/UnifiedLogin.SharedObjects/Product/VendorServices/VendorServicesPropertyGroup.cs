using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedLogin.SharedObjects.Product.VendorServices;

public class VendorServicesPropertyGroup
{
    public int? PropertyGroupId { get; set; }
    public string Name { get; set; }
    public string AccessLevel { get; set; }
    public bool IsAssigned { get; set; }
}