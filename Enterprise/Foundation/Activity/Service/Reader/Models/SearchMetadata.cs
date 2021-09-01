using System.Collections.Generic;
using RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
    public class SearchMetadata
    {
        public List<LogCategory> LogCategoryList { get; set; }
        public List<LogType> LogTypeList { get; set; }
    }
}