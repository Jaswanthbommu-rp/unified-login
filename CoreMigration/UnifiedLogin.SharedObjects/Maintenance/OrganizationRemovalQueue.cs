using System;

namespace UnifiedLogin.SharedObjects.Maintenance
{
    public class OrganizationRemovalQueue
    {
        public int OrganizationRemovalQueueId { get; set; }

        public long OrganizationPartyId { get; set; }

        public Guid OrganizationRealPageId { get; set; }

        public int OrganizationRemovalQueueStatusId { get; set; } = 0;

        public bool OrganizationRemoveUDMData { get; set; } = false;

        public int OrganizationRemovalRetryCount { get; set; }
        
        public long OrganizationCustomerMasterId { get; set; }

        public string OrganizationDomain { get; set; }

        public string OrganizationName { get; set; }
        
        public string RequestedBy { get; set; }
    }
}
