using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Migration
{
    /// <summary>
    /// 
    /// </summary>
    public class ResidentPortalMigrationUser : MigrationUser
    {
        #region Ctor
        /// <summary>
        /// Migration User
        /// </summary>
        public ResidentPortalMigrationUser()
        {
            ResidentPortalProperties = new List<string>();
        }
        #endregion

        /// <summary>
        /// List of properties for a user
        /// </summary>
        [JsonProperty("Properties")]
        public IList<string> ResidentPortalProperties
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
        /// login Name
        /// </summary>
        [JsonProperty("userStatus")]
        public string UserStatus
        {
            set
            {
                base.Status = value;
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

    }
}
