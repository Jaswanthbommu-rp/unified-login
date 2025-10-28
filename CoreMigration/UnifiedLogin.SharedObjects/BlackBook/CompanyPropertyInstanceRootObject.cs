using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    public class UPFMPropertyInstanceRootObject
    {
        public List<UPFMPropertyInstanceData> data { get; set; }
    }

    public class UPFMPropertyInstanceData
    {
        public string type { get; set; }
        public UPFMPropertyInstanceAttributes attributes { get; set; }
    }

    public class UPFMPropertyInstanceAttributes
    {

        public List<PropertyInstance> propertyInstance { get; set; }
    }

    public class GetCompanyPropertyInstanceAttributes
    {
        public int propertyInstanceId { get; set; }
    }
}
