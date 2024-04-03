using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.RealConnect
{
    #region User objects
    public class CreateRCUser
    {
        public CreateRCUser()
        {
            ReplaceLicenseAccess = true;
            Upsert = false;
            ReplaceCourseAccess = true;
            DualRole = false;
            Role = "student";
        }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "upsert")]
        public bool Upsert { get; set; }
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "clientSku")]
        public string ClientSku { get; set; }
        [JsonProperty(PropertyName = "replaceLicenseAccess")]
        public bool ReplaceLicenseAccess { get; set; }
        [JsonProperty(PropertyName = "courseIds")]
        public List<string> CourseIds { get; set; }
        [JsonProperty(PropertyName = "studentLicenseIds")]
        public List<string> StudentLicenseIds { get; set; }
        [JsonProperty(PropertyName = "managerLicenseIds")]
        public List<string> ManagerLicenseIds { get; set; }
        [JsonProperty(PropertyName = "replaceCourseAccess")]
        public bool ReplaceCourseAccess { get; set; }
        [JsonProperty(PropertyName = "externalCustomerId")]
        public string ExternalCustomerId { get; set; }
        [JsonProperty(PropertyName = "dualRole")]
        public bool DualRole { get; set; }
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }
    }

    public class RealConnectUser
    {
        public Guid Id { get; set; }
        public Guid? LearnerUserId { get; set; }
        public Guid? ManagerUserId { get; set; }
        public Guid? ClientId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public bool Disabled { get; set; }
        public string SfAccountId { get; set; }
        public string SfContactId { get; set; }
        public string RoleKey { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Telephone { get; set; }
        public string Country { get; set; }
        public string StripeCustomerId { get; set; }
        public string ExternalCustomerId { get; set; }
        public string ShippingName { get; set; }
        public string Asset { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime LastActiveAt { get; set; }
        public string Ref1 { get; set; }
        public string Ref2 { get; set; }
        public string Ref3 { get; set; }
        public string Ref4 { get; set; }
        public string Ref5 { get; set; }
        public string Ref6 { get; set; }
        public string Ref7 { get; set; }
        public string Ref8 { get; set; }
        public string Ref9 { get; set; }
        public string Ref10 { get; set; }
        public string Language { get; set; }
        public CustomFields CustomFields { get; set; }
        public List<object> PurchasedBundles { get; set; }
        public List<AllocatedLicenses> AllocatedLicenses { get; set; }
        public List<KeyValuePair<string, string>> AllocatedLearningPaths { get; set; }
        public List<PurchasedCourses> PurchasedCourses { get; set; }
        public List<KeyValuePair<string, string>> WaitlistedCourses { get; set; }
        public int? Balance { get; set; }
    }

    public class CustomFields
    {
        //Add custom fields
    }
    public class PurchasedCourses
    {
        public string CourseId { get; set; }
        public string Status { get; }
    }

    public class AllocatedLicenses
    {
        public string LicenseId { get; set; }
    }

    public class RCUserStatus
    {
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
    }
    #endregion

    #region Client Objects
    public class ClientLicenseDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public PageInfo PageInfo { get; set; }
        public List<License> Licenses { get; set; }
    }

    public class License
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? AccessDays { get; set; }
        public string ExpirationDate { get; set; }
        public int? SeatsLimit { get; set; }
        public List<string> LearningPathIds { get; set; }
        public List<string> CourseIds { get; set; }
        public List<CourseTagDetails> CourseTags { get; set; }
        public Guid? ParentLicenseId { get; set; }
        public string Ref1 { get; set; }
        public object Ref2 { get; set; }
        public object Ref3 { get; set; }
        public object Ref4 { get; set; }
        public object Ref5 { get; set; }
        public object Ref6 { get; set; }
        public object Ref7 { get; set; }
        public object Ref8 { get; set; }
        public object Ref9 { get; set; }
        public object Ref10 { get; set; }

        public bool IsAssigned { get; set; }

        public int SortId { get; set; }
    }

    public class PageInfo
    {
        public int Total { get; set; }
        public int CurrentPage { get; set; }
        public bool HasMore { get; set; }
    }

    public class CourseTagDetails
    {
        public string Id { get; set; }
        public string Label { get; set; }
    }

    #endregion

    #region Role
    public class RCRole
    {
        [JsonProperty(PropertyName = "role")]
        public string Role { get; set; }
    }

    public class RCRoleResponse
    {
        public string LearnerId { get; set; }
        public string ManagerId { get; set; }
    }
    #endregion

    #region properties
    public class CompanyLicenses
    {
        public ClientLicenseDetails ManagerLicenses { get; set; }
        public ClientLicenseDetails LearnerLicenses { get; set; }
    }

    #endregion

    #region ProductBatch
    public class RCProductBatch
    {
        public List<string> LearnerLicenseId { get; set; }
        public List<string> ManagerLicenseId { get; set; }
    }
    #endregion
}
