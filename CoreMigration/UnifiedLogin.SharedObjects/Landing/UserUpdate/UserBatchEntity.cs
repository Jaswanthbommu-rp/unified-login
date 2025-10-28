namespace UnifiedLogin.SharedObjects.Landing.UserUpdate
{
    public class UserBatchEntity
    {
        public string UserTypeChangedToFromExternal { get; set; } = string.Empty;

        public int BatchProcessUserType { get; set; }

        public bool UserTypeChanged { get; set; }

        public string UserTypeName { get; set; } = string.Empty;

        public bool IsUserTypeChangedFromNoEmailToRegular { get; set; }

        public bool IsUserTypeChangedFromNoEmailToExternal { get; set; }

    }
}
