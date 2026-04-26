using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UnifiedLogin.SharedObjects.Product.Onsite;

public class OnSiteInsertUpdate
{
    public string? UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? UserName { get; set; }
    [JsonPropertyName("email_address")]
    public string? Email { get; set; }
    public bool? IsActive { get; set; }
    [JsonPropertyName("property_access")]
    public OnSiteApiPropertyAccess? Properties { get; set; }
    public List<OnSiteRole>? Roles { get; set; }
}
