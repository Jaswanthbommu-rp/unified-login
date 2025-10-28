namespace UnifiedLogin.SharedObjects.Enum
{
	/// <summary>
	/// Enum for batch process types
	/// </summary>
	public enum BatchProcessType
	{
		/// <summary>
		/// Create Update Product User
		/// </summary>
		CreateUpdateProductUser = 1,

		/// <summary>
		/// Profile Update
		/// </summary>
		ProfileUpdate = 2,

		/// <summary>
		/// Deactivate Product User
		/// </summary>
		DeactivateProductUser = 3,

		/// <summary>
		/// Activate Product User
		/// </summary>
		ActivateProductUser = 4,

		/// <summary>
		/// User Type changed from Regular To Admin
		/// </summary>
		UserTypeRegularToAdmin = 5,

		/// <summary>
		/// User Type changed from Admin To Regular
		/// </summary>
		UserTypeAdminToRegular = 6,

       
        /// <summary>
        /// Un-assign user from product
        /// </summary>
        UnassignUser = 7,

        /// <summary>
        /// User Type changed from External To Admin
        /// </summary>
        UserTypeExternalToAdmin = 8,

        /// <summary>
        /// User Type changed from Admin To External
        /// </summary>
        UserTypeAdminToExternal = 9,
		/// <summary>
		/// Enterprise Role Create Update Product User
		/// </summary>
		EnterpriseRoleCreateUpdateProductUser = 10,
		/// <summary>
		/// Primary Properties Update Product User
		/// </summary>
		PrimaryPropertiesUpdateProductUser = 14,

        /// <summary>
        /// Adding or updating Enterprise role to user
        /// </summary>
        BulkAddUpdateEnterpriseRole = 15,
        /// <summary>
        /// Assign or Un assign products  role to user
        /// </summary>
        AssignOrUnasignProductsForBulkUsers = 16
    }
}
