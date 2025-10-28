using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class NavigationMenuTree
    {
        public string Title { get; set; }

        public string PageId { get; set; }

        public string Icon { get; set; }

        public string URL { get; set; }

        public string Origin { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<NavigationMenuTree> Items { get; set; }
    }
}