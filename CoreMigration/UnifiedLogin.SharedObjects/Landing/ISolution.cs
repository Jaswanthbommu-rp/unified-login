namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for Soultion
	/// </summary>
	public interface ISolution
	{
		/// <summary>
		/// Unique Product Type Id
		/// </summary>
		int FamilyId { get; set; }

		/// <summary>
		/// Is product linked to User?
		/// </summary>
		bool IsAssigned { get; set; }

		/// <summary>
		/// Product Unique Id
		/// </summary>
		int ProductId { get; set; }

		/// <summary>
		/// A unique id for the title for the product
		/// </summary>
		string ProductName { get; set; }

		/// <summary>
		/// Sub solution of the product
		/// </summary>
		string SubSolution { get; set; }

		/// <summary>
		/// Product Shown In User Details
		/// </summary>		
		 bool ShowInUserDetails { get; set; }

		/// <summary>
		/// Product Shown In RolesAndRights
		/// </summary>		
		bool ShowInRolesAndRights { get; set; }

		/// <summary>
		/// Is product checkbox selectable?
		/// </summary>
		bool LockOnProductAccess { get; set; }

		/// <summary>
		/// Is Notification Email required by the product?
		/// </summary>
		bool NotificationEmailRequiredForUserWithNoEmail { get; set; }

		/// <summary>
		/// If Product requires a User ex - Accounting, Onesite
		/// </summary>
		bool ProductAPIRequiresUser { get; set; }

		/// <summary>
		/// Regular User-NoEmail Not available for this product?
		/// </summary>
		bool ProductNotAvailableForRegularUserNoEmail { get; set; }
	}
}