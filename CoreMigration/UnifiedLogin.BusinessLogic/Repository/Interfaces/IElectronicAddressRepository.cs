using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for ElectronicAddressRepository.cs
	/// </summary>
	public interface IElectronicAddressRepository
	{
		/// <summary>
		/// Link an electronic address for a person
		/// </summary>
		/// <param name="electronicAddress">Person's Electronic Address parameter values</param>
		/// <returns>Repository response object</returns>
		RepositoryResponse CreateElectronicAddress(IElectronicAddress electronicAddress);

		/// <summary>
		/// List electronic address details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Electronic Address details for a person</returns>
		IList<ElectronicAddress> ListElectronicAddressForPerson(Guid realPageId, string ContactMechanismUsageTypeName);

		/// <summary>
		/// List electronic address details for a person with login name and organization
		/// </summary>
		/// <param name="loginName">User lohin name</param>
		/// <param name="orgPartyId">Organization party id</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Electronic Address details for a person</returns>
		IList<ElectronicAddress> ListElectronicAddressForPerson(string loginName, long orgPartyId, string ContactMechanismUsageTypeName = "");
	}
}