using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ProductBatch
	/// </summary>
	public interface IProductBatch
	{
		/// <summary>
		/// Assigned to PersonaId
		/// </summary>
		long AssignUserPersonaId { get; set; }

		/// <summary>
		/// Product batch create datetime
		/// </summary>
		DateTime CreatedDate { get; set; }

		/// <summary>
		/// Created By PersomaId
		/// </summary>
		long CreateUserPersonaId { get; set; }

		/// <summary>
		/// Error details
		/// </summary>
		string ErrorDetails { get; set; }

		/// <summary>
		/// Product API input JSON
		/// </summary>
		RolePropertyList InputJson { get; set; }

		/// <summary>
		/// Product API Last run datetime
		/// </summary>
		DateTime LastRunDate { get; set; }

		/// <summary>
		/// Product batch modified datetime
		/// </summary>
		DateTime ModifiedDate { get; set; }

		/// <summary>
		/// Person PartyId
		/// </summary>
		long PersonPartyId { get; set; }

		/// <summary>
		/// Unique Product Batch Id
		/// </summary>
		int ProductBatchId { get; set; }

		/// <summary>
		/// ProductId
		/// </summary>
		int ProductId { get; set; }

		/// <summary>
		/// Unique Identifier - EnterpriseUserId
		/// </summary>
		Guid RealPageId { get; set; }

		/// <summary>
		/// Retry count - used to call the API for this Product
		/// </summary>
		byte RetryCount { get; set; }

		/// <summary>
		/// Product Batch Status (Waiting, Running, Error, and Success)
		/// </summary>
		int StatusTypeId { get; set; }

		/// <summary>
		/// Batch Group GUID
		/// </summary>
		int BatchProcessorGroupId { get; set; }
	}
}