using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
	public class RelationshipTypeRepository : BaseRepository, IRelationshipTypeRepository
	{
		#region Constructor
		/// <summary>
		/// RelationshipType Repository base Constructor
		/// </summary>
		public RelationshipTypeRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}

		/// <summary>
		/// Unit test constructor
		/// </summary>
		/// <param name="repository"></param>
        public RelationshipTypeRepository(IRepository repository) : base(repository)
        {
        }

		#endregion

		#region public RelationshipTypeRepository methods
		/// <summary>
		/// Get RelationshipType
		/// </summary>
		/// <param name="relationshipTypeName">Relationship Type Name</param>
		/// <returns>List RelationshipType object</returns>
		public IList<RelationshipType> GetRelationshipType(string relationshipTypeName)
		{
			using (var repository = GetRepository())
			{
				dynamic param = null;
				if (!string.IsNullOrEmpty(relationshipTypeName))
				{
					param = new
					{
						RelationshipTypeName = relationshipTypeName
					};
				}

				return repository.GetMany<RelationshipType>(StoredProcNameConstants.SP_ListRelationshipType, param);
			}
		}
        /// <summary>
        /// Get UserRelationshipType
        /// </summary>
        /// <param name="partyId">PartyId of user</param>
        /// <returns>List UserRelationshipTypes object</returns>
        public IList<UserRelationShipType> GetUserRelationShipTypes(long partyId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = null;
                if (partyId != 0)
                {
                    param = new
                    {
                        PartyId = partyId
                    };
                }

                return repository.GetMany<UserRelationShipType>(StoredProcNameConstants.SP_ListUserRelationshipTypes, param);
            }
        }
        #endregion
    }
}
