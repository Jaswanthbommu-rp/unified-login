using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage Telecommunication Number repository calls
	/// </summary>
	public interface IManageTelecommunicationNumber
	{
		/// <summary>
		/// Link telecommunication number to a person
		/// </summary>
		/// <param name="telecommunicationNumber">Person's telecommunication number parameter values</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateTelecommunicationNumber(ITelecommunicationNumber telecommunicationNumber);

		/// <summary>
		/// List telecommunication number details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of telecommunication number details for a person</returns>
		IList<TelecommunicationNumber> ListTelecommunicationNumberForPerson(Guid realPageId, string ContactMechanismUsageTypeName);
	}
}