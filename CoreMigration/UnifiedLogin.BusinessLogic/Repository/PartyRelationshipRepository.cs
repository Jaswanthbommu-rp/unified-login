using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Party Relationship Repository
	/// </summary>
	public class PartyRelationshipRepository : BaseRepository, IPartyRelationshipRepository
    {
        private IRoleTypeRepository _roleTypeRepository;
        private IRelationshipTypeRepository _relationshipTypeRepository;

		#region Constructor
		/// <summary>
		/// Party Relationship Repository base Constructor
		/// </summary>
		public PartyRelationshipRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _roleTypeRepository = new RoleTypeRepository();
            _relationshipTypeRepository = new RelationshipTypeRepository();
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="repository"></param>
		public PartyRelationshipRepository(IRepository repository) : base(repository)
		{
            _roleTypeRepository = new RoleTypeRepository(repository);
            _relationshipTypeRepository = new RelationshipTypeRepository(repository);
        }
		#endregion

		#region public PartyRelationshipRepository methods

		/// <summary>
		/// Get a Party (Person) Relationship within a Linked Organization
		/// </summary>
		/// <param name="realPageIdFrom">Person unique identifier</param>
		/// <param name="realPageIdTo">Organization unique identifier</param>
		/// <param name="roleTypeNameFrom">Person Role Type name in the Relationship (Optional)</param>
		/// <param name="roleTypeNameTo">Organization Role Type name in the Relationship (Optional)</param>
		/// <param name="relationshipTypeName">Parties Relationhip type name (Optional)</param>
		/// <returns>PartyRelationship object</returns>
		public PartyRelationship GetPartyRelationship(Guid realPageIdFrom, Guid realPageIdTo, string roleTypeNameFrom, string roleTypeNameTo, string relationshipTypeName)
        {
            RoleType roleType = new RoleType();
            RelationshipType relationshipType = new RelationshipType();
            
            PartyRelationship partyRelationshipResult = new PartyRelationship();

            using (var repository = GetRepository())
            {
                dynamic param = null;
                if ((string.IsNullOrEmpty(roleTypeNameFrom)) && (string.IsNullOrEmpty(relationshipTypeName)))
                {
                    param = new
                    {
                        RealPageIdFrom = realPageIdFrom,
                        RealPageIdTo = realPageIdTo
                    };
                }
                else if (string.IsNullOrEmpty(roleTypeNameFrom))
                {
                    param = new
                    {
                        RealPageIdFrom = realPageIdFrom,
                        RealPageIdTo = realPageIdTo,
                        RelationshipTypeName = relationshipTypeName //fix
                    };
                }
                else if (string.IsNullOrEmpty(relationshipTypeName))
                {
                    param = new
                    {
                        RealPageIdFrom = realPageIdFrom,
                        RealPageIdTo = realPageIdTo,
                        RelationshipTypeName = relationshipTypeName
                    };
                }
                else
                {
                    param = new
                    {
                        RealPageIdFrom = realPageIdFrom,
                        RealPageIdTo = realPageIdTo,
                        RoleTypeName = roleTypeNameFrom,
                        RelationshipTypeName = relationshipTypeName
                    };
                }

                partyRelationshipResult = repository.GetOne<PartyRelationship>(StoredProcNameConstants.SP_GetPartyRelationshipByRealPageId, param);
            }

            IList<RelationshipType> relationshipTypeList = _relationshipTypeRepository.GetRelationshipType(relationshipTypeName);
            if (partyRelationshipResult != null)
            {
                //RoleType From
                IList<RoleType> roleTypeList = _roleTypeRepository.GetRoleType(roleTypeName: roleTypeNameFrom, partyId: null);
                roleType = roleTypeList.First(i => i.PartyRoleTypeId == partyRelationshipResult.RoleTypeIdFrom);
                if (roleType != null)
                {
                    partyRelationshipResult.RoleTypeFrom = roleType;
                }

                //RoleType To
                roleTypeList = _roleTypeRepository.GetRoleType(roleTypeName: roleTypeNameTo, partyId: null);
                roleType = roleTypeList.First(i => i.PartyRoleTypeId == partyRelationshipResult.RoleTypeIdTo);
                if (roleType != null)
                {
                    partyRelationshipResult.RoleTypeTo = roleType;
                }

                //RelationshipType
                relationshipType = relationshipTypeList.First(i => i.RelationshipTypeId == partyRelationshipResult.PartyRelationshipTypeId);
                if (relationshipType != null)
                {
                    partyRelationshipResult.PartyRelationshipType = relationshipType;
                }
            }
            return partyRelationshipResult;

        }

        /// <summary>
		/// Link a Person to an Organization
		/// </summary>
		/// <param name="RealPageIdFrom">Person unique identifier</param>
		/// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkPersonToOrganization(Guid RealPageIdFrom, PartyRelationship partyRelationship)
		{
			dynamic param = new
			{
				PersonRealPageId = RealPageIdFrom,
				OrganizationRealPageId = partyRelationship.RealPageIdTo,
				RoleTypeIdFrom = partyRelationship.RoleTypeIdFrom,
				RoleTypeIdTo = partyRelationship.RoleTypeIdTo
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonToOrganization, param);
				return result;
			}
		}

		/// <summary>
		/// Link an Organization to an Organization
		/// </summary>
		/// <param name="RealPageIdFrom">From Organization unique identifier</param>
		/// <param name="partyRelationship">To Party Unique Identifier and From/To Relationship RoleType</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse LinkOrganizationToOrganization(Guid RealPageIdFrom, PartyRelationship partyRelationship)
		{
			dynamic param = new
			{
				OrganizationRealPageIdFrom = RealPageIdFrom,
				OrganizationRealPageIdTo = partyRelationship.RealPageIdTo,
				RoleTypeIdFrom = partyRelationship.RoleTypeIdFrom,
				RoleTypeIdTo = partyRelationship.RoleTypeIdTo
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkOrganizationToOrganization, param);
				return result;
			}
		}
		#endregion
	}
}