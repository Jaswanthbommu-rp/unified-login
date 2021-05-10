using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	/// <summary>
	/// Suggested property for a user
	/// </summary>
	public class SuggestedPropertyResult
	{
		/// <summary>
		/// productPropertyId
		/// </summary>
		public long ProductPropertyId { get; set; } = 0;

		/// <summary>
		///propertyInstanceId
		/// </summary>
		public Guid PropertyInstanceId { get; set; }

		/// <summary>
		/// ProductId
		/// </summary>
		public int ProductId { get; set; }
    }
}
