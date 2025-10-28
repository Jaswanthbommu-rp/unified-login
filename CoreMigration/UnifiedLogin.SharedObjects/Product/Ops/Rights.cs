using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
   public  class MainGroup
    {
        public string mainID { get; set; }
        public string mainName { get; set; }
        public List<SubGroup> subGroupList { get; set; }
    }

    public class SubGroup
    {
        public string subID { get; set; }
        public string subName { get; set; }
        public List<RightDetails> rightsList { get; set; }
    }

    public class RightDetails
    {
        public string rightID { get; set; }
        public string right { get; set; }
        public string description { get; set; }
        public string more { get; set; }
        public string value { get; set; }
        public string name { get; set; }
        public bool isAssigned { get; set; }
        public bool isCompliance { get; set; }
        public bool isWarnAssigned { get; set; }
        public string warntext { get; set; }

    }
}
