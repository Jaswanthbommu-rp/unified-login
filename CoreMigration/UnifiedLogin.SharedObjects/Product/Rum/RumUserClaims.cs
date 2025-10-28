using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Rum
{
    public class RumUserClaims
    {
        public List<UserClaim> Claims { get; set; }
        public bool IsValidUser { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class UserClaim
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
