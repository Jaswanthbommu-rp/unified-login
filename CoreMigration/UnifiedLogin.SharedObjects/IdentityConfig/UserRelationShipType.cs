using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public class UserRelationShipType
    {
        public int Id { get; set; }
        public string UserRelationshipName { get; set; }
        public string Description { get; set; }
        public int PartyRoleTypeId { get; set; }
        public int ThirdPartyRelationshipId { get; set; }
        public int SortIndex { get; set; }
        public long PartyId { get; set; }
    }
}
