using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ProductFamily
	/// </summary>
	public interface IProductFamily
	{
		/// <summary>
		/// Product Type Description
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Product Type Name
		/// </summary>
		string Name { get; set; }
		
		/// <summary>
		/// Unique Product Type Id
		/// </summary>
		int ProductTypeId { get; set; }

		/// <summary>
		/// List of Products
		/// </summary>
		IList<Solution> Solutions { get; set; }
	}
}