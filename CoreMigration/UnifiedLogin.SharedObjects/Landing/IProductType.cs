using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Interface for ProductType
    /// </summary>
    public interface IProductType
    {
		/// <summary>
		/// Parent Product Type Name
		/// </summary>
		string ParentProductTypeName { get; set; }
		
		/// <summary>
		/// Unique Identifier - Product Type Guid
		/// </summary>
		Guid ProductTypeGuid { get; set; }

		/// <summary>
		/// Unique Product Type Id
		/// </summary>
		int ProductTypeId { get; set; }

		/// <summary>
		/// Parent Product Type Id
		/// </summary>
		int? ParentProductTypeId { get; set; }

		/// <summary>
		/// Product Type Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Product Type Description
		/// </summary>
		string Description { get; set; }
	}
}
