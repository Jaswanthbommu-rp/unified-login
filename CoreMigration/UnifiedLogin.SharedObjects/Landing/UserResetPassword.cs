using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class UserResetPassword
    {
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
        public Guid?  RealPageId   { get; set; }
    }
}
