namespace UnifiedLogin.SharedObjects.Product.MarketingCenter
{
    /// <summary>
    /// A Marketing Center Role
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Used to store the role id
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Used to store the role name
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Used to store of the role is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Used to store description fo the role
        /// </summary>
        public string Description { get; set; }
    }
}
