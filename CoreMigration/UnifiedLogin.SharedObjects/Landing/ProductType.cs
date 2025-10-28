using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Product Type
    /// </summary>
    public class ProductType : IProductType
	{
		/// <summary>
		/// Unique Identifier - Product Type Guid
		/// </summary>
		[JsonProperty(PropertyName = "ProductTypeGuid")]
		public Guid ProductTypeGuid { get; set; }

		/// <summary>
		/// Unique Product Type Id
		/// </summary>
		[JsonProperty(PropertyName = "ProductTypeId")]
		public int ProductTypeId { get; set; }

		/// <summary>
		/// Parent Product Type Id
		/// </summary>
		[JsonProperty(PropertyName = "ParentProductTypeId")]
		public int? ParentProductTypeId { get; set; }

		/// <summary>
		/// Product Type Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }

		/// <summary>
		/// Product Type Description
		/// </summary>
		[JsonProperty(PropertyName = "Description")]
		public string Description { get; set; }

		/// <summary>
		/// Parent Product Type Name
		/// </summary>
		[JsonProperty(PropertyName = "ParentProductTypeName")]
		public string ParentProductTypeName { get; set; }
	}
}
