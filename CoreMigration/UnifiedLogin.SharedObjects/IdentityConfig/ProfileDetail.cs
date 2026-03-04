using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Composite object of Person, UserLogin, Contact
    /// </summary>
    public class ProfileDetail : Person, IProfileDetail
    {
        /// <summary>
        /// Avatar
        /// </summary>
        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        /// <summary>
        /// Cloned User
        /// </summary>
        [JsonProperty("clonedUser")]
        public bool ClonedUser { get; set; }

        /// <summary>
        /// Migrated User
        /// </summary>
        [JsonProperty("migratedUser")]
        public bool MigratedUser { get; set; } = false;

        /// <summary>
        /// Create User Source Type - serialized as integer for backward compatibility
        /// </summary>
        [JsonProperty("createUserSourceType")]
        public CreateUserSourceType? CreateUserSourceType { get; set; } = Enum.CreateUserSourceType.UnifiedPlatform;

        /// <summary>
        /// UserLogin attributes
        /// </summary>
        [JsonProperty("userLogin")]
        public IUserLogin userLogin { get; set; } = new UserLogin();

        /// <summary>
        /// List of organization for a user
        /// </summary>
        [JsonProperty("organization")]
        public IList<Organization> organization { get; set; } = new List<Organization>();

        /// <summary>
		/// Organization Settings
		/// </summary>
		[JsonProperty("OrganizationSetting", NullValueHandling = NullValueHandling.Ignore)]
        public List<OrganizationSetting> OrganizationSettings { get; set; }

        /// <summary>
        /// Contact Mechanism for a person attributes
        /// </summary>
        [JsonProperty("contactMechanism", NullValueHandling = NullValueHandling.Ignore)]
        public IList<CommonAddress> contactMechanism { get; set; } = new List<CommonAddress>();

        /// <summary>
        /// Summary count of properties, products and roles associated to user
        /// </summary>
        [JsonProperty("summaryCounts")]
        public SummaryCounts SummaryCount { get; set; } = new SummaryCounts();

        /// <summary>
        /// Products assigned to user
        /// </summary>
        [JsonProperty("assignedProducts", NullValueHandling = NullValueHandling.Ignore)]
        public IList<PersonaProductUserDetails> AssignedProducts { get; set; } = new List<PersonaProductUserDetails>();

        /// <summary>
        /// PartyRole (e.g. User Job Title) attributes
        /// </summary>
        [JsonProperty("partyRole", NullValueHandling = NullValueHandling.Ignore)]
        public PartyRole PartyRole { get; set; } = new PartyRole();

        /// <summary>
        /// Contact mechanisim telecommunication number attributes
        /// </summary>
        [JsonProperty("telecommunicationNumber", NullValueHandling = NullValueHandling.Ignore)]
        public IList<TelecommunicationNumber> TelecommunicationNumber { get; set; } = new List<TelecommunicationNumber>();

        /// <summary>
        /// Persona associated with the Profile
        /// </summary>
        [JsonProperty("Persona", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Persona> Persona { get; set; } = new List<Persona>();

        /// <summary>
        /// Persona disassociated with the Profile
        /// </summary>
        [JsonProperty("InactivePersona", NullValueHandling = NullValueHandling.Ignore)]
        public IList<Persona> InactivePersona { get; set; } = new List<Persona>();

        /// <summary>
        /// Initial password of the user
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// Notification Email
        /// </summary>
        [JsonProperty("notificationEmail")]
        public string NotificationEmail { get; set; }

        /// <summary>
        /// ProductBatch attributes
        /// </summary>
        [JsonProperty("productBatch", NullValueHandling = NullValueHandling.Ignore)]
        public IList<ProductBatch> productBatch { get; set; }

        /// <summary>
        /// Identity Provider
        /// </summary>
        [JsonProperty("authenticationType")]
        public string AuthenticationType { get; set; }

        /// <summary>
        /// Get or set verification activity token
        /// </summary>
        [JsonProperty("verificationActivityToken", NullValueHandling = NullValueHandling.Ignore)]
        public string VerificationActivityToken { get; set; }

        /// <summary>
        /// User Type (Regular User, SuperUser, Regular User with no email)
        /// </summary>
        [JsonProperty("userTypeId")]
        public int UserTypeId { get; set; }

        /// <summary>
        /// User RoleId
        /// </summary>
        [JsonProperty("roleId")]
        public int RoleId { get; set; }

        /// <summary>
        /// User RoleId List
        /// </summary>
        [JsonProperty("roleIdList")]
        public List<string> RoleIdList { get; set; }

        /// <summary>
        /// User Custom Fields
        /// </summary>
        [JsonProperty("CustomFields", NullValueHandling = NullValueHandling.Ignore)]
        public IList<CustomFieldValue> CustomFields { get; set; }

        /// <summary>
        /// Custom Field  
        /// </summary>
        [JsonProperty("customField")]
        public string CustomField { get; set; }

        /// <summary>
        /// Password Expiration Detail  
        /// </summary>
        [JsonProperty("passwordExpirationDetail")]
        public CheckPasswordExpirationResponse PasswordExpirationDetail { get; set; }

        /// <summary>
        /// Total number of records count (without any paging if the response is limited by paging)
        /// </summary>
        [JsonProperty("totalRecords")]
        public int TotalRecords { get; set; }

        /// <summary>
        /// EmployeeId
        /// </summary>
        [JsonProperty("employeeId")]
        public string EmployeeId { get; set; }

        /// <summary>
        /// SuperVisorUserId
        /// </summary>
        [JsonProperty("superVisorUserId")]
        public long SuperVisorUserId { get; set; }

        /// <summary>
        /// SuperVisorInfo
        /// </summary>
        [JsonProperty("superVisorUser")]
        public UserInfoLite SuperVisorUser { get; set; }

        /// <summary>
        /// UserEmployee
        /// </summary>
        [JsonProperty("userEmployee")]
        public int UserEmployeeId { get; set; }

        /// <summary>
        /// User Enterprise Role Name
        /// </summary>
        [JsonProperty("entepriseRoleName")]
        public string EntepriseRoleName { get; set; }

        /// <summary>
        /// User Enterprise Role Template Id
        /// </summary>
        [JsonProperty("roleTemplateId")]
        public int RoleTemplateId { get; set; }

        /// <summary>
        /// Persona Has Product Assignment Error
        /// </summary>
        [JsonProperty("personaHasProductError")]
        public bool PersonaHasProductError { get; set; }

        /// <summary>
        /// Is the person been created a RP employee
        /// </summary>
        [JsonProperty("isRPEmployee")]
        public bool IsRPEmployee { get; set; }

        /// <summary>
        /// RealPartner toggle to customer user management for use by RealPage employees
        /// </summary>
        [JsonProperty("isRealPartner")]
        public bool IsRealPartner { get; set; }

        /// <summary>
        /// ExternalUserRelationship
        /// </summary>
        [JsonProperty("externalUserRelationship")]
        public ExternalUserRelationship ExternalUserRelationship { get; set; }

        /// <summary>
        /// Operator
        /// </summary>
        [JsonProperty("operator")]
        public string Operator { get; set; }

        /// <summary>
        /// UserRelationshipType
        /// </summary>
        [JsonProperty("userRelationshipType")]
        public string UserRelationshipType { get; set; }

        /// <summary>
        /// CompanyName
        /// </summary>
        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        /// <summary>
        /// PhoneNumber
        /// </summary>
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// PhoneNumberType
        /// </summary>
        [JsonProperty("phoneNumberType")]
        public string PhoneNumberType { get; set; }

        /// <summary>
        /// Enterprise Roles for Delegate user
        /// </summary>
        [JsonProperty("delegateRoleTemplate")]
        public DelegateRoleTemplate DelegateRoleTemplate { get; set; }

        /// <summary>
        /// IsDelegate flag coming from UI
        /// </summary>
        [JsonProperty("isDelegateAdmin")]
        public bool IsDelegateAdmin { get; set; } = false;


        /// <summary>
        /// Is FromImport , This is to identify update is from import file or not.
        /// </summary>
        public bool IsFromImport { get; set; }

        public bool hasExistsInExternalUsersCompany { get; set; }

        public bool IsThisPrimaryOrganization { get; set; }

    }
}
