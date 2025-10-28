namespace UnifiedLogin.SharedObjects.Product.Lead2Lease
{
    /// <summary>
    /// Used to store the persmissions for a user in Lead2Lease
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// The role id given to the user
        /// </summary>
        public int UserRoleId { get; set; }

        /// <summary>
        /// The property id where the given role id is assigned
        /// </summary>
        public int PropertyId { get; set; }
    }
}
