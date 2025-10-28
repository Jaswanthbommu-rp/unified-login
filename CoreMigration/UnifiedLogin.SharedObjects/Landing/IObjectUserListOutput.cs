using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ObjectUserListOutput
	/// </summary>
	/// <typeparam name="T1">data list Generic type</typeparam>
	/// <typeparam name="T2">error Generic type</typeparam>
	public interface IObjectUserListOutput<T1, T2>
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
		bool OrganizationHasProductAssignmentError { get; set; }
	}
}
