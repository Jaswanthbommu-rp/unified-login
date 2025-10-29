using System;
using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage Contact Mechanism repository calls
	/// </summary>
	public class ManageContactMechanism : IManageContactMechanism
	{
		#region Private Variables
		IContactMechanismRepository _contactMechanismRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="contactMechanismRepository">Contact Mechanism Repository</param>
		public ManageContactMechanism(IContactMechanismRepository contactMechanismRepository)
		{
			_contactMechanismRepository = contactMechanismRepository;
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="repository"></param>
        public ManageContactMechanism(IRepository repository)
        {
            _contactMechanismRepository = new ContactMechanismRepository(repository);
        }

		/// <summary>
		/// Create a basic instance of the ManageContactMechanism Controller class
		/// </summary>
		/// 
		public ManageContactMechanism()
		{
			_contactMechanismRepository = new ContactMechanismRepository();
		}
		#endregion

		#region Public ManageContactMechanism methods
		/// <summary>
		/// Create a new Contact Mechanism.  Used to Add/Link a Postal Address to a person
		/// </summary>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateContactMechanism()
		{
			return _contactMechanismRepository.CreateContactMechanism();
		}

		/// <summary>
		/// List contact mechanism details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Contact Mechanism details for a person</returns>
		public IList<CommonAddress> ListContactMechanismForPerson(Guid realPageId, string ContactMechanismUsageTypeName)
		{
			if (realPageId == Guid.Empty)
			{
				throw new Exception("Invalid parameter realPageId.");
			}

			return _contactMechanismRepository.ListContactMechanismForPerson(realPageId, ContactMechanismUsageTypeName);
		}

		/// <summary>
		/// Create a new Contact Mechanism.  Used to Add/Link a Postal Address to a person
		/// </summary>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkContactMechanismToParty(Guid realPageId, IPartyContactMechanism partyContactMechanism)
		{
			if (realPageId == Guid.Empty)
			{
				throw new Exception("Invalid parameter realPageId.");
			}

			if (partyContactMechanism == null)
			{
				throw new ArgumentNullException(nameof(partyContactMechanism), "Null PartyContactMechanism.");
			}

			return _contactMechanismRepository.LinkContactMechanismToParty(realPageId, partyContactMechanism);
		}

		/// <summary>
		/// Link Geographic Boundary To Contact Mechanism
		/// </summary>
		/// <param name="contactMechanismBoundary">ContactMechanismBoundary object</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkGeographicBoundaryToContactMechanism(IContactMechanismBoundary contactMechanismBoundary)
		{
			if (contactMechanismBoundary == null)
			{
				throw new ArgumentNullException(nameof(contactMechanismBoundary), "Null ContactMechanismBoundary.");
			}

			return _contactMechanismRepository.LinkGeographicBoundaryToContactMechanism(contactMechanismBoundary);
		}

		/// <summary>
		/// Link UsageType to Party Contact Mechanism
		/// </summary>
		/// <param name="PartyContactMechanismID"> Contact Mechanism associated to a Party</param>
		/// <param name="ContactMechanismUsageTypeId">Defines the contact usage. Such as Personal, Work, or Account Recovery.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkUsageTypeToPartyContactMechanism(long PartyContactMechanismID, int? ContactMechanismUsageTypeId)
		{
			if (PartyContactMechanismID <= 0)
			{
				throw new Exception("Missing Party Contact Mechanism Id.");
			}

			return _contactMechanismRepository.LinkUsageTypeToPartyContactMechanism(PartyContactMechanismID, ContactMechanismUsageTypeId);
		}

		/// <summary>
		/// Update Contact Mechanism Usage For Party
		/// </summary>
		/// <param name="PartyContactMechanismID"> Contact Mechanism associated to a Party</param>
		/// <param name="ContactMechanismUsageTypeId">Defines the contact usage. Such as Personal, Work, or Account Recovery.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse UpdateContactMechanismUsageForParty(long PartyContactMechanismID, int? ContactMechanismUsageTypeId)
		{
			if (PartyContactMechanismID <= 0)
			{
				throw new Exception("Missing Party Contact Mechanism Id.");
			}

			return _contactMechanismRepository.UpdateContactMechanismUsageForParty(PartyContactMechanismID, ContactMechanismUsageTypeId);
		}
		#endregion
	}
}