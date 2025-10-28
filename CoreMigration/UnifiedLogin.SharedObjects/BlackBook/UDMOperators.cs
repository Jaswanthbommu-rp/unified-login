using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
	public class UDMOperatorsRootObject
	{
        [JsonProperty(PropertyName = "data")]
		public UDMOperatorsDataObject Data { get; set; }
	}
	public class UDMOperators
	{
        [JsonProperty(PropertyName = "origin")]
		public UDMOperatorsDetails Origin { get; set; }
        
        [JsonProperty(PropertyName = "translations")]
		public List<UDMOperatorsDetails> Translations { get; set; }
	}

	public class UDMOperatorsAttributesObject
	{
        [JsonProperty(PropertyName = "operators")]
		public List<UDMOperators> booksOperators { get; set; }
	}

	public class UDMOperatorsDataObject
	{
		public string type { get; set; }
		public UDMOperatorsAttributesObject attributes { get; set; }
	}

    public class UDMOperatorsDetails
    {
		public string Source { get; set; }
        public string CompanyInstanceSourceId { get; set; }
        public string CompanyName { get; set; }

	}
}
