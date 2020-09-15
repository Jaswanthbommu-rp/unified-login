using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
{
    public class ProductCenterEnablement
    {
        public string EnabledBy { get; set; }


        public List<ProductCenterEnablementSettings> Details { get; set; }
    }

    public class ProductCenterEnablementSettings
    {
        public int CustomerCompanyId { get; set; }
        public string CompanyInstanceSourceId { get; set; }
        public string CustomerPropertyId { get; set; }
        public string PropertyInstanceSourceId { get; set; }
        public string ProductCenterSourceId { get; set; }
        public string Source { get; set; }
        public string CustomerEnvironment { get; set; }
    }
}
