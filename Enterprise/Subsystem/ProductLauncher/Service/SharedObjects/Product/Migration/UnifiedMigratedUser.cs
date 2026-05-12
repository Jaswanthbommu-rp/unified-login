using Newtonsoft.Json;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Migration
{
    /// <summary>
    /// Product-agnostic representation of a unity migrated user.
    /// Used as a unified response model across product APIs (AO, OneSite, etc.)
    /// </summary>
    public class UnifiedMigratedUser
    {
        [JsonProperty("productUserId")]
        public string ProductUserId { get; set; }

        [JsonProperty("productUserName")]
        public string ProductUserName { get; set; }

        [JsonProperty("productId")] 
        public int ProductId { get; set; }

        [JsonProperty("productCode")] 
        public string ProductCode { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("middleName")]
        public string MiddleName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("companyId")]
        public string CompanyId { get; set; }

        [JsonProperty("properties")]
        public IList<string> Properties { get; set; }

        [JsonProperty("propertyGroups")]
        public IList<string> PropertyGroups { get; set; }

        [JsonProperty("userGroups")]
        public IList<string> UserGroups { get; set; }

        [JsonProperty("roles")]
        public IList<string> Roles { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("isUnifiedLogin")]
        public bool IsUnifiedLogin { get; set; }

        [JsonProperty("productUserStatus")]
        public string ProductUserStatus { get; set; }

        [JsonProperty("productActivationDate")]
        public string ProductActivationDate { get; set; }

        [JsonProperty("productDeactivationDate")]
        public string ProductDeactivationDate { get; set; }

        [JsonProperty("productLastLoginDate")]
        public string ProductLastLoginDate { get; set; }

        [JsonProperty("additionalFields")]
        public IList<UnifiedMigratedUserAdditionalField> AdditionalFields { get; set; }
    }

    /// <summary>
    /// Optional product-specific fields attached to a <see cref="UnifiedMigratedUser"/>.
    /// </summary>
    public class UnifiedMigratedUserAdditionalField
    {
        [JsonProperty("referenceNumber")]
        public string ReferenceNumber { get; set; }
    }
}
