using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Postal Address Repository
	/// </summary>
	public class PostalAddressRepository : BaseRepository, IPostalAddressRepository
	{
		#region Constructor
		/// <summary>
		/// Postal Address base Constructor
		/// </summary>
		public PostalAddressRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}
		#endregion

		#region public PostalAddressRepository methods
		/// <summary>
		/// List postal address details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Postal Address details for a person</returns>
		public IList<PostalAddress> ListPostalAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName)
		{
			using (var repository = GetRepository())
			{
				ContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
				IList<ContactMechanismUsageType> contactMechanismUsageTypeList = contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName);

				IList<PostalAddress> result = repository.GetMany<PostalAddress>(StoredProcNameConstants.SP_ListPostalAddressesForPerson, new { realPageId }).ToList();
				if (result != null)
				{
					foreach (var postalAddress in result)
					{
						var usageType = contactMechanismUsageTypeList.First(i => i.ContactMechanismUsageTypeId == postalAddress.ContactMechanismUsageTypeId);
						if (usageType != null)
						{
							postalAddress.contactMechanismUsageType = usageType;
						}
					}
				}
				return result;
			}
		}
		#endregion
	}
}