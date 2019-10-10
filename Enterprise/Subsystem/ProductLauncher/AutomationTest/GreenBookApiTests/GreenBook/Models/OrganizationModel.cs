using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenBook.Models
{
    public class OrgRoleTypeFrom
    {
        public int partyRoleTypeId { get; set; }
        public int parentPartyRoleTypeId { get; set; }
        public string name { get; set; }
    }

    public class OrgRoleTypeTo
    {
        public int partyRoleTypeId { get; set; }
        public int parentPartyRoleTypeId { get; set; }
        public string name { get; set; }
    }

    public class OrgPartyRelationshipType
    {
        public int relationshipTypeId { get; set; }
        public int roleTypeIdValidFrom { get; set; }
        public int roleTypeIdValidTo { get; set; }
        public string name { get; set; }
        public string description { get; set; }
    }

    public class OrgPartyRelationship
    {
        public int partyRelationshipId { get; set; }
        public int partyIdFrom { get; set; }
        public string realPageIdFrom { get; set; }
        public int partyIdTo { get; set; }
        public string realPageIdTo { get; set; }
        public int roleTypeIdFrom { get; set; }
        public OrgRoleTypeFrom roleTypeFrom { get; set; }
        public int roleTypeIdTo { get; set; }
        public OrgRoleTypeTo roleTypeTo { get; set; }
        public int partyRelationshipTypeId { get; set; }
        public OrgPartyRelationshipType partyRelationshipType { get; set; }
        public DateTime fromDate { get; set; }
        public DateTime thruDate { get; set; }
    }

    public class LandingOrgList
    {
        public string realPageId { get; set; }
        public int partyId { get; set; }
        public int booksMasterId { get; set; }
        public string name { get; set; }
        public OrgPartyRelationship partyRelationship { get; set; }
    }

    public class LandingOrganizationModel
    {
        public List<LandingOrgList> data { get; set; }
    }
}
