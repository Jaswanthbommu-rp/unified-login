using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    public class CompanyInstanceAttributes
	{
        /// <summary>
        /// CompanyInstanceId
        /// </summary>
        public int CompanyInstanceId { get; set; }

        /// <summary>
        /// CompanyInstanceSourceId
        /// </summary>
        public string CompanyInstanceSourceId { get; set; }

        /// <summary>
        /// Source
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Domain
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// CompanyName
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// GreenBookCares
        /// </summary>
        public string GreenBookCares { get; set; }

        public List<CustomerCompanyMap> CustomerCompanyMap { get; set; }
    }

    public class BooksCompanyInstance
    {
        [JsonIgnore]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "attributes", NullValueHandling = NullValueHandling.Ignore)]
        public CompanyInstanceAttributes Attributes { get; set; }
    }
}
