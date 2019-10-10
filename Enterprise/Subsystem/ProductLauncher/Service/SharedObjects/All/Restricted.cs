using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.All
{
    public class Restricted
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public DataType Type { get; set; }
        public string Name { get; set; }
        public bool Allowed { get; set; }

        public enum DataType : short
        {
            Tab = 0,
            Field = 1
        }
    }

    
}
