using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.AssetOptimization
{
    internal class UserProfile
    {
        public string UserId;
        public string oldUserId;
        public string email;
        public string firstName;
        public string lastName;
        public bool superuser;
        public bool internalUser;
        public bool deleted;
        public bool enabled;
        public string divisions;
        public string groupsModel;
        public string model;
    }
}
