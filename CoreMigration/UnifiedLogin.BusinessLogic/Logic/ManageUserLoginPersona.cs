using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;

namespace UnifiedLogin.BusinessLogic.Logic
{
	public class ManageUserLoginPersona : IManageUserLoginPersona
	{
		#region Private Variables
		IUserLoginPersonaRepository _userLoginPersonaRepository;
		private DefaultUserClaim _defaultUserClaim;
		#endregion

		#region Constructors
		/// <summary>
		/// Used for dependency injection
		/// </summary>
		/// <param name="userLoginPersonaRepository"></param>
		/// <param name="userClaim"></param>
		public ManageUserLoginPersona(IUserLoginPersonaRepository userLoginPersonaRepository, DefaultUserClaim userClaim)
		{
			_userLoginPersonaRepository = userLoginPersonaRepository;
			_defaultUserClaim = userClaim;
		}

		/// <summary>
		/// Create a basic instance of the ManageUserLoginPersona
		/// </summary>
		public ManageUserLoginPersona()
		{
			_userLoginPersonaRepository = new UserLoginPersonaRepository();
		}

		/// <summary>
		/// Default Constructor
		/// </summary>
		/// <param name="userClaim"></param>
		public ManageUserLoginPersona(DefaultUserClaim userClaim)
		{
			_userLoginPersonaRepository = new UserLoginPersonaRepository();
			_defaultUserClaim = userClaim;
		}
		#endregion

		#region Public ManageUserLoginPersona methods
		/// <summary>
		/// List one or more UserLogin Persona records
		/// </summary>
		/// <param name="userLoginPersonaId">Optional userLoginPersonaId</param>
		/// <param name="userLoginId">Optional UserLoginId</param>
		/// <param name="organizationPartyId">Optional Organization PartyId</param>
		/// <returns>List of UserLoginPersona</returns>
		public IList<UserLoginPersona> ListUserLoginPersona(long? userLoginPersonaId, long? userLoginId, long? organizationPartyId)
		{
			IList<UserLoginPersona> userLoginPersonaList = new List<UserLoginPersona>();

			userLoginPersonaList = _userLoginPersonaRepository.ListUserLoginPersona(userLoginPersonaId, userLoginId, organizationPartyId);

			return userLoginPersonaList;
		}
		#endregion
	}
}
