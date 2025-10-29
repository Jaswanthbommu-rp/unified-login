using System;
using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage Postal Address repository calls
	/// </summary>
	public class ManagePostalAddress : IManagePostalAddress
	{
		#region Private Variables
		IPostalAddressRepository _postalAddressRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManagePostalAddress Constructor
		/// </summary>
		/// <param name="postalAddressRepository">Postal Address Repository</param>
		public ManagePostalAddress(IPostalAddressRepository postalAddressRepository)
		{
			_postalAddressRepository = postalAddressRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManagePostalAddress Controller class
		/// </summary>
		/// 
		public ManagePostalAddress()
		{
			_postalAddressRepository = new PostalAddressRepository();
		}
		#endregion

		#region Public ManagePostalAddress methods
		/// <summary>
		/// List postal address details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Postal Address details for a person</returns>
		public IList<PostalAddress> ListPostalAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName)
		{
			if (realPageId == Guid.Empty)
			{
				throw new Exception("Invalid parameter realPageId.");
			}

			return _postalAddressRepository.ListPostalAddressForPerson(realPageId, ContactMechanismUsageTypeName);
		}
		#endregion
	}
}