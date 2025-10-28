using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserCompaniesProperties
    {
        [JsonIgnore]
        /// <summary>
        /// User PersonaId
        /// </summary>
        public long PersonaId { get; set; }

        /// <summary>
        /// Organization Name
        /// </summary>
        public string OrganizationName { get; set; }

        /// <summary>
        /// CompanyMasterId
        /// </summary>        
        public string Id { get; set; }

        /// <summary>
        /// Company InstanceId
        /// </summary>
        public Guid InstanceId { get; set; }

        [JsonIgnore]
        /// <summary>
        /// ErrorReason for failed calls
        /// </summary>
        public string ErrorReason { get; set; }

        /// <summary>
        /// Properties
        /// </summary>
        public List<Properties> Properties { get; set; }
    }
}
