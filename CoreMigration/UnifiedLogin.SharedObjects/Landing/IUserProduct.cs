using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    public interface IUserProduct
    {
        /// <summary>
        /// User access token
        /// </summary>
        string AccessToken { get; set; }
        /// <summary>
        /// Products assigned to user
        /// </summary>
        IList<ProductUI> AssignedProducts { get; set; }
        /// <summary>
        /// Company name
        /// </summary>
        string CompanyName { get; set; }
        /// <summary>
        /// Email address
        /// </summary>
        string Email { get; set; }
        /// <summary>
        /// First name
        /// </summary>
        string Firstname { get; set; }
        /// <summary>
        /// Last name
        /// </summary>
        string Lastname { get; set; }
        /// <summary>
        /// Phone number
        /// </summary>
        string Phone { get; set; }
        /// <summary>
        /// Summary count
        /// </summary>
        SummaryCounts SummaryCount { get; set; }
        /// <summary>
        /// Title
        /// </summary>
        string Title { get; set; }
		/// <summary>
		/// The user id
		/// </summary>
		Int64 UserId { get; set; }
    }
}