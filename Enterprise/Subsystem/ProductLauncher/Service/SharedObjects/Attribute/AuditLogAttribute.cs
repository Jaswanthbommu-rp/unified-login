using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;

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

        /// <summary>
        /// Activity log value
        /// </summary>
        public string LogActivityTypeConstant { get; private set; }

        /// <summary>
        /// Property format
        /// </summary>
        public string Format { get; private set; }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="columName">Column name</param>
        /// <param name="format">Property format</param>
        /// <param name="logActivityTypeConstant">Activity log value</param>
        /// <param name="nullBlankValue">Value for nulls or empty</param>
        public AuditLogAttribute(string columName, string logActivityTypeConstant, string format ="", string nullBlankValue = "a blank value")
        {
            ColumnName = columName;
            NullBlankValue = nullBlankValue;
            LogActivityTypeConstant = logActivityTypeConstant;
            Format = format;
        }

        #endregion 
    }
}
