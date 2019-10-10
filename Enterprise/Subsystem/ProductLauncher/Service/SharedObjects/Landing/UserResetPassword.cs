using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class UserResetPassword
    {
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
        public Guid?  RealPageId   { get; set; }
    }
}
