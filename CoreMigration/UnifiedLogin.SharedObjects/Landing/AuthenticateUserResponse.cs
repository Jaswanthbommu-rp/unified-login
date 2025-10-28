using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class AuthenticateUserResponse : ResponseBase
    {
        /// <summary>
		/// Enterprise User Id
		/// </summary>
        public UserLoginOnly UserLogin { get; set; }

        public OrganizationStatus PrimaryOrganizationStatus { get; set; }
    }
}
