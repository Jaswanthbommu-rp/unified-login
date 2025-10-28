using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class AzureUser
    {
        public string odatacontext { get; set; }
        public List<Value> value { get; set; }
    }

    public class Value
    {
        public string odataid { get; set; }
        public string userPrincipalName { get; set; }
        public string onPremisesSamAccountName { get; set; }
        public string displayName { get; set; }
        public string mail { get; set; }
        public string id { get; set; }
    }
}
