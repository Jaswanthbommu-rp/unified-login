namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Attribute
{
    /// <summary>
    /// Attribute to audit properties in models
    /// </summary>
    public class AuditLogAttribute : System.Attribute
    {
        #region "Properties"

        /// <summary>
        /// Name of the column
        /// </summary>
        public string ColumnName { get; private set; }

        /// <summary>
        /// Value for nulls
        /// </summary>
        public string NullValue { get; private set; }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="columName">Column name</param>
        /// <param name="nullValue">Value for nulls</param>
        public AuditLogAttribute(string columName, string nullValue = "a blank value")
        {
            ColumnName = columName;
            NullValue = nullValue;
        }

        #endregion 
    }
}
