using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Product.VendorServices;
using UnifiedLogin.SharedObjects.Product.ResidentPortal;
using UnifiedLogin.SharedObjects.Product.RealConnect;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Product Batch
	/// </summary>
	public class ProductBatch : IProductBatch
	{
		/// <summary>
		/// Unique Product Batch Id
		/// </summary>
		public int ProductBatchId { get; set; }

		/// <summary>
		/// Person PartyId
		/// </summary>
		public long PersonPartyId { get; set; }

		/// <summary>
		/// Unique Identifier - EnterpriseUserId
		/// </summary>
		public Guid RealPageId { get; set; }

		/// <summary>
		/// Created By PersomaId
		/// </summary>
		public long CreateUserPersonaId { get; set; }

		/// <summary>
		/// </summary>
		/// Assigned to PersonaId
		public long AssignUserPersonaId { get; set; }

		/// <summary>
		/// ProductId
		/// </summary>
		public int ProductId { get; set; }

        /// <summary>
        /// Product Batch Status (Waiting, Running, Error, and Success)
        /// </summary>
        public int StatusTypeId { get; set; } = 5;

        /// <summary>
        /// Retry count - used to call the API for this Product
        /// </summary>
        public byte RetryCount { get; set; } = 0;

		/// <summary>
		/// Product API (List of Properties and Roles) input JSON
		/// </summary>
		public RolePropertyList InputJson { get; set; }

		/// <summary>
		/// Product API Last run datetime 
		/// </summary>
		public DateTime LastRunDate { get; set; }

		/// <summary>
		/// Product batch create datetime
		/// </summary>
		public DateTime CreatedDate { get; set; }

		/// <summary>
		/// Product batch modified datetime
		/// </summary>
		public DateTime ModifiedDate { get; set; }

		/// <summary>
		/// Error details
		/// </summary>
		public string ErrorDetails { get; set; }

		/// <summary>
        /// Batch Group GUID
        /// </summary>
        public int BatchProcessorGroupId { get; set; }

    }

	/// <summary>
	/// Product API (List of Properties and Roles) input JSON
	/// </summary>
	public class RolePropertyList
	{
		/// <summary>
		/// List of Properties to assign to a user
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> PropertyList { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> RemovedPropertyList { get; set; }

		/// <summary>
		/// List of Roles to assign to a user
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> RoleList { get; set; }

        /// <summary>
        /// List of userGroups to assign to a user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> userGroups { get; set; }

        /// <summary>
        /// A Property Group to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PropertyGroup> PropertyGroup { get; set; }

        /// <summary>
		/// Regions to assign to the user
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RegionList { get; set; }

        /// <summary>
        /// PropertyGroup to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyGroupList { get; set; }

        /// <summary>
        /// IsInsuranceExpired Notification 
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsInsuranceExpired { get; set; } = null;

		/// <summary>
		/// IsVendorRecommendationChanges Notification 
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsVendorRecommendationChanges { get; set; } = null;

        /// <summary>
		/// IsVendorNotLinkedToAnyProperty Notification 
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsVendorNotLinkedToAnyProperty { get; set; } = null;

        /// <summary>
        /// Assign a product to a User (True = assign and false = remove access)
        /// </summary>
        public bool IsAssigned { get; set; } = true;

        /// <summary>
        /// override vendor roleid flag
        /// </summary>
        public bool IsVendorRoleIdOverride { get; set; } = true;

        /// <summary>
        /// Company Id for the product
        /// </summary>
	    public int CompanyId { get; set; }

		/// <summary>
		/// a list of Departments assigned to the user
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> DepartmentList { get; set; }

		/// <summary>
		/// Used for PAM and RPDM
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<PropertyRoleList> PropertyRoleList { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<PAMRolePropertyList> RolePropertiesList { get; set; }

		/// <summary>
		/// Used for ClickPay
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<OrganizationRole> OrganizationRoleList { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool CanReceiveMonthlyReport { get; set; } // used in DIQ

		#region Accounting - Financial Suite
		/// <summary>
		/// List of Roles to assign to a user
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> CompaniesList { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasAccessToSiteSpendManagementOnly { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAccountingAdmin { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? HasAccessToAllCurrentFutureProperties { get; set; } = null;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAssignedNewPropertyByDefault { get; set; } = null;

        #endregion

        #region Resident Portal
        /// <summary>
        /// List of Groups (Messaging groups)
        /// </summary>
        [JsonProperty(PropertyName = "MessageGroups", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> MessageGroups { get; set; }


		/// <summary>
		/// Staff user notification settings (optional)
		/// </summary>
		[JsonProperty(PropertyName = "Notifications", NullValueHandling = NullValueHandling.Ignore)]
		public Notifications Notifications { get; set; }


		#endregion
		/// <summary>
		/// Use Primary Properties to assigned to a user
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool UsePrimaryProperties { get; set; } = false;

        /// <summary>
        /// Use Primary Properties to assigned to a user
        /// </summary>
        /// roleType to adminsupport role assignment
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RoleType { get; set; } = string.Empty;

        /// <summary>
        /// List of Properties to assign to a product with propertyinstanceIds
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<ProductPrimaryProperties> ProductPrimaryProperties { get; set; }

		/// <summary>
		/// prop is for realconnect to get the manager and learner licenses product batch
		/// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RCProductBatch RCLicenseDetails { get; set; }
    }
}
