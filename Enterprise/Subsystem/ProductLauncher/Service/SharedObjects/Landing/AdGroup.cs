using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class AdGroup
    {
        public int ADGroupId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ADGroupName { get; set; }
        public Guid ActiveDirectoryId { get; set; }
    }
}
