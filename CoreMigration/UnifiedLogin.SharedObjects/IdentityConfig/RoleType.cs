using Newtonsoft.Json;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Role Type
    /// </summary>
    public class RoleType : IRoleType
	{
		/// <summary>
		/// Party RoleTypeId
		/// </summary>
		[JsonProperty(PropertyName = "PartyRoleTypeId")]
		public int PartyRoleTypeId { get; set; }

		/// <summary>
		/// Parent Party RoleTypeId
		/// </summary>
		[JsonProperty(PropertyName = "ParentPartyRoleTypeId")]
		public int ParentPartyRoleTypeId { get; set; }

		/// <summary>
		/// RoleType Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

        /// <summary>
        /// User RelationShip Types
        /// </summary>
        [JsonProperty(PropertyName = "UserRelationShipTypes")]
        public List<UserRelationShipType> UserRelationShipTypes { get; set; }
    }
}
