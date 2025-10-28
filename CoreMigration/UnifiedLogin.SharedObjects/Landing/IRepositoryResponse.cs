using System;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for Repository Response
	/// </summary>
	public interface IRepositoryResponse
	{
		/// <summary>
		/// Returns the message text of the error that caused the CATCH block of a TRY…CATCH
		/// </summary>		
		string ErrorMessage { get; set; }

		/// <summary>
		/// Id of the updated/Inserted row
		/// </summary>
		long Id { get; set; }

		/// <summary>
		/// User master unique identifier
		/// </summary>
		Guid RealPageId { get; set; }
	}
}