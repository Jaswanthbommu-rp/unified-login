namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum
{
    public enum CommunicationEventPurposeType
    {
        /// <summary>
        /// Creating a New User
        /// </summary>
        NewUserSetup = 1,
        /// <summary>
        /// Resetting a Password
        /// </summary>
        PasswordReset = 2,
        /// <summary>
        /// Unlocking an Account
        /// </summary>
        UnlockAccount = 3,

        /// <summary>
        /// Account recovery
        /// </summary>
        AccountRecovery = 4,

        /// <summary>
        /// MFA one-time verification code
        /// </summary>
        MFAVerification = 5
    }
}
