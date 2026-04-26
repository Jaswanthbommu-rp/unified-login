using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UnifiedLogin.SharedObjects.Product.Onsite;

public  class OnSiteProperty
{
    public int Id { get; set; }                         // "id"
    [JsonPropertyName("property_id")]
    public int PropertyId { get => Id; set => Id = value; } // "property_id" alias
    public string Name { get; set; } = string.Empty;         // "name"
    [JsonPropertyName("property_name")]
    public string? PropertyName { get => Name; set => Name = value ?? string.Empty; } // alias
    public string? State { get; set; }
    public string? City { get; set; }
    [JsonPropertyName("region_id")]
    public string? RegionId { get; set; }
    [JsonPropertyName("active")]
    public bool IsActive { get; set; }   // "active" — not "is_active"
    [JsonIgnore]
    public bool IsAssigned { get; set; }   // local flag; not serialised
    public string? InstanceId { get; set; }
}