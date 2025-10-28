using UnifiedLogin.SharedObjects.Constants;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Attribute
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

        /// <summary>
        /// The message must be a json serialized dictionary
        /// </summary>
        public string Message { get; private set; }

        #endregion

        #region "Constructors"

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="columName">Column name</param>
        /// <param name="format">Property format</param>
        /// <param name="logActivityTypeConstant">Activity log value</param>
        /// <param name="message">The message must be a json serialized dictionary></param>
        /// <param name="nullBlankValue">Value for nulls or empty</param>
        public AuditLogAttribute(string columName, string logActivityTypeConstant, string format = "",  string message = "", string nullBlankValue = "a blank value")
        {
            ColumnName = columName;
            NullBlankValue = nullBlankValue;
            LogActivityTypeConstant = logActivityTypeConstant;
            Format = format;
            Message = message;
        }

        #endregion 
    }
}
