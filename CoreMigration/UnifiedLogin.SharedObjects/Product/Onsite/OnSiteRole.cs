using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UnifiedLogin.SharedObjects.Product.Onsite;

public class OnSiteRole
{
    public string? Title { get; set; }    // title
    public int Level { get; set; }    // level
    public int CompanyId { get; set; }  // company_id
    [JsonIgnore]
    public bool? IsAssigned { get; set; } // local assignment flag; not serialised to/from API
}