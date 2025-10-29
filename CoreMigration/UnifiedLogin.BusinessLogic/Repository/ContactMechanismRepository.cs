using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Contact Mechanism Repository
	/// </summary>
	public class ContactMechanismRepository : BaseRepository, IContactMechanismRepository
	{
		#region Constructor
		/// <summary>
		/// Contact Mechanism base Constructor
		/// </summary>
		public ContactMechanismRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public ContactMechanismRepository(IRepository repository) : base(repository)
        {
        }
		#endregion

		#region public ContactMechanismRepository methods
		/// <summary>
		/// Create a new Contact Mechanism.  Used to Add/Link a Postal Address to a person
		/// </summary>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateContactMechanism()
		{
			int ContactMechanismId = 0;
			dynamic param = new
			{
				ContactMechanismId = ContactMechanismId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateContactMechanism, param);
				return result;
			}
		}

		/// <summary>
		/// List Contact Mechanism details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Contact Mechanism details for a person</returns>
		public IList<CommonAddress> ListContactMechanismForPerson(Guid realPageId, string ContactMechanismUsageTypeName)
		{
			ContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
			using (var repository = GetRepository())
			{
				IList<ContactMechanismUsageType> contactMechanismUsageTypeList = contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName);

				IList<CommonAddress> result = repository.GetMany<CommonAddress>(StoredProcNameConstants.SP_ListContactMechanismsForPerson, new { realPageId }).ToList();
				if (result != null)
				{
					foreach (var contactMechanism in result)
					{
						var usageType = contactMechanismUsageTypeList.FirstOrDefault(i => i.ContactMechanismUsageTypeId == contactMechanism.ContactMechanismUsageTypeId);
						if (usageType != null)
						{
							contactMechanism.contactMechanismUsageType = usageType;
						}
					}
				}
				return result;
			}
		}

		/// <summary>
		/// Link Contact Mechanism To a Party
		/// </summary>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkContactMechanismToParty(Guid realPageId, IPartyContactMechanism partyContactMechanism)
		{
			dynamic param = new
			{
				realPageId,
				partyContactMechanism.PartyContactMechanismId,
				partyContactMechanism.ContactMechanismId,
				partyContactMechanism.FromDate,
				partyContactMechanism.ThruDate
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkContactMechanismToParty, param);
				return result;
			}
		}

		/// <summary>
		/// Link Geographic Boundary To Contact Mechanism
		/// </summary>
		/// <param name="contactMechanismBoundary">ContactMechanismBoundary object</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkGeographicBoundaryToContactMechanism(IContactMechanismBoundary contactMechanismBoundary)
		{
			dynamic param = new
			{
				contactMechanismBoundary.ContactMechanismId,
				contactMechanismBoundary.GeographicBoundaryId,
				contactMechanismBoundary.FromDate,
				contactMechanismBoundary.ThruDate
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkGeographicBoundaryToContactMechanism, param);
				return result;
			}
		}

		/// <summary>
		/// Link UsageType to Party Contact Mechanism
		/// </summary>
		/// <param name="PartyContactMechanismID"> Contact Mechanism associated to a Party</param>
		/// <param name="ContactMechanismUsageTypeId">Defines the contact usage. Such as Personal, Work, or Account Recovery.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkUsageTypeToPartyContactMechanism(long PartyContactMechanismID, int? ContactMechanismUsageTypeId)
		{
			dynamic param = new
			{
				PartyContactMechanismID,
				ContactMechanismUsageTypeId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkUsageTypeToPartyContactMechanism, param);
				return result;
			}
		}

		/// <summary>
		/// Update Contact Mechanism Usage For Party
		/// </summary>
		/// <param name="PartyContactMechanismID"> Contact Mechanism associated to a Party</param>
		/// <param name="ContactMechanismUsageTypeId">Defines the contact usage. Such as Personal, Work, or Account Recovery.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse UpdateContactMechanismUsageForParty(long PartyContactMechanismID, int? ContactMechanismUsageTypeId)
		{
			dynamic param = new
			{
				PartyContactMechanismID,
				ContactMechanismUsageTypeId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdateContactMechanismUsageForParty, param);
				return result;
			}
		}
#endregion
    }
}