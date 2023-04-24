using System;
using System.Collections.Generic;
using System.Linq;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Telecommunication Number Repository
	/// </summary>
	public class TelecommunicationNumberRepository : BaseRepository, ITelecommunicationNumberRepository
	{
		#region Constructor
		/// <summary>
		/// Telecommunication Number base Constructor
		/// </summary>
		public TelecommunicationNumberRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}
		#endregion

		#region public ElectronicAddressRepository methods
		/// <summary>
		/// Link a telecommunication number details for a person
		/// </summary>
		/// <param name="telecommunicationNumber">Person's telecommunication number parameter values</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateTelecommunicationNumber(ITelecommunicationNumber telecommunicationNumber)
		{
			dynamic param = new
			{
				telecommunicationNumber.ContactMechanismId,
				telecommunicationNumber.AreaCode,
				telecommunicationNumber.CountryCode,
				telecommunicationNumber.PhoneNumber,
				telecommunicationNumber.ISOCode

			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateTelecommunicationNumber, param);
				return result;
			}
		}

		/// <summary>
		/// List telecommunication number datails for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of telecommunication number details for a person</returns>
		public IList<TelecommunicationNumber> ListTelecommunicationNumberForPerson(Guid realPageId, string ContactMechanismUsageTypeName = "")
		{
			using (var repository = GetRepository())
			{
				ContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
				IList<ContactMechanismUsageType> contactMechanismUsageTypeList = contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName);

				IList<TelecommunicationNumber> result = repository.GetMany<TelecommunicationNumber>(StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson, new { realPageId }).ToList();
				if (result != null)
				{
					foreach (var telecommunicationNumber in result)
					{
						var usageType = contactMechanismUsageTypeList.First(i => i.ContactMechanismUsageTypeId == telecommunicationNumber.ContactMechanismUsageTypeId);
						if (usageType != null)
						{
							telecommunicationNumber.contactMechanismUsageType = usageType;
						}
					}
				}
				return result;
			}
		}
		#endregion
	}
}