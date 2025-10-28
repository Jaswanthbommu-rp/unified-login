using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// Used to get a session token for the Ops system
    /// </summary>
    public class SessionRequest
    {
        /// <summary>
        /// The Ops user to sign in with
        /// </summary>
        [JsonProperty(PropertyName = "login_name")]
        public string Login_name { get; set; }
        /// <summary>
        /// The trusted key to authenticate with
        /// </summary>
        [JsonProperty(PropertyName = "trust_key")]
        public string Trust_key { get; set; }
    }
}
