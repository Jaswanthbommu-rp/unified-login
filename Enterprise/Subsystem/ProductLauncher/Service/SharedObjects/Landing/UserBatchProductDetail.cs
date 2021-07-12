using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class UserBatchProductDetail
    {
        public int StatusTypeId { get; set; }
        
        public string Name { get; set; }
        
        public bool BatchProcessorGroupActivityLogged { get; set; }

        public string InputJSON { get; set; }

        public bool IsAssigned { get; set; }
    }
}
