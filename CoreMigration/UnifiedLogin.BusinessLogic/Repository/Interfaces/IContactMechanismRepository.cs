using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for ContactMechanismRepository.cs
	/// </summary>
	public interface IContactMechanismRepository
	{
		/// <summary>
		/// Create a new Contact Mechanism.  Used to Add/Link a Postal Address to a person
		/// </summary>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateContactMechanism();

		/// <summary>
		/// List Contact Mechanism details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Contact Mechanism details for a person</returns>
		IList<CommonAddress> ListContactMechanismForPerson(Guid realPageId, string ContactMechanismUsageTypeName);

		/// <summary>
		/// Create a new Contact Mechanism.  Used to Add/Link a Postal Address to a person
		/// </summary>
		/// <returns>Repository response object</returns>
		RepositoryResponse LinkContactMechanismToParty(Guid realPageId, IPartyContactMechanism partyContactMechanism);

		/// <summary>
		/// Link Geographic Boundary To Contact Mechanism
		/// </summary>
		/// <param name="contactMechanismBoundary">ContactMechanismBoundary object</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse LinkGeographicBoundaryToContactMechanism(IContactMechanismBoundary contactMechanismBoundary);

		/// <summary>
		/// Link UsageType to Party Contact Mechanism
		/// </summary>
		/// <param name="PartyContactMechanismID"> Contact Mechanism associated to a Party</param>
		/// <param name="ContactMechanismUsageTypeId">Defines the contact usage. Such as Personal, Work, or Account Recovery.</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse LinkUsageTypeToPartyContactMechanism(long PartyContactMechanismID, int? ContactMechanismUsageTypeId);

		/// <summary>
		/// Update Contact Mechanism Usage For Party
		/// </summary>
		/// <param name="PartyContactMechanismID"> Contact Mechanism associated to a Party</param>
		/// <param name="ContactMechanismUsageTypeId">Defines the contact usage. Such as Personal, Work, or Account Recovery.</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse UpdateContactMechanismUsageForParty(long PartyContactMechanismID, int? ContactMechanismUsageTypeId);
	}
}