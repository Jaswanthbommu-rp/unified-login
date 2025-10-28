using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product.ResidentPortal
{
	/// <summary>
	/// Interface for Messaging group
	/// </summary>
	public interface IMessagingGroups
	{
		/// <summary>
		/// Messaging group Id
		/// </summary>
		string Id { get; set; }

		/// <summary>
		/// Messaging group Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Is a Messaging group selected
		/// </summary>
		bool IsAssigned { get; set; }
	}
}