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
	public interface IContactMechanismRepositoryAsync
	{
		/// <summary>
		/// Create a new Contact Mechanism.  Used to Add/Link a Postal Address to a person
		/// </summary>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> CreateContactMechanismAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// List Contact Mechanism details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Contact Mechanism details for a person</returns>
		Task<IList<CommonAddress>> ListContactMechanismForPersonAsync(Guid realPageId, string ContactMechanismUsageTypeName, CancellationToken cancellationToken = default);

		/// <summary>
		/// Create a new Contact Mechanism.  Used to Add/Link a Postal Address to a person
		/// </summary>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> LinkContactMechanismToPartyAsync(Guid realPageId, IPartyContactMechanism partyContactMechanism, CancellationToken cancellationToken = default);

		/// <summary>
		/// Link Geographic Boundary To Contact Mechanism
		/// </summary>
		/// <param name="contactMechanismBoundary">ContactMechanismBoundary object</param>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> LinkGeographicBoundaryToContactMechanismAsync(IContactMechanismBoundary contactMechanismBoundary, CancellationToken cancellationToken = default);

		/// <summary>
		/// Link UsageType to Party Contact Mechanism
		/// </summary>
		/// <param name="PartyContactMechanismID"> Contact Mechanism associated to a Party</param>
		/// <param name="ContactMechanismUsageTypeId">Defines the contact usage. Such as Personal, Work, or Account Recovery.</param>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> LinkUsageTypeToPartyContactMechanismAsync(long PartyContactMechanismID, int? ContactMechanismUsageTypeId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Update Contact Mechanism Usage For Party
		/// </summary>
		/// <param name="PartyContactMechanismID"> Contact Mechanism associated to a Party</param>
		/// <param name="ContactMechanismUsageTypeId">Defines the contact usage. Such as Personal, Work, or Account Recovery.</param>
		/// <returns>Repository response object</returns>
		Task<RepositoryResponse> UpdateContactMechanismUsageForPartyAsync(long PartyContactMechanismID, int? ContactMechanismUsageTypeId, CancellationToken cancellationToken = default);
	}
}