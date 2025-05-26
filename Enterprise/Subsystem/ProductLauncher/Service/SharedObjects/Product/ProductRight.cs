using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product
{
    /// <summary>
    /// Used to store information about a product right
    /// </summary>
    public class ProductRight : IProductRight
    {
        /// <summary>
        /// THe id of the right
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// The description of the right
        /// </summary>
        public string Description { get; set; }
		/// <summary>
		/// The center the right belongs to
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string CenterName { get; set; }
        /// <summary>
        /// Is the right assigned
        /// </summary>
        public bool Assigned { get; set; }

        
        /// <summary>
        /// is the editor has right
        /// </summary>
        public bool isEditorHasRight { get; set; } = false;

        /// <summary>
        /// Used to store an alternate name for the right
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Alias { get; set; }
		/// <summary>
        /// How many roles is the right assigned to
        /// </summary>
        public int RolesAssigned { get; set; }
        /// <summary>
        /// Right description from OneSite
        /// </summary>
        public string UsageDescription { get; set; }

		/// <summary>
		/// Right description from Right
		/// </summary>
		public string RightDescription { get; set; }

		/// <summary>
		/// Used to document examples of the Rights Model webapi result
		/// </summary>
		[ExcludeFromCodeCoverage]
        public class RightExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Role example</returns>
            public object GetExamples()
            {
                IList<ProductRight> list = new List<ProductRight>();
                ProductRight example = new ProductRight()
                {
                    ID = 21,
                    Description = "A right description",
                    Assigned = false,
                    CenterName = "Core",
					Alias = "rghtshort",
                    RolesAssigned = 5
                };
                list.Add(example);
                ListResponse output = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = 1,
                    RowsPerPage = 1000,
                    TotalPages = 1
                };
                return output;
            }
        }
    }

    public class ProductRightAcct : IProductRight
    {
        /// <summary>
        /// THe id of the right
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// THe id of the right
        /// </summary>
        public int RightID { get; set; }
        /// <summary>
        /// The description of the right
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The center the right belongs to
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string CenterName { get; set; }
        /// <summary>
        /// Is the right assigned
        /// </summary>
        public bool Assigned { get; set; }
        /// <summary>
        /// Used to store an alternate name for the right
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }
        /// <summary>
        /// How many roles is the right assigned to
        /// </summary>
        public int RolesAssigned { get; set; }

        /// <summary>
        /// Used to store an ModuleID for the right
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ModuleID { get; set; }

        /// <summary>
        /// Used to store an Action value for the right
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }

        /// <summary>
        /// Used to store an Action value for the right
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ActionLabel { get; set; }

        /// <summary>
        /// Used to store an Right value for the right
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Right { get; set; }

    }
}
