using System;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Command.Models
{
    [Serializable]
    public class AdditionalParameters
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}