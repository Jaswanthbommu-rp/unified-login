using System.Collections.Generic;
using System.Net.Http;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manage Relationship Type repository calls
    /// </summary>
    public class ManageRelationshipType : IManageRelationshipType
    {
        #region Private Variables
        IRelationshipTypeRepository _relationshipTypeRepository;
        private DefaultUserClaim _userClaims;
        private IManagePersona _managePersona;
        #endregion

        #region Constructors
        /// <summary>
        /// Unit Test Constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaim"></param>
        /// <param name="messageHandler"></param>
        public ManageRelationshipType(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            _managePersona = new ManagePersona(repository, userClaim, messageHandler);
            _relationshipTypeRepository = new RelationshipTypeRepository(repository);
            _userClaims = userClaim;
        }

        /// <summary>
        /// Create a basic instance of the ManageOrganization Controller class
        /// </summary>
        public ManageRelationshipType(DefaultUserClaim userClaim)
        {
            _managePersona = new ManagePersona(userClaim);
            _relationshipTypeRepository = new RelationshipTypeRepository();
            _userClaims = userClaim;
        }
        #endregion

        /// <summary>
        /// Get RelationshipType
        /// </summary>
        /// <param name="relationshipTypeName">Relationship Type Name</param>
        /// <returns>List of RelationshipType object</returns>
        public IList<RelationshipType> GetRelationshipType(string relationshipTypeName)
        {
            return _relationshipTypeRepository.GetRelationshipType(relationshipTypeName);
        }
        public IList<UserRelationShipType> GetUserRelationShipTypes()
        {
            var persona = _managePersona.GetPersona(_userClaims.PersonaId);
            if (persona == null)
            {
                return null;
            }

            List<UserRelationShipType> userRelationShipTypes = (List<UserRelationShipType>)_relationshipTypeRepository.GetUserRelationShipTypes(partyId: _userClaims.OrganizationPartyId);
            if (!_userClaims.IsRPEmployee && persona.UserTypeId == (int)UserRoleType.ExternalUser)
            {
                userRelationShipTypes.RemoveAll(x => x.PartyRoleTypeId == 402);
            }
            if (_userClaims.IsRPEmployee && persona.UserTypeId == (int)UserRoleType.ExternalUser) 
            {
                userRelationShipTypes.RemoveAll(x => x.PartyRoleTypeId == 403);
            }
            return userRelationShipTypes;
        }
    }
}