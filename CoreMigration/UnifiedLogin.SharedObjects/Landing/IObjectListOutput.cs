using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ObjectListOutput
	/// </summary>
	/// <typeparam name="T1">data list Generic type</typeparam>
	/// <typeparam name="T2">error Generic type</typeparam>
	public interface IObjectListOutput<T1, T2>
	{
		/// <summary>
		/// Error status object - API/UI Call Success/Error Communication
		/// </summary>
		Status<T2> Status { get; set; }

		/// <summary>
		/// Paging Summary
		/// </summary>
		PagingSummary pagingSummary { get; set; }

		/// <summary>
		/// List of collection of data
		/// </summary>
		IList<T1> list { get; set; }
	}
}