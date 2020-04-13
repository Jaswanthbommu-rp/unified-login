namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.UserUpdate
{
    public class UserBatchEntity
    {
        public string UserTypeChangedToFromExternal { get; set; }

        public int BatchProcessUserType { get; set; }

        public bool UserTypeChanged { get; set; }

        public string UserTypeName { get; set; }

        public bool IsUserTypeChangedFromNoEmailToRegular { get; set; }

        public bool IsUserTypeChangedFromNoEmailToExternal { get; set; }

    }
}
