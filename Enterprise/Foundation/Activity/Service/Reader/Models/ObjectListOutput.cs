using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
    /// <summary>
    /// UI Output JSON.  Used for the result UI.
    /// </summary>
    /// <typeparam name="T1">data list Generic type</typeparam>
    /// <typeparam name="T2">error Generic type</typeparam>
    [ExcludeFromCodeCoverage]
    public class ObjectListOutput<T1, T2> 
    {
        /// <summary>
        /// List of collection of data
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public IList<T1> list { get; set; }

        /// <summary>
        /// Error status object - API/UI Call Success/Error Communication
        /// </summary>
        [JsonProperty("Status", NullValueHandling = NullValueHandling.Ignore)]
        public Status<T2> Status { get; set; }
    }
}