namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants
{

    /// <summary>
    /// These constants used as search parameter
    /// </summary>
    public static class LogActivityTypeConstants
    {
        public const string LOGIN_SUCCESS = "Login success";
        public const string LOGIN_FAILURE = "Login failure";
        public const string CHANGE_PASSWORD_SUCCESS = "Change password success";
        public const string CHANGE_PASSWORD_FAILURE = "Change password failure";
		public const string CHANGE_SECURITY_QUESTIONS_SUCCESS = "Change security questions success";
		public const string CHANGE_SECURITY_QUESTIONS_FAILURE = "Change security questions failure";
		public const string USER_LOCKED = "User locked";
        public const string USER_UNLOCKED = "User unlocked";
        public const string CREATE_USER = "Create user";
        public const string UPDATE_USER = "Update user";
        public const string CLONE_USER = "Clone user";
        public const string LOGIN_ENABLED = "Login enabled";
        public const string LOGIN_DISABLED = "Login disabled";
        public const string PRODUCT_ACCESS = "Product access";
        public const string EMAIL_SENT = "Email sent";
        public const string EMAIL_RESENT = "Email resent";
        public const string EMAIL_RESETPASSWORDSENT = "Reset Password Email";
        public const string SIGNOUT = "Signout";
		public const string USER_EXPIRED = "User Expired";
        public const string COMPANY_CREATED = "Company Create";
        public const string COMPANY_UPDATED = "Company Update";
        public const string PRODUCT_ENABLED_FOR_COMPANY = "Product Enablement";
        public const string PRODUCT_DISABLED_FOR_COMPANY = "Product Disablement";
        public const string PROPERTY_CREATED = "Property Create";
        public const string PROPERTY_DELETED = "Property Delete";
        public const string PROPERTY_UPDATED = "Property Update";
        public const string USER_REQUESTED_NEW_ACTIVATION_LINK = "User requested new activation link";
        public const string USER_UPDATE_INTERNAL = "User Update - Internal";
        public const string CLIENT_SETTINGS_UPDATE = "Client Settings Update";
    }

    //public static class SecurityActivityMessageConstants
    //{
    //    public const string LOGIN_SUCCESS = "User {0} successfully logged-in.";

    //    public const string CHANGE_PASSWORD = "User {0} successfully changed forgotten password.";
    //    public const string RESET_PASSWORD = "User {0} successfully reset current password.";
    //    public const string SET_PASSWORD = "User {0} successfully set new password.";

    //    public const string USER_LOCKED_PASSWORD_FAIL = "User {0} locked while attempting to login.";
    //}

    //public static class UserActivityMessageConstants
    //{
    //    public const string NEW_USER = "New User {0} successfully created by user {1}.";
    //    public const string UPDATE_USER = "User {0} successfully updated by user {1}.";
    //    public const string CLONE_USER = "User {0} successfully cloned from user {1} by user {2}.";
    //    public const string ACTIVATE_USER = "User {0} successfully activated by user {1}.";
    //    public const string DISABLE_USER = "User {0} successfully disabled by user {1}.";
    //    public const string LOCK_USER_MANUAL = "User {0} successfully locked by user {1}.";
    //    public const string UNLOCK_USER_MANUAL = "User {0} successfully unlocked by user {1}.";
    //    public const string USER_PROFILE_UPDATE = "User {0} profile successfully updated by user {1}.";
    //}

    //public static class ProductAccessActivityMessageConstants
    //{
    //    public const string PRODUCT_ACCESS = "User {0} accessed product {1}.";

    //    public const string PRODUCT_USER_CREATED = "User {0} created in product {1} by user {2}.";
    //    public const string PRODUCT_USER_UPDATED = "User {0} is updated for product {1} by user {2}.";

    //    public const string PRODUCT_ACCESS_UNASSIGNED = "User {0} access is removed for product {1} by user {2}.";

    //    public const string PRODUCT_PROPERTIES_UPDATED = "Properties for product {0} are updated for User {1} by user {2}.";
    //    public const string PRODUCT_ROLES_UPDATED = "Roles for product {0} are updated for User {1} by user {2}.";
    //}

    //public static class EmailActivityMessageConstants
    //{
    //    public const string EMAIL_NEW_USER = "Email sent to user {0} after successful creation.";
    //    public const string RESEND_EMAIL_NEW_USER = "Resent Email to user {0} by user {1}.";
    //}
}
