using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
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
        public long Id { get; set; }
        /// <summary>
        /// Company InstanceId
        /// </summary>
        public Guid InstanceId { get; set; }
        /// <summary>
        /// Properties
        /// </summary>
        public List<Properties> Properties { get; set; }
    }
}
