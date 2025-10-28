using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Product Family
    /// </summary>
    public class ProductFamily : IProductFamily
	{
		/// <summary>
		/// Unique Product Type Id
		/// </summary>
		[JsonProperty(PropertyName = "FamilyId")]
		public int ProductTypeId { get; set; } = 0;

		/// <summary>
		/// Product Type Name
		/// </summary>
		[JsonProperty(PropertyName = "Title")]
		public string Name { get; set; } = string.Empty;

		/// <summary>
		/// Product Type Description
		/// </summary>
		[JsonIgnore]
		[JsonProperty(PropertyName = "Description")]
		public string Description { get; set; } = string.Empty;

		/// <summary>
		/// List of Products
		/// </summary>
		public IList<Solution> Solutions { get; set; } = new List<Solution>();
    }
}
