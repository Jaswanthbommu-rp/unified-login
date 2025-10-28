using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.OmniChannel
{
   public class PropertyRole
    {
        public long RoleID { get; set; }

        
        public long PropID { get; set; }
        
    }

    public class UserProperty
    {
        public long PropID { get; set; }
        public bool IsAssigned { get; set; }

    }

    public class Property
    {        
        public long PropID { get; set; }

    }
}
