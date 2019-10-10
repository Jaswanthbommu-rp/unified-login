using Newtonsoft.Json;
using RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
    public class ListResponse <T> : ResponseBase
    {
        /// <summary>
        /// Data
        /// </summary>
        [JsonProperty(PropertyName = "data")]
        public IList<T> Records { get; set; }

        /// <summary>
        /// Rows Per Page
        /// </summary>
        public int RowsPerPage { get; set; }

        /// <summary>
        /// Skip Rows
        /// </summary>
        public int SkipRows { get; set; }

        /// <summary>
        /// Current Page
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Total Pages
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Total Rows
        /// </summary>
        [JsonProperty(PropertyName = "totalCount")]
        public int TotalRows { get; set; }

        /// <summary>
        /// Additional
        /// </summary>
        public object Additional { get; set; }
    }
}