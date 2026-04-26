using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UnifiedLogin.SharedObjects.Product.Onsite;

public  class OnSiteApiUserProfile
{
    public string? UserId { get; set; }   // user_id via snake_case
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? UserName { get; set; }
    [JsonPropertyName("email_address")]
    public string? Email { get; set; }   // differs from snake_case "email"
    public bool? IsActive { get; set; }
    [JsonPropertyName("property_access")]
    public OnSiteApiPropertyAccess? Properties { get; set; }
    public List<OnSiteRole>? Roles { get; set; }
}
