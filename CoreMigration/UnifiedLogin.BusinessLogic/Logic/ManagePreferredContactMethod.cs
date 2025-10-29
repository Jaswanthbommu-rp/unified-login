using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage Preferred Contact Methods repository calls
	/// </summary>
	public class ManagePreferredContactMethod : IManagePreferredContactMethod
	{
		#region Private Variables
		IPreferredContactMethodRepository _preferredContactMethodRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManagePreferredContactMethod Constructor
		/// </summary>
		/// <param name="preferredContactMethodRepository">Preferred Contact Method Repository</param>
		public ManagePreferredContactMethod(IPreferredContactMethodRepository preferredContactMethodRepository)
		{
			_preferredContactMethodRepository = preferredContactMethodRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManagePreferredContactMethod Controller class
		/// </summary>
		/// 
		public ManagePreferredContactMethod()
		{
			_preferredContactMethodRepository = new PreferredContactMethodRepository();
		}
		#endregion

		#region Public ManagePreferredContactMethod methods
		/// <summary>
		/// Get a list of Preferred Contact Methods
		/// </summary>
		/// <returns>List of Preferred Contact Methods</returns>
		public IList<PreferredContactMethod> ListPreferredContactMethod()
		{
			return _preferredContactMethodRepository.ListPreferredContactMethod();
		}
		#endregion
	}
}