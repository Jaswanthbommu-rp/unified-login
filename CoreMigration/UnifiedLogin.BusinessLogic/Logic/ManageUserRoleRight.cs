using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Used to get role/right information for the given persona, product id
	/// </summary>
	public class ManageUserRoleRight : IManageUserRoleRight
	{
		#region Private Variables
		private IUserRoleRightRepository _userRoleRightRepository;
		#endregion

		/// <summary>
		/// Default constructor
		/// </summary>
		public ManageUserRoleRight()
		{
			_userRoleRightRepository = new UserRoleRightRepository();
		}

		/// <summary>
		/// DI constructor
		/// </summary>
		/// <param name="userRoleRightRepository"></param>
		public ManageUserRoleRight(IUserRoleRightRepository userRoleRightRepository)
		{
			_userRoleRightRepository = userRoleRightRepository;
		}

        /// <summary>
        /// DI constructor
        /// </summary>
        /// <param name="repository"></param>
        public ManageUserRoleRight(IRepository repository)
        {
            _userRoleRightRepository = new UserRoleRightRepository(repository);
        }

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="repository"></param>
		/// <param name="userClaim"></param>
		public ManageUserRoleRight(IRepository repository, DefaultUserClaim userClaim)
		{
			_userRoleRightRepository = new UserRoleRightRepository(repository, userClaim);
		}

		/// <summary>
		/// Used to get a list of roles assigned to the given persona for the given product id
		/// </summary>
		/// <param name="productId">The product id</param>
		/// <param name="userPersonaId">The persona id</param>
		/// <param name="organizationPartyId">Optional Organization PartyId</param>
		/// <returns>A list of roles</returns>
		public IList<UL.Role> GetAssignedRoleForPersona(ProductEnum productId, long? userPersonaId = null, long? organizationPartyId = null)
		{
			List<UL.Role> propRole = _userRoleRightRepository.ListRoleByPersona((int)productId, userPersonaId, organizationPartyId);
			return propRole;
		}
	}
}