using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.Ops
{
    /// <summary>
    /// 
    /// </summary>
    public class OpsPagination
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("total_records")]
        public int TotalRecords { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("page_number")]
        public int PageNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("page_size")]
        public int PageSize { get; set; }
    }
}
