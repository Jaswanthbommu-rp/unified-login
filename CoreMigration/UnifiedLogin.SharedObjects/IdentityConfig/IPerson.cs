using System;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    /// <summary>
    /// Interface for IPerson
    /// </summary>
    public interface IPerson
	{
		/// <summary>
		/// Firstname
		/// </summary>
		string FirstName { get; set; }

		/// <summary>
		/// Lastname
		/// </summary>
		string LastName { get; set; }

		/// <summary>
		/// Middle initial
		/// </summary>
		string MiddleName { get; set; }

		/// <summary>
		/// EmployeeId
		/// </summary>
		 string EmployeeId { get; set; }

		/// <summary>
		/// PartyId
		/// </summary>
		long PartyId { get; set; }

		/// <summary>
		/// Preferred Contact Method Id (Email, Phone,....)
		/// </summary>
		int? PreferredContactMethodId { get; set; }

		/// <summary>
		/// Unique Identifier - EnterpriseUserId
		/// </summary>
		Guid RealPageId { get; set; }

		/// <summary>
		/// Suffix
		/// </summary>
		string Suffix { get; set; }

		/// <summary>
		/// Title
		/// </summary>
		string Title { get; set; }

        /// <summary>
        /// Is FirstName Null or WhiteSpace
        /// </summary>
        bool IsFirstNameNullOrWhiteSpace { get; }

		/// <summary>
		/// Is LastName Null or WhiteSpace
		/// </summary>
		bool IsLastNameNullOrWhiteSpace { get; }
	}
}