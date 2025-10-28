using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using UnifiedLogin.SharedObjects.BlackBook;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// An Organization in the GreenBook system
	/// </summary>
	public class Organization
	{
		/// <summary>
		/// The unique id for the Organization in BlueBook
		/// </summary>
		[JsonProperty(PropertyName = "realPageId")]
		public Guid RealPageId { get; set; }

        /// <summary>
		/// The unique id for the Organization in GreenBook
		/// </summary>
		public long PartyId { get; set; }

		/// <summary>
		/// Used to store the BlueBook master id for the organization
		/// </summary>
		public long BooksMasterId { get; set; }

		/// <summary>
		/// Used to store the BlackBook Company master id for the organization RPUP id
		/// </summary>
		public long BooksCustomerMasterId { get; set; }

		/// <summary>
		/// The name of the Organization
		/// </summary>
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

		/// <summary>
		/// The date the Organization was added to GreenBook
		/// </summary>
		[JsonIgnore]
		public DateTime CreateDate { get; set; }

		/// <summary>
		/// Party Relationship
		/// </summary>
		[JsonProperty("partyRelationship", NullValueHandling = NullValueHandling.Ignore)]
		public PartyRelationship partyRelationship { get; set; }

		/// <summary>
		/// Role Value
		/// </summary>
		[JsonIgnore]
		public string RoleNameFrom { get; set; }

		/// <summary>
		/// Role Name To
		/// </summary>
		[JsonIgnore]
		public string RoleNameTo { get; set; }

		/// <summary>
		/// Relationship Type
		/// </summary>
		[JsonIgnore]
		public string RelationshipType { get; set; }

		/// <summary>
		/// Organization Type Id
		/// </summary>
		[JsonIgnore]
		public int OrganizationTypeId { get; set; }

		/// <summary>
		/// Organization Type
		/// </summary>
		[JsonProperty(PropertyName = "OrganizationType")]
		public OrganizationType organizationType { get; set;}

        /// <summary>
        /// Organization Domain Id
        /// </summary>
        [JsonIgnore]
        public int OrganizationDomainId { get; set; }

        /// <summary>
        /// Organization Domain
        /// </summary>
        [JsonProperty(PropertyName = "OrganizationDomain")]
        public OrganizationDomain OrganizationDomain { get; set;}

        /// <summary>
        /// Flag to indicate which company is used for auth purposes
        /// </summary>
        public bool PrimaryOrganization { get; set; }

		/// <summary>
		/// IsActive
		/// </summary>		
		public int IsActive { get; set; } = 1;

        /// <summary>
        /// Enable Primary properties
        /// </summary>		
        public int EnablePrimaryProperties { get; set; } = 0;

        /// <summary>
        /// Enable Enterprise Roles
        /// </summary>		
        public int EnableEnterpriseRoles { get; set; } = 0;

        /// <summary>
        /// ThirdPartyIDP
        /// </summary>		
        public String ThirdPartyIDP { get; set; }


        #region Examples
        /// <summary>
        /// Example for New UserLogin method
        /// </summary>
        /// <returns>Newly Created User Id</returns>
        public static OrganizationCreate GetOrganizationExample()
        {
            OrganizationCreate result = new OrganizationCreate()
			{
				Name = "Some company",
				EnablePrimaryProperties = 0,
                EnableEnterpriseRoles = 0,
                OrganizationTypeId = 6,
				OrganizationDomainId = 1,
				OrganizationDomain = "Primary",
				CompanyInstancePartner = "OS",
				CompanyInstancePartnerSourceId = "1051412",
				CompanyAddress = new CompanyInstanceAddress()
                {
					Address = "1234 Street",
					City = "Some city",
					State = "Some state",
					County = "Some county",
					Country = "Some country",
					PostalCode = "12345"
                },
				IsActive = 1
            };
            return result;
        }
        #endregion
    }

	/// <summary>
	/// BooksMaster object
	/// </summary>
    public class BooksMaster
    {       
        /// <summary>
        /// Used to store the BlueBook master id for the organization
        /// </summary>
        public long BlackBookId { get; set; }

        /// <summary>
        /// Used to store the BlackBook Company master id for the organization RPUP id
        /// </summary>
        public long BlueBookId { get; set; }
    }
}