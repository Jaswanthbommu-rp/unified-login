using System.Collections.Generic;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
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