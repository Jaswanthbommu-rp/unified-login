using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Lead2Lease
{
    /// <summary>
    /// Used to store information about a Lead2Lease user
    /// </summary>
    public class Lead2LeaseUser : ICloneable
    {
        /// <summary>
        /// Returns a clone of the existing object
        /// </summary>
        /// <returns></returns>
        public object Clone() { return this.MemberwiseClone(); }

        /// <summary>
        /// The user id
        /// </summary>
        public int UserId { get; set; }
        /// <summary>
        /// The user name
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// The user first name
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// The user last name
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// The user email address
        /// </summary>
        public string Email { get; set; }
        /// <summary>
        /// The user type
        /// </summary>
        public string UserType { get; set; }
        /// <summary>
        /// The user password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// The permissions assigned to the user
        /// </summary>
        public List<Permission> Permissions { get; set; }
        /// <summary>
        /// The properties assigned to the user
        /// </summary>
        public List<Property> Properties { get; set; }
    }

    
}
