namespace UnifiedLogin.SharedObjects.Product.Lead2Lease
{
    /// <summary>
    /// The role from the Lead2Lease system
    /// </summary>
    public class Role
    {
        /// <summary>
        /// The unique id of the role
        /// </summary>
        public int UserRoleId { get; set; }
        
        /// <summary>
        /// The name of the role
        /// </summary>
        public string UserRoleName { get; set; }

        /// <summary>
        /// The description of the role
        /// </summary>
        public string UserRoleDescription { get; set; }
        
        /// <summary>
        /// The type id of the role for presets
        /// </summary>
        public int RoleTypeId { get; set; }

    }
}
