using UnifiedLogin.DataAccess;
using UnifiedLogin.DataAccess.Helper;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.UnifiedLogin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using UnifiedLogin.SharedObjects.Constants;
using UnifiedLogin.SharedObjects.Landing.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
    /// <summary>
    /// Used to get product role/right information for the given user, product, role
    /// </summary>
    public class UserRoleRightRepository : BaseRepository, IUserRoleRightRepository
    {
        #region Private variables
        IRepository _repository;
        IProductInternalSettingRepository _productInternalSettingRepository;
        #endregion

        #region Constructor
        /// <summary>
        /// UserRoleRight base Constructor
        /// </summary>
        public UserRoleRightRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _productInternalSettingRepository = new ProductInternalSettingRepository();
        }

        /// <summary>
        /// Used when called with existing repository with transaction
        /// </summary>
        /// <param name="repository"></param>
        public UserRoleRightRepository(IRepository repository) : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _repository = repository;
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
        }

        public UserRoleRightRepository(IRepository repository, HttpMessageHandler messageHandler, DefaultUserClaim userClaims) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="userClaim"></param>
        public UserRoleRightRepository(IRepository repository, DefaultUserClaim userClaim) : base(repository)
        {
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
        }

        #endregion

        /// <summary>
        /// List of Roles by User Persona ID
        /// </summary>
        /// <param name="productId">Product ID</param>  
        /// <param name="userPersonaId">Optional Persona ID</param>   
        /// <param name="organizationPartyId">Optional Organization PartyId</param>
        /// <returns>List of roles assigned to Persona</returns>
        public List<SharedObjects.Product.UnifiedLogin.Role> ListRoleByPersona(int productId, long? userPersonaId = null, long? organizationPartyId = null)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"sp_ListRolesForProductsByPersonaId_{productId}_{userPersonaId}_{organizationPartyId}";

            List<SharedObjects.Product.UnifiedLogin.Role> getRolesForProductByPersona = rpCache.GetFromCache<List<SharedObjects.Product.UnifiedLogin.Role>>(cacheKey, 120, () =>
            {
                using (var repository = GetRepository())
                {
                    var procName = StoredProcNameConstants.SP_ListRolesForProductsByPersonaId;

                    dynamic param = new
                    {
                        ProductID = productId,
                        PersonaID = userPersonaId,
                        PartyId = organizationPartyId
                    };

                    List<SharedObjects.Product.UnifiedLogin.Role> roleList = new List<SharedObjects.Product.UnifiedLogin.Role>();
                    var result = repository.GetMany<dynamic>(procName, param);
                    if (result != null)
                    {
                        foreach (var item in result)
                        {
                            roleList.Add(new UnifiedLogin.SharedObjects.Product.UnifiedLogin.Role { RoleID = item.RoleId, Name = item.Role, PersonaId = item.PersonaId.ToString(), RoleNickName = item.RoleNickName });
                        }
                    }

                    return roleList;
                }
        });

            return getRolesForProductByPersona;
        }

        /// <summary>
        /// Get a single role id for a given persona and product id
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>Role assigned to Persona</returns>
        public long GetRoleIdByPersona(long userPersonaId, int productId)
        {
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListRolesForProductsByPersonaId;

                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    ProductID = productId
                };
                var userRole = repository.GetOne<dynamic>(procName, param);
                return (userRole != null ? userRole.RoleId : 0);
            }
        }

        /// <summary>
        /// Get list of role ids for a given persona and product id
        /// </summary>
        /// <param name="userPersonaId">Persona ID</param>   
        /// <param name="productId">Product ID</param>   
        /// <returns>Role assigned to Persona</returns>
        public List<long> GetRoleIdsByPersona(long userPersonaId, int productId)
        {
            List<long> roleids = new List<long>();
            using (var repository = GetRepository())
            {
                var procName = StoredProcNameConstants.SP_ListRolesForProductsByPersonaId;

                dynamic param = new
                {
                    PersonaID = userPersonaId,
                    ProductID = productId
                };
                var userRoles = repository.GetMany<dynamic>(procName, param);

                foreach (var id in userRoles)
                {
                    roleids.Add(Convert.ToInt64(id.RoleId));
                }
                return roleids;
            }
        }

        /// <summary>
        /// Insert Role to User
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>             
        /// <param name="roleId">User Role</param>   
        /// <param name="deleteRole">isDeleted</param>   
        /// <returns>List of Roles assigned to Persona</returns>
        public RepositoryResponse InsertAssignedRoleToUser(long userPersonaId, long roleId, int userId, bool deleteRole = false)
        {
            RepositoryResponse rr = new RepositoryResponse();
            
            dynamic param = new
            {
                PersonaID = userPersonaId,
                RoleID = roleId,
                IsDeleted = deleteRole,
                CreatedBy = userId,
                PersonaPrivilgeID = 0
            };

            if (_repository == null)
            {
                _repository = GetRepository();
                rr = _repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, param);
            }
            else
            {
                rr = _repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_LinkPersonaToRole, param);
            }

            return rr;
        }

        /// <summary>
        /// Get all roles and associated rights in master-detail hierarchy
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="productIdList"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public IList<UnifiedLoginRoleRights> GetPlatFormRoleRights(long partyId, IList<int> productIdList, int productId)
        {
            Dictionary<int, UnifiedLoginRoleRights> userRoles = new Dictionary<int, UnifiedLoginRoleRights>();

            if (productIdList.Count == 0)
            {
                throw new Exception("Missing company product id list");
            }

            var procName = StoredProcNameConstants.SP_ListRightsAssociatedWithRoles;

            using (var repository = GetRepository())
            {
                repository.GetManyWithSpliOn<UnifiedLoginRoleRights, UnifiedLoginRight, UnifiedLoginRoleRights>(
                    sql: procName,
                    map: (roles, rights) =>
                    {
                        if (!userRoles.ContainsKey(roles.RoleId))
                        {
                            userRoles.Add(roles.RoleId, roles);
                        }
                        UnifiedLoginRoleRights userRole = userRoles[roles.RoleId];
                        userRole.UserRights.Add(rights);
                        return userRole;
                    },
                    param: new
                    {
                        PartyId = partyId,
                        ProductId = productId,
                        TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
                    },
                      splitOn: "RoleId,RightId");
                return userRoles.Values.ToList<UnifiedLoginRoleRights>();
            }
        }

        /// <summary>
        /// Get all roles and associated rights in master-detail hierarchy
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="productIdList"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        public IList<UserRoleRights> GetAllRoleRights(long partyId, IList<int> productIdList, int productId)
        {
            Dictionary<int, UserRoleRights> userRoles = new Dictionary<int, UserRoleRights>();

            if (productIdList.Count == 0)
            {
                throw new Exception("Missing company product id list");
            }

            var procName = StoredProcNameConstants.SP_ListRightsAssociatedWithRoles;

            using (var repository = GetRepository())
            {
                repository.GetManyWithSpliOn<UserRoleRights, Right, UserRoleRights>(
                    sql: procName,
                    map: (roles, rights) =>
                    {
                        if (!userRoles.ContainsKey(roles.RoleId))
                        {
                            userRoles.Add(roles.RoleId, roles);
                        }
                        UserRoleRights userRole = userRoles[roles.RoleId];
                        userRole.UserRights.Add(rights);
                        return userRole;
                    },
                    param: new
                    {
                        PartyId = partyId,
                        ProductId = productId,
                        TargetProductId = TableValueParamHelper.ConvertToTableValuedParameter(productIdList, "enterprise.productidtype")
                    },
                      splitOn: "RoleId,RightId");
                return userRoles.Values.ToList<UserRoleRights>();
            }
        }

        /// <summary>
		/// Get the list of persist rights that should be carry forwarded
		/// </summary>
		/// <returns>Persist rights list</returns>
		public IList<Right> GetPersistRights()
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"getPersistRights";

            return rpCache.GetFromCache<IList<Right>>(cacheKey, 180, () =>
            {
                using (var repository = GetRepository())
                {
                    return repository.GetMany<Right>(StoredProcNameConstants.SP_GetPersistRights, null).ToList();
                }
            });
        }

        /// <summary>
		/// Get Rights for ad group by persona
		/// </summary>
		/// <returns>list of ad group rights for the persona</returns>
		public IList<Right> GetADGroupRightsByPersonaId(long userPersonaId)
        {
            RPObjectCache rpCache = new RPObjectCache();
            var cacheKey = $"getADGroupRights_{userPersonaId}";
            dynamic param = new
            {
                PersonaId = userPersonaId
            };

            return rpCache.GetFromCache<IList<Right>>(cacheKey, 180, () =>
            {
                using (var repository = GetRepository())
                {
                    return repository.GetMany<Right>(StoredProcNameConstants.SP_GetADGroupRightsByPersona, param);
                }
            });
        }

        #region Private Methods
        private string getRoleRightsSchemaName()
        {
            RPObjectCache rpcache = new RPObjectCache();

            var cacheKey = "getRoleRightsSchemaName_" + (int)ProductEnum.UnifiedPlatform;
            string schemaName = rpcache.GetFromCache<string>(cacheKey, 60, () =>
            {
                var productInternalSettingList = _productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
                return productInternalSettingList.FirstOrDefault(s => s.Name.Equals("RolesRightsSchemaName", StringComparison.OrdinalIgnoreCase))?.Value;
            });

            return schemaName;

        }
        #endregion
    }
}
