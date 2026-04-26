using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UnifiedLogin.SharedObjects.Product.Onsite;

public class OnSiteRegion
{
    public int Id { get; set; }                          // "id"
    [JsonPropertyName("region_id")]
    public int RegionId { get => Id; set => Id = value; }  // "region_id" alias
    public string Name { get; set; } = string.Empty;          // "name"
    [JsonPropertyName("region_name")]
    public string? RegionName { get => Name; set => Name = value ?? string.Empty; } // alias
    public int CompanyId { get; set; }
    [JsonIgnore]
    public bool IsAssigned { get; set; }
}
