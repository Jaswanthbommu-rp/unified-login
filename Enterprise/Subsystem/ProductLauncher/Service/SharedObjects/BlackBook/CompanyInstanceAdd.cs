using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
{
    public class CompanyInstanceAdd : CompanyInstance
    {
        [JsonIgnore]
        public long Id { get;set; }
    }
}
