using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    public class ProductSuggestedProperties
    {

        public ProductSuggestedProperties()
        {
			SuggestedProperiesList = new List<SuggestedProperty>();
		}
		/// <summary>
		/// productId
		/// </summary>
		[JsonProperty(PropertyName = "productId")]
		public long ProductId { get; set; } = 0;

		/// <summary>
		/// List of suggested properties for the product
		/// </summary>
		[JsonProperty(PropertyName = "suggestedProperiesList")]
		public List<SuggestedProperty> SuggestedProperiesList { get; set; }
	}
}
