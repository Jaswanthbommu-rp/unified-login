using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Interface for DataList
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IDataList<T>
	{
		/// <summary>
		/// List of collection of data
		/// </summary>
		IList<T> data { get; set; }
	}
}