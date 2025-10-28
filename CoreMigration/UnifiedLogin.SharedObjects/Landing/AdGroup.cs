using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class AdGroup
    {
        public int ADGroupId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ADGroupName { get; set; }
        public Guid ActiveDirectoryId { get; set; }
    }
}
