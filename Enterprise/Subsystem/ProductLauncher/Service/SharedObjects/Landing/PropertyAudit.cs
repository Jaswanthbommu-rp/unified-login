using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class PropertyAudit
    {
        public string Name { get; set; }

        public string ProductInstanceId { get; set; }

        public string UPFMInstanceId { get; set; }

        public string UPFMName { get; set; }

        public string Status { get; set; } = "No ID";
    
    }
}
