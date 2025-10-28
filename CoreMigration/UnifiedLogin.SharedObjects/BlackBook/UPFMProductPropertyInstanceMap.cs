using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.BlackBook
{
	public class UPFMProductPropertyInstanceAttribute
    {
        public string propertyInstanceSourceId { get; set; }
    }

    public class UPFMProductPropertyInstanceData
    {
        public string type { get; set; }
        public List<UPFMProductPropertyInstanceAttribute> attributes { get; set; }
    }

    public class UPFMProductPropertyInstanceMap
    {
        public UPFMProductPropertyInstanceData data { get; set; }
    }
}
