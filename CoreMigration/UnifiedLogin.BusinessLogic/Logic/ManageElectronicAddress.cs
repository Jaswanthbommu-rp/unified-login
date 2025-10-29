using System;
using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage Electronic Address repository calls
	/// </summary>
	public class ManageElectronicAddress : IManageElectronicAddress
	{
		#region Private Variables
		IElectronicAddressRepository _electronicAddressRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageElectronicAddress Constructor
		/// </summary>
		/// <param name="electronicAddressRepository">Electronic Address Repository</param>
		public ManageElectronicAddress(IElectronicAddressRepository electronicAddressRepository)
		{
			_electronicAddressRepository = electronicAddressRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageElectronicAddress Controller class
		/// </summary>
		/// 
		public ManageElectronicAddress()
		{
			_electronicAddressRepository = new ElectronicAddressRepository();
		}
		#endregion

		#region Public ManageElectronicAddress methods
		/// <summary>
		/// Link an electronic address to a person
		/// </summary>
		/// <param name="electronicAddress">Person's Electronic Address parameter values</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateElectronicAddress(IElectronicAddress electronicAddress)
		{
			if (electronicAddress == null)
			{
				throw new ArgumentNullException(nameof(electronicAddress), "Null ElectronicAddress.");
			}

			return _electronicAddressRepository.CreateElectronicAddress(electronicAddress);
		}

		/// <summary>
		/// List electronic address details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Electronic Address details for a person</returns>
		public IList<ElectronicAddress> ListElectronicAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName)
		{
			if (realPageId == Guid.Empty)
			{
				throw new Exception("Invalid parameter realPageId.");
			}

			return _electronicAddressRepository.ListElectronicAddressForPerson(realPageId, ContactMechanismUsageTypeName);
		}

		/// <summary>
		/// List electronic address details for a person with login name and organization
		/// </summary>
		/// <param name="loginName">User lohin name</param>
		/// <param name="orgPartyId">Organization party id</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Electronic Address details for a person</returns>
		public IList<ElectronicAddress> ListElectronicAddressForPerson(string loginName, long orgPartyId, string ContactMechanismUsageTypeName = "")
		{
			if (string.IsNullOrEmpty(loginName))
			{
				throw new Exception("Invalid parameter user login name.");
			}

			return _electronicAddressRepository.ListElectronicAddressForPerson(loginName,orgPartyId, ContactMechanismUsageTypeName);
		}
		#endregion
	}
}