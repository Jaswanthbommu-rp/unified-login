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
        /// Value for nulls or empty
        /// </summary>
        public string NullBlankValue { get; private set; }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="columName">Column name</param>
        /// <param name="nullBlankValue">Value for nulls or empty</param>
        public AuditLogAttribute(string columName, string nullBlankValue = "a blank value")
        {
            ColumnName = columName;
            NullBlankValue = nullBlankValue;
        }

        #endregion 
    }
}
