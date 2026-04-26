using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UnifiedLogin.SharedObjects.Product.Onsite;

public  class OnSiteApiPropertyAccess
{
    [JsonPropertyName("all_in_company_ids")]
    public List<int>? CompanyIdList { get; set; }
    [JsonPropertyName("all_in_region_ids")]
    public List<int>? RegionIdList { get; set; }
    [JsonPropertyName("property_ids")]
    public List<int>? PropertyIdList { get; set; }
}