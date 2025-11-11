using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// RoleType Repository
	/// </summary>
	public class RoleTypeRepository : BaseRepository, IRoleTypeRepository
    {
        #region Constructor
        /// <summary>
        /// RoleType Repository base Constructor
        /// </summary>
        public RoleTypeRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        public RoleTypeRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        #region public RoleTypeRepository methods
        /// <summary>
        /// Get RoleType
        /// </summary>
        /// <param name="roleTypeName">Role Type Name</param>
        /// <param name="partyId">The organization to filter the role type if used</param>
        /// <returns>List RoleType object</returns>
        public IList<RoleType> GetRoleType(string roleTypeName, long? partyId)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"sp_ListRoleType_{roleTypeName}_{partyId}";

            IList<RoleType> getRoleType = rpCache.GetFromCache<List<RoleType>>(cacheKey, 180, () =>
            {
                using (var repository = GetRepository())
                {
                    dynamic param = null;
                    if (!string.IsNullOrEmpty(roleTypeName))
                    {
                        param = new
                        {
                            RoleTypeName = roleTypeName,
                            OrganizationPartyID = partyId
                        };
                    }

                    return repository.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleType, param);
                }
            });
            return getRoleType;
        }

        /// <summary>
        /// Get RoleType
        /// </summary>
        /// <param name="roleTypeId">Role Type Id</param>
        /// <param name="partyId">The organization to filter the role type if used</param>
        /// <returns>List RoleType object</returns>
        public IList<RoleType> GetRoleTypeDependency(long? roleTypeId, long? partyId)
        {
            using (var repository = GetRepository())
            {
                dynamic param = null;

                param = new
                {
                    PartyId = partyId,
                    ParentRoleTypeID = roleTypeId

                };


                return repository.GetMany<RoleType>(StoredProcNameConstants.SP_ListRoleTypeDependency, param);
            }
        }
        #endregion
    }
}