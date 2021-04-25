using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Maintenance
{
    public class OrganizationToDelete
    {
        public int OrganizationRemovalQueueId { get; set; }
        public long OrganizationPartyId { get; set; }
        public Guid OrganizationRealPageId { get; set; }

        public int OrganizationRemovalQueueStatusId { get; set; }
        public bool OrganizationRemoveUDMData { get; set; }
        public int OrganizationRemovalRetryCount { get; set; }
    }
}
