using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class ProductSuggestedProperties
    {

        public ProductSuggestedProperties()
        {
			SuggestedPropertiesList = new List<SuggestedProperty>();
		}
		/// <summary>
		/// productId
		/// </summary>
		[JsonProperty(PropertyName = "productId")]
		public long ProductId { get; set; } = 0;

		/// <summary>
		/// List of suggested properties for the product
		/// </summary>
		[JsonProperty(PropertyName = "suggestedPropertiesList")]
		public List<SuggestedProperty> SuggestedPropertiesList { get; set; }
	}
}
