using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    /// <summary>
    /// Output result for newly created user
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class UserProductOutputResult
    {
        /// <summary>
        /// The user
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// User Product list
        /// </summary>
        public List<UserProducts> Products { get; set; }

        /// <summary>
        /// User Resource list
        /// </summary>
        public List<UserProducts> Resources { get; set; }
    }
}
