using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.RealConnect
{
    #region User objects
    public class CreateRCUser
    {
        public CreateRCUser()
        {
            ReplaceLicenseAccess = false;
            Upsert = false;
            ReplaceCourseAccess = false;
            DualRole = false;
            Role = "student";
        }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "upsert", NullValueHandling = NullValueHandling.Ignore)]
        public bool Upsert { get; set; }
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "clientSku", NullValueHandling = NullValueHandling.Ignore)]
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
        [JsonProperty(PropertyName = "clientId")]
        public string ClientId { get; set; }
        [JsonProperty(PropertyName = "learningPathSlugs")]
        public List<string> LearningPathSlugs { get; set; }
    }

    public class RealConnectUser
    {
        public Guid Id { get; set; }
        public Guid? LearnerUserId { get; set; }
        public Guid? ManagerUserId { get; set; }
        public Guid? ClientId { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
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
        public DateTime? LastActiveAt { get; set; }
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

    public class BulkContentAssignment
    {
        [JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "learningPathIds")]
        public List<string> LearningPathIds { get; set; }
        [JsonProperty(PropertyName = "replaceCourseAccess")]
        public bool ReplaceCourseAccess { get; set; } = false;
        [JsonProperty(PropertyName = "replaceLearningPathAccess")]
        public bool ReplaceLearningPathAccess { get; set; } = true;
    }

    public class BulkContentAssignmentResponse
    {
        public int UpdatedRecordCount { get; set; }
        public List<BulkContentAssignmentError> Errors { get; set; }
    }

    public class BulkContentAssignmentError
    {
        public object Id { get; set; }
        public object Identifier { get; set; }
        public string Error { get; set; }
        public string Record { get; set; }
    }

    public class UpdateUserProfile
    {
        [JsonProperty(PropertyName = "firstName", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "lastName", NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }
        [JsonProperty(PropertyName = "clientSku", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientSku { get; set; }
        [JsonProperty(PropertyName = "clientId", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }
        public bool Upsert { get; set; }
    }

    public class BulkRemoveDualRoleManager
    {
        [JsonProperty(PropertyName = "userIds", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> UserIds { get; set; }
    }

    public class BulkRemoveDualRoleManagerResponse
    {
        [JsonProperty(PropertyName = "updatedRecordCount", NullValueHandling = NullValueHandling.Ignore)]
        public int UpdatedRecordCount { get; set; }

        [JsonProperty(PropertyName = "invalidUserIds", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> InvalidUserIds { get; set; }
    }
    #endregion

    #region Client Objects

    public class RCClientDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> LearningPathIds { get; set; }
        public List<string> CourseIds { get; set; }
        public List<CourseTag> CourseTags { get; set; }
        public DateTime? LicenseEndDate { get; set; }
        public string Sku { get; set; }
        public string Slug { get; set; }
        public int? SeatsAllocatedCount { get; set; }
        public int? SeatsLimit { get; set; }
        public int? SeatsUsedCount { get; set; }
        public object StartingBalance { get; set; }
        public object CurrentBalance { get; set; }
        public List<object> Tags { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public object NotificationEmails { get; set; }
        public bool EnableSegmentation { get; set; }
        public bool EnableDiscussions { get; set; }
        public bool EnableCommunitiesSegmentation { get; set; }
        public bool EnableBranding { get; set; }
        public bool EnableEcommerce { get; set; }
        public bool EnableOnboardingSurvey { get; set; }
        public bool EnableRecommendationAssessment { get; set; }
        public bool EnableNavLinks { get; set; }
        public bool EnableLicenseDashboards { get; set; }
        public List<object> Languages { get; set; }
        public bool EnableContentDetailPage { get; set; }
        public bool EnableCreditPurchasing { get; set; }
        public bool Disabled { get; set; }
    }

    public class CourseTag
    {
        public string Id { get; set; }
        public string Label { get; set; }
    }

    public class BulkAssignContent
    {
        public BulkAssignContent()
        {
            Users = new List<BulkContentAssignment>();
        }

        [JsonProperty(PropertyName = "users")]
        public List<BulkContentAssignment> Users { get; set; }
    }

    public class ClientLicenseDetails
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Sku { get; set; }
        public PageInfo PageInfo { get; set; }
        public List<License> Licenses { get; set; }
        public List<string> LearningPathIds { get; set; }
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
        public string Cursor { get; set; }
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

    #region LearningPath
    public class ContentItem
    {
        public string Id;
        public DateTime? CreatedAt;
        public DateTime? UpdatedAt;
        public bool? HasChildren;
        public DateTime? CourseStartDate;
        public object CourseEndDate;
        public DateTime? EnrollmentStartDate;
        public object EnrollmentEndDate;
        public string Kind;
        public string Language;
        public string ContentTypeLabel;
        public string Asset;
        public string AssetAltText;
        public string Title;
        public string Slug;
        public object Description;
        public string MetaTitle;
        public object MetaDescription;
        public object Sku;
        public CustomFields CustomFields;
        public List<string> AuthorsAndInstructors;
        public object SeatsLimit;
        public int? EnrollmentCount;
        public string Status;
        public string Source;
        public bool? FreeWithRegistration;
        public object PriceInCents;
        public object SuggestedRetailPriceInCents;
        public bool? WaitlistingEnabled;
        public bool? WaitlistingTriggered;
        public object WaitlistCount;
        public string Url;
        public List<object> Tags;
    }

    public class LearningPathsContent
    {
        public PageInfo PageInfo;
        public List<ContentItem> ContentItems;
    }
    #endregion
}
