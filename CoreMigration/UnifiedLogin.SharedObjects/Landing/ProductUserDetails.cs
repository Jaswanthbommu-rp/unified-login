using UnifiedLogin.SharedObjects.Enum;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Product User Details
	/// </summary>
	public class ProductUserDetails : ProductUI
    {
        /// <summary>
        /// Re-adding this because this is used in Landing project's product controller and causing compile error
        /// </summary>
        //public int PortfolioProductUserId { get; set; } = 0;

        /// <summary>
        /// Re-adding this because this is used in Landing project's product controller and causing compile error
        /// </summary>
        public int UserId { get; set; } = 0; 

        /// <summary>
        /// The id of the user
        /// </summary>
        public int PersonPartyId { get; set; } = 0;

        /// <summary>
        /// The total accounts held by the user
        /// </summary>
        public int TotalAccounts { get; set; } = 0;        
        
        /// <summary>
        /// A metatag unique identifier
        /// </summary>
        public string MetatagUniqueId { get; set; } = "";
		
	}
}