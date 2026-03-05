namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Kafka
{
    /// <summary>
    /// Avro schema model for unified login user activation or deactivation
    /// Namespace: com.realpage.avro.unity.unifiedlogin
    /// </summary>
    public class UnifiedLoginUserStatusEvent
    {
        /// <summary>
        /// PersonaId of the unified login user
        /// </summary>
        public long? personaId { get; set; }

        /// <summary>
        /// Username or login name of the user
        /// </summary>
        public string user_login_name { get; set; }

        /// <summary>
        /// Status of the user (true = Active, false = Disabled)
        /// </summary>
        public bool? is_active { get; set; }

        /// <summary>
        /// User's last updated date (UTC) when user is deactivated or activated
        /// Stored as days since Unix epoch (1970-01-01)
        /// </summary>
        public int? user_activation_deactivation_date { get; set; }
    }
}