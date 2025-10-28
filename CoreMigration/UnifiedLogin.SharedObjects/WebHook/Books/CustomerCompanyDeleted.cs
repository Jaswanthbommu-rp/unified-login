using System;

namespace UnifiedLogin.SharedObjects.WebHook.Books
{
    public class CustomerCompanyDeleted
    {
        /// <summary>
        /// The id being changed
        /// </summary>
        public int CustomerCompanyId { get; set; }

        /// <summary>
        /// The date the customer company record was deleted
        /// </summary>
        public DateTime DeletedAt { get; set; }

        /// <summary>
        /// The replacement customer company id if one exists
        /// </summary>
        public int ReplacementCustomerCompanyId { get; set; }
    }
}
