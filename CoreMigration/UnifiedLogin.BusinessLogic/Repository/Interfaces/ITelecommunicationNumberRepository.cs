using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Telecommunication Number Repository
	/// </summary>
	public interface ITelecommunicationNumberRepository
	{
		/// <summary>
		/// Link a telecommunication number to a person
		/// </summary>
		/// <param name="telecommunicationNumber">Person's telecommunication number parameter values</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateTelecommunicationNumber(ITelecommunicationNumber telecommunicationNumber);

		/// <summary>
		/// List telecommunication number datails for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of telecommunication number details for a person</returns>
		IList<TelecommunicationNumber> ListTelecommunicationNumberForPerson(Guid realPageId, string ContactMechanismUsageTypeName);
	}
}