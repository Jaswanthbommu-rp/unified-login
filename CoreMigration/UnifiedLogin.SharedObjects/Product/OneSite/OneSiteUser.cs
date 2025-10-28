namespace UnifiedLogin.SharedObjects.Product.OneSite
{
    /// <summary>
    /// Used to store informaton about a OneSite User
    /// </summary>
    public class OneSiteUser
    {
        /// <summary>
        /// The OneSite user id
        /// </summary>
        public long UserId { get; set; }
        /// <summary>
        /// The OneSite PMCID and Login Name for the user
        /// </summary>
        public string SystemIdentifier { get; set; } = "";
        /// <summary>
        /// The users first name in OneSite
        /// </summary>
        public string FirstName { get; set; } = "";
        /// <summary>
        /// The users last name in OneSite
        /// </summary>
        public string LastName { get; set; } = "";
        /// <summary>
        /// The users password PIN in OneSite
        /// </summary>
        public int? UserPin { get; set; } = null;
        /// <summary>
        /// Does the user have access to all properties in OneSite
        /// </summary>
        public bool AllProperties { get; set; } = false;
		/// <summary>
		/// The user ThirdPartyReference in OneSite
		/// </summary>
		public string UserThirdPartyReference { get; set; } = "";

	}
}
