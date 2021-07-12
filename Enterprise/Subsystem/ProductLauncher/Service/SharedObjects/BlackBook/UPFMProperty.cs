using Newtonsoft.Json;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
{
    public class UPFMProperty
	{
        [JsonProperty("propertyInstanceSourceIds")]
        public List<string> id { get; set; }
    }
    //public class UPFMTranslatePropertyInstanceData
    //{
    //    [JsonProperty("data")]
    //    public TranslatePropertyInstanceData Data { get; set; }
    //}

    //public partial class TranslatePropertyInstanceData
    //{
    //    [JsonProperty("propertyInstanceSourceIds")]
    //    public List<string> id { get; set; }

    //}
}
