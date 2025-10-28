using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Enum;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// list of products within a Family
	/// </summary>
	public class Solution : ISolution
	{
		/// <summary>
		/// Product Unique Id
		/// </summary>
		[JsonProperty(PropertyName = "productId")]
		public int ProductId { get; set; }

		/// <summary>
		/// Product code
		/// </summary>
		[JsonProperty(PropertyName = "productCode")]
		public string ProductCode { get; set; }
		
		/// <summary>
		/// Solution Unique Id
		/// </summary>
		[JsonProperty(PropertyName = "solutionId")]
		public int SolutionId { get; set; }
		
		/// <summary>
		/// Unique Product Type Id
		/// </summary>
		[JsonIgnore]
		[JsonProperty(PropertyName = "FamilyId")]
		public int FamilyId { get; set; } = 0;

		/// <summary>
		/// A unique id for the title for the product
		/// </summary>
		[JsonProperty(PropertyName = "TitleId")]
		public string ProductName { get; set; }

		/// <summary>
		/// Is product linked to User?
		/// </summary>
		public bool IsAssigned { get; set; } = false;

		/// <summary>
		///  Learn more url
		/// </summary>
		[JsonProperty(PropertyName = "ProductStatus")]
		public int ProductStatus { get; set; } = 0;

		/// <summary>
		/// Sub solution of the product
		/// </summary>
		[JsonProperty(PropertyName = "Products")]
		public string SubSolution { get; set; } = string.Empty;

		/// <summary>
		/// Product Shown In User Details
		/// </summary>
		[JsonProperty(PropertyName = "ShowInUserDetails")]
		public bool ShowInUserDetails { get; set; } = false;

		/// <summary>
		/// Product Shown In RolesAndRights
		/// </summary>
		[JsonProperty(PropertyName = "ShowInRolesAndRights")]
		public bool ShowInRolesAndRights { get; set; } = false;

		/// <summary>
		/// Is product checkbox selectable?
		/// </summary>
		[JsonProperty(PropertyName = "LockOnProductAccess")]
		public bool LockOnProductAccess { get; set; } = true;

		/// <summary>
		/// Is Notification Email required by the product?
		/// </summary>
		[JsonProperty(PropertyName = "NotificationEmailRequiredForUserWithNoEmail")]
		public bool NotificationEmailRequiredForUserWithNoEmail { get; set; } = false;

        /// <summary>
		/// If Product requires a User ex - Accounting, Onesite
		/// </summary>
        [JsonIgnore]
        public bool ProductAPIRequiresUser { get; set; } = false;

		/// <summary>
		/// Regular User-NoEmail Not available for this product?
		/// </summary>
		[JsonProperty(PropertyName = "ProductNotAvailableForRegularUserNoEmail")]
		public bool ProductNotAvailableForRegularUserNoEmail { get; set; } = false;

		/// <summary>
		/// Product use primary properties
		/// </summary>
		[JsonProperty(PropertyName = "UsePrimaryProperties")]
		public bool UsePrimaryProperties { get; set; } = false;

		/// <summary>
		/// Persona Product used primary properties
		/// </summary>
		[JsonProperty(PropertyName = "PersonaUsedPrimaryProperties")]
		public bool PersonaUsedPrimaryProperties { get; set; } = false;

		/// <summary>
		/// Show product In RoleTemplate
		/// </summary>
		[JsonProperty(PropertyName = "ShowInRoleTemplate")]
		public bool ShowInRoleTemplate { get; set; } = false;
		/// <summary>
		/// Product Enabled in Product Access Panel For Admin User Edit
		/// </summary>
		[JsonProperty(PropertyName = "EnableProductForAdminUserEdit")]
		public bool EnableProductForAdminUserEdit { get; set; } = false;
	}
}
