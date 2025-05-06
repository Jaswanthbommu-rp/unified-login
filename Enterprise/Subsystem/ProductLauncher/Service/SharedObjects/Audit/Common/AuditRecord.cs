namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common
{
    /// <summary>
    /// Class for auditing properties
    /// </summary>
    public class AuditRecord
    {
        #region "Properties"

        /// <summary>
        /// Property old value
        /// </summary>
        public object OldValue { get; set; }

        /// <summary>
        /// Property new value
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// Audit message
        /// </summary>
        public string AuditMessage { get; set; }

        /// <summary>
        /// Log activity type
        /// </summary>
        public string LogActivityType { get; set; }

        /// <summary>
        /// Column name 
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;
        #endregion 
    }
}
