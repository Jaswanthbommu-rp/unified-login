using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.SharedObjects.Product.RPDocumentManagement
{
   public class RPDMigrationUser : MigrationUser
    {
        #region Ctor
        /// <summary>
        /// Migration User
        /// </summary>
        public RPDMigrationUser()
        {
            RPDProperties = new List<string>();
        }
        #endregion 

        /// <summary>
        /// List of properties for a user
        /// </summary>
        [JsonProperty("Properties")]
        public IList<string> RPDProperties
        {
            set
            {
                base.Properties = value.Select(x => new MigrationProperty() { PropertyInstanceSourceId = x }).ToList();
            }
        }

        /// <summary>
        /// login Name
        /// </summary>
        [JsonProperty("loginName")]
        public string LoginName
        {
            set
            {
                base.Username = value;
            }
        }
        /// <summary>
        /// last Access Date
        /// </summary>
        [JsonProperty("lastAccessDate")]
        public string lastAccessDate
        {
            set
            {
                base.LastActivity = value;
            }
        }
        /// <summary>
        /// The comapny instance source id of the user
        /// </summary>
        /// 
        [JsonProperty("companyId")]
        public string companyId { get; set; }

      
    }
}
