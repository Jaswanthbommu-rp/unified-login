using System;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage StreetAddress repository calls
	/// </summary>
	public class ManageStreetAddress : IManageStreetAddress
	{
		#region Private Variables
		IStreetAddressRepository _streetAddressRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageStreetAddress Constructor
		/// </summary>
		/// <param name="streetAddressRepository">Contact Mechanism Repository</param>
		public ManageStreetAddress(IStreetAddressRepository streetAddressRepository)
		{
			_streetAddressRepository = streetAddressRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageStreetAddress Controller class
		/// </summary>
		/// 
		public ManageStreetAddress()
		{
			_streetAddressRepository = new StreetAddressRepository();
		}
		#endregion

		#region Public StreetAddressMechanism methods
		/// <summary>
		/// Create the StreetAddress for a Person
		/// </summary>
		/// <param name="streetAddress">StreetAddress Object.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateStreetAddress(IStreetAddress streetAddress)
		{
			if (streetAddress == null)
			{
				throw new ArgumentNullException(nameof(streetAddress), "Null StreetAddress.");
			}

			return _streetAddressRepository.CreateStreetAddress(streetAddress);
		}
		#endregion
	}
}