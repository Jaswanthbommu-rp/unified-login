using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// An Organization Level Settings
    /// </summary>
    public class OrganizationSetting
    {
        /// <summary>
        /// The Seeting name of the Organization
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        /// <summary>
        /// The value of the setting
        /// </summary>
        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}
