using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage Contact Mechanism Usage Types repository calls
	/// </summary>
	public class ManageContactMechanismUsageType : IManageContactMechanismUsageType
	{
		#region Private Variables
		IContactMechanismUsageTypeRepository _contactMechanismUsageTypeRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageContactMechanismUsageType Constructor
		/// </summary>
		/// <param name="contactMechanismUsageTypeRepository">Contact Mechanism Usage Type Repository</param>
		public ManageContactMechanismUsageType(IContactMechanismUsageTypeRepository contactMechanismUsageTypeRepository)
		{
			_contactMechanismUsageTypeRepository = contactMechanismUsageTypeRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageContactMechanismUsageType Controller class
		/// </summary>
		/// 
		public ManageContactMechanismUsageType()
		{
			_contactMechanismUsageTypeRepository = new ContactMechanismUsageTypeRepository();
		}
		#endregion

		#region Public ManageContactMechanismUsageType methods
		/// <summary>
		/// Get a list of Contact Mechanism Usage Types
		/// </summary>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of Contact Mechanism Usage Types</returns>
		public IList<ContactMechanismUsageType> ListContactMechanismUsageType(string ContactMechanismUsageTypeName)
		{
			return _contactMechanismUsageTypeRepository.ListContactMechanismUsageType(ContactMechanismUsageTypeName);
		}
		#endregion
	}
}