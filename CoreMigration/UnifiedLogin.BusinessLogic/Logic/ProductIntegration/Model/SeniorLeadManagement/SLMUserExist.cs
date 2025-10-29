using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model.SeniorLeadManagement
{
    public class SLMUserExist
    {
        public int status { get; set; }
        public string Message { get; set; }
        public List<string> errors;
    }
}
