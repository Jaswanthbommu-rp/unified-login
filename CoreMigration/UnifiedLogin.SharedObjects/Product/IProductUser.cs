namespace UnifiedLogin.SharedObjects.Product
{
    /// <summary>
    /// Used to determine if the product user is associated to the object
    /// </summary>
    public interface IProductUser
    {
        /// <summary>
        /// The id of the product user
        /// </summary>
        int UserId { get; set; }

        /// <summary>
        /// The login name of the product user
        /// </summary>
        string UserLogin { get; set; }

        /// <summary>
        /// The name of the product user
        /// </summary>
        string UserName { get; set; }

        /// <summary>
        /// The users first name
        /// </summary>
        string FirstName { get; set; }

        /// <summary>
        /// The users last name
        /// </summary>
        string LastName { get; set; }

        /// <summary>
        /// If the user is assigned to the object
        /// </summary>
        bool IsAssigned { get; set; }
    }
}