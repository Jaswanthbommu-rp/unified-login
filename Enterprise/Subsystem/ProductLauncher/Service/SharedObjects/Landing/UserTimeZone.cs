using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class UserTimeZone //: IUserTimeZone
    {
        /// <summary>
		/// TimeZone
		/// </summary>
		[JsonProperty(PropertyName = "timeZone")]
        public string TimeZone { get; set; }
        /// <summary>
		/// TimeZoneOffset
		/// </summary>
		[JsonProperty(PropertyName = "timeZoneOffset")]
        public string TimeZoneOffset { get; set; }
    }
}
