using System.Collections.Generic;

namespace RP.Enterprise.Foundation.DataAccess.Component.Model
{
    public class TableValueParmInfo
    {
        /// <summary>
        /// Stored Procedure Name
        /// </summary>
        public string StoredProcedureName { get; set; }

        /// <summary>
        /// Ordered Column Name
        /// </summary>
        public IEnumerable<string> OrderedColumnName { get; set; }

        /// <summary>
        /// Table Param Type Name
        /// </summary>
        public string TableParamTypeName { get; set; }

        /// <summary>
        /// Table Variable Name
        /// </summary>
        public string TableVariableName { get; set; }
    }
}
