using System;
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
        #region Constants
        /// <summary>
        /// Party Role Type ID filtered for non-RealPage employee external users
        /// </summary>
        private const int PartyRoleTypeId_ExternalUserNonRP = 402;

        /// <summary>
        /// Party Role Type ID filtered for RealPage employee external users
        /// </summary>
        private const int PartyRoleTypeId_ExternalUserRP = 403;
        #endregion

        #region Private Variables
        private readonly IRelationshipTypeRepository _relationshipTypeRepository;
        private readonly DefaultUserClaim _userClaims;
        private readonly IManagePersona _managePersona;
        #endregion

        #region Constructors
        /// <summary>
        /// Unit Test Constructor with full dependency injection
        /// </summary>
        /// <param name="relationshipTypeRepository">Relationship type repository</param>
        /// <param name="managePersona">Persona management service</param>
        /// <param name="userClaim">User claims</param>
        public ManageRelationshipType(
            IRelationshipTypeRepository relationshipTypeRepository,
            IManagePersona managePersona,
            DefaultUserClaim userClaim)
        {
            _relationshipTypeRepository = relationshipTypeRepository ?? throw new ArgumentNullException(nameof(relationshipTypeRepository));
            _managePersona = managePersona ?? throw new ArgumentNullException(nameof(managePersona));
            _userClaims = userClaim ?? throw new ArgumentNullException(nameof(userClaim));
        }

        /// <summary>
        /// Constructor with repository and message handler (legacy support)
        /// </summary>
        /// <param name="repository">Repository</param>
        /// <param name="userClaim">User claims</param>
        /// <param name="messageHandler">HTTP message handler</param>
        public ManageRelationshipType(IRepository repository, DefaultUserClaim userClaim, HttpMessageHandler messageHandler)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            if (userClaim == null) throw new ArgumentNullException(nameof(userClaim));
            if (messageHandler == null) throw new ArgumentNullException(nameof(messageHandler));

            _managePersona = new ManagePersona(repository, userClaim, messageHandler);
            _relationshipTypeRepository = new RelationshipTypeRepository(repository);
            _userClaims = userClaim;
        }

        /// <summary>
        /// Create a basic instance of the ManageRelationshipType class (legacy support)
        /// </summary>
        public ManageRelationshipType(DefaultUserClaim userClaim)
        {
            if (userClaim == null) throw new ArgumentNullException(nameof(userClaim));

            _managePersona = new ManagePersona(userClaim);
            _relationshipTypeRepository = new RelationshipTypeRepository();
            _userClaims = userClaim;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get RelationshipType
        /// </summary>
        /// <param name="relationshipTypeName">Relationship Type Name</param>
        /// <returns>List of RelationshipType object</returns>
        /// <exception cref="ArgumentException">Thrown when relationshipTypeName is null or empty</exception>
        public IList<RelationshipType> GetRelationshipType(string relationshipTypeName)
        {
            if (string.IsNullOrWhiteSpace(relationshipTypeName))
            {
                throw new ArgumentException("Relationship type name cannot be null or empty.", nameof(relationshipTypeName));
            }

            return _relationshipTypeRepository.GetRelationshipType(relationshipTypeName);
        }

        /// <summary>
        /// Get user relationship types with role-based filtering
        /// </summary>
        /// <returns>Filtered list of UserRelationShipType or empty list if persona not found</returns>
        public IList<UserRelationShipType> GetUserRelationShipTypes()
        {
            var persona = GetPersonaForCurrentUser();
            if (persona == null)
            {
                return new List<UserRelationShipType>();
            }

            var userRelationShipTypes = GetUserRelationShipTypesFromRepository();
            
            return ApplyUserRelationshipTypeFilters(userRelationShipTypes, persona);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get persona for current user
        /// </summary>
        /// <returns>Persona or null if not found</returns>
        private Persona GetPersonaForCurrentUser()
        {
            return _managePersona.GetPersona(_userClaims.PersonaId);
        }

        /// <summary>
        /// Get user relationship types from repository
        /// </summary>
        /// <returns>List of UserRelationShipType</returns>
        private List<UserRelationShipType> GetUserRelationShipTypesFromRepository()
        {
            return (List<UserRelationShipType>)_relationshipTypeRepository.GetUserRelationShipTypes(
                partyId: _userClaims.OrganizationPartyId);
        }

        /// <summary>
        /// Apply filtering rules to user relationship types based on user role and employee status
        /// </summary>
        /// <param name="userRelationShipTypes">List of relationship types to filter</param>
        /// <param name="persona">Current user's persona</param>
        /// <returns>Filtered list of UserRelationShipType</returns>
        private IList<UserRelationShipType> ApplyUserRelationshipTypeFilters(
            List<UserRelationShipType> userRelationShipTypes,
            Persona persona)
        {
            if (userRelationShipTypes == null || userRelationShipTypes.Count == 0)
            {
                return userRelationShipTypes ?? new List<UserRelationShipType>();
            }

            // Rule 1: Non-RealPage employee + External User
            // External users from non-RP organizations cannot see PartyRoleTypeId 402
            if (ShouldFilterNonRPEmployeeExternalUser(persona))
            {
                userRelationShipTypes.RemoveAll(x => x.PartyRoleTypeId == PartyRoleTypeId_ExternalUserNonRP);
            }

            // Rule 2: RealPage employee + External User
            // External users from RealPage cannot see PartyRoleTypeId 403
            if (ShouldFilterRPEmployeeExternalUser(persona))
            {
                userRelationShipTypes.RemoveAll(x => x.PartyRoleTypeId == PartyRoleTypeId_ExternalUserRP);
            }

            return userRelationShipTypes;
        }

        /// <summary>
        /// Determine if filtering should be applied for non-RealPage employee external users
        /// </summary>
        /// <param name="persona">User's persona</param>
        /// <returns>True if filtering should be applied</returns>
        private bool ShouldFilterNonRPEmployeeExternalUser(Persona persona)
        {
            return !_userClaims.IsRPEmployee && 
                   persona.UserTypeId == (int)UserRoleType.ExternalUser;
        }

        /// <summary>
        /// Determine if filtering should be applied for RealPage employee external users
        /// </summary>
        /// <param name="persona">User's persona</param>
        /// <returns>True if filtering should be applied</returns>
        private bool ShouldFilterRPEmployeeExternalUser(Persona persona)
        {
            return _userClaims.IsRPEmployee && 
                   persona.UserTypeId == (int)UserRoleType.ExternalUser;
        }
        #endregion
    }
}