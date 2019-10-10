using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class UserLockActivity
    {
        public string LockActivityCode { get; set; }
        //public string LockActivityDescription { get; set; }
        public string LockReason { get; set; }
        public bool IsLockActive { get; set; }
        public DateTime LockDateTime { get; set; }
    }
}