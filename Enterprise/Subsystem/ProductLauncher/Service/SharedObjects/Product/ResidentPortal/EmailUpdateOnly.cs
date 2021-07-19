using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.ResidentPortal
{
    public class EmailUpdateOnly
    {
        /// <summary>
        /// The email address of the user
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; } = null;
    }
}
