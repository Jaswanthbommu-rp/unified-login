using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnifiedLogin.SharedObjects.Swagger;

namespace UnifiedLogin.SharedObjects.Product
{
    /// <summary>
    /// 
    /// </summary>
    public class ProductRole : IProductRole
    {
        /// <summary>
        /// The unique id of the role in the product
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of the role in the product
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description for the role
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Is the role assigned to the user in the product
        /// </summary>
        public bool IsAssigned { get; set; }

        /// <summary>
        /// role attribute - accessAllProperties - true/false
        /// </summary>
        public bool accessAllProperties { get; set; } = false;

        /// <summary>
        /// Is editor has the right
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public bool isEditorHasRight { get; set; }

        
        /// <summary>
        /// The type of role
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Roletype { get; set; }
		
		/// <summary>
		/// Used to store an alternate name for the role
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Alias { get; set; }
		
		/// <summary>
		/// The number of rights assigned to the role
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RightsAssigned { get; set; }

		/// <summary>
		/// Is the Default role assigned 
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string DefaultRole { get; set; }

        /// <summary>
        /// Property List
        /// </summary>

        public IList<object> propertiesList { get; set; }

        /// <summary>
        /// for custom sort
        /// </summary>
        public int SortId { get; set; }

        #region Examples
        /// <summary>
        /// Used to document examples of the Role Model webapi result
        /// </summary>
        [ExcludeFromCodeCoverage]
        public class RoleExample : IProvideExamples
        {
            /// <summary>
            /// Example object data used by Swagger to document the output of the webapi method
            /// </summary>
            /// <returns>Role example</returns>
            public object GetExamples()
            {
                IList<ProductRole> list = new List<ProductRole>();
                ProductRole example = new ProductRole()
                {
                    ID = "21",
                    Name = "Test Role",
                    Description = "Test Description",
                    IsAssigned = true,
                    RightsAssigned = "999",
                    Roletype = "Role type"
                };
                list.Add(example);
                ListResponse output = new ListResponse()
                {
                    Records = list.Cast<object>().ToList(),
                    TotalRows = 1,
                    RowsPerPage = 1000,
                    TotalPages = 1,
                };
                return output;
            }
        }
        #endregion
    }

    public class ProductRoleAcct : IProductRole
    {
        /// <summary>
        /// The unique id of the role in the product
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// The name of the role in the product
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A description for the role
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        /// <summary>
        /// Is the role assigned to the user in the product
        /// </summary>
        public bool IsAssigned { get; set; }

        /// <summary>
        /// The type of role
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Roletype { get; set; }

        /// <summary>
        /// Used to store an alternate name for the role
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Alias { get; set; }

        /// <summary>
        /// The number of rights assigned to the role
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string RightsAssigned { get; set; }

       
    }
}
