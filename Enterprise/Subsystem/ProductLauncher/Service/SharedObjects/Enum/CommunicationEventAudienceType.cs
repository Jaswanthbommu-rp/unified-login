namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum
{
    /// <summary>
    /// 
    /// </summary>
    public enum CommunicationEventAudienceType : int
    {
        /// <summary>
        /// Super User
        /// </summary>
        SuperUser = 1,

        /// <summary>
        /// Regular User
        /// </summary>
        RegularUser = 2,

        /// <summary>
        /// External User
        /// </summary>
        ExternalUser = 3,

        /// <summary>
        /// Multi Company User 
        /// </summary>
        MultiCompanyUser = 4,

        /// <summary>
        /// MFA Code
        /// </summary>
        MFACode = 5
    }
}
