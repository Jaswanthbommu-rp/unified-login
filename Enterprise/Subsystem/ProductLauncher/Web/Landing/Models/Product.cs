using System.Collections.Generic;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models
{
    public class Product
    {
        [JsonProperty(PropertyName = "titleIDGuid")]
        public string TitleUniqueId { get; set; }

        [JsonProperty(PropertyName = "titleID")]
        public string TitleId { get; set; }

        [JsonProperty(PropertyName = "subDescription")]
        public string SubDescription { get; set; }

        [JsonProperty(PropertyName = "className")]
        public string ClassName { get; set; }

        [JsonProperty(PropertyName = "settingsUrl")]
        public string SettingsUrl { get; set; }

        [JsonProperty(PropertyName = "productUrl")]
        public string ProductUrl { get; set; }

        [JsonProperty(PropertyName = "activitiesList")]
        public IList<Activities> ActivitiesList { get; set; }

        [JsonProperty(PropertyName = "newTab")]
        public bool NewTab { get; set; } = false;
    }
}