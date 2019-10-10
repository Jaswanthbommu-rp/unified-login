using System.Collections.Generic;
using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models
{
    public class UserProduct
    {
        [JsonProperty(PropertyName = "userId")]
        public int UserId { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "companyName")]
        public string CompanyName { get; set; }

        [JsonProperty(PropertyName = "email")]
        public string Email { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "summaryCounts")]
        public SummaryCounts SummaryCount { get; set; }

        [JsonProperty(PropertyName = "initialProducts")]
        public IList<Product> AssignedProducts { get; set; }
    }
}