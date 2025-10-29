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
	/// Electronic Address Repository
	/// </summary>
	public class ElectronicAddressRepository : BaseRepository, IElectronicAddressRepository
	{
        ContactMechanismUsageTypeRepository _contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();

        #region Constructor
        /// <summary>
        /// Electronic Address base Constructor
        /// </summary>
        public ElectronicAddressRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public ElectronicAddressRepository(IRepository repository) : base(repository)
        {
            _contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository(repository);
        }
        #endregion

        #region public ElectronicAddressRepository methods
        /// <summary>
        /// Link an electronic address to a person
        /// </summary>
        /// <param name="electronicAddress">Person's Electronic Address parameter values</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateElectronicAddress(IElectronicAddress electronicAddress)
		{
			dynamic param = new
			{
				electronicAddress.ContactMechanismId,
				ElectronicAddressString = electronicAddress.AddressString,
				ElectronicAddressType = electronicAddress.AddressType
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateElectronicAddress, param);
				return result;
			}
		}

		/// <summary>
		/// List electronic address details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="contactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Electronic Address details for a person</returns>
		public IList<ElectronicAddress> ListElectronicAddressForPerson(Guid realPageId, string contactMechanismUsageTypeName = "")
		{
            IList<ContactMechanismUsageType> contactMechanismUsageTypeList = _contactMechanismUsageTypeRepository.ListContactMechanismUsageType(contactMechanismUsageTypeName);

            using (var repository = GetRepository())
			{

				IList<ElectronicAddress> result = repository.GetMany<ElectronicAddress>(StoredProcNameConstants.SP_ListEmailsForPerson, new { realPageId }).ToList();
				if (result != null)
				{
					foreach (var electronicAddress in result)
					{
						var usageType = contactMechanismUsageTypeList.First(i => i.ContactMechanismUsageTypeId == electronicAddress.ContactMechanismUsageTypeId);
						if (usageType != null)
						{
							electronicAddress.contactMechanismUsageType = usageType;
						}
					}
				}
				return result;
			}
		}

		/// <summary>
		/// List electronic address details for a person with login name and organization
		/// </summary>
		/// <param name="loginName">User lohin name</param>
		/// <param name="orgPartyId">Organization party id</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Electronic Address details for a person</returns>
		public IList<ElectronicAddress> ListElectronicAddressForPerson(string loginName,long orgPartyId, string ContactMechanismUsageTypeName = "")
		{
			using (var repository = GetRepository())
			{
				ContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
				IList<ContactMechanismUsageType> contactMechanismUsageTypeList = contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName);

				IList<ElectronicAddress> result = repository.GetMany<ElectronicAddress>(StoredProcNameConstants.SP_GetNotificationEmailForPerson, new { loginName , orgPartyId }).ToList();
				if (result != null)
				{
					foreach (var electronicAddress in result)
					{
						var usageType = contactMechanismUsageTypeList.First(i => i.ContactMechanismUsageTypeId == electronicAddress.ContactMechanismUsageTypeId);
						if (usageType != null)
						{
							electronicAddress.contactMechanismUsageType = usageType;
						}
					}
				}
				return result;
			}
		}
		#endregion
	}
}