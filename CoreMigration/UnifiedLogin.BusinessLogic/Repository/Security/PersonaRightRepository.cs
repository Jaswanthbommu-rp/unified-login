using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing.Security;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using System.Linq;
using System;
using UnifiedLogin.DataAccess;

namespace UnifiedLogin.BusinessLogic.Repository.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonaRightRepository : BaseRepository, IPersonaRightRepository
    {
        IProductInternalSettingRepository _productInternalSettingRepository;
        #region Ctor

        /// <summary>
        /// User base Constructor
        /// </summary>
        public PersonaRightRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
            _productInternalSettingRepository = new ProductInternalSettingRepository();
        }

        public PersonaRightRepository (IRepository repository) : base(repository)
        {
            _productInternalSettingRepository = new ProductInternalSettingRepository(repository);
        }
        #endregion

        /// <summary>
        /// List Rights By PersonaId for given route
        /// </summary>
        /// <param name="personaId"></param>
        /// <param name="routeId"></param>
        /// <returns></returns>
        public IEnumerable<PersonaActionRight> ListRightsAndActionsByPersonaId(long personaId, string routeId)
        {
            RPObjectCache rpcache = new RPObjectCache();
            var cacheKey = $"listRightsAndActionsByPersonaId_{personaId}_{routeId}";
           
            var procName = StoredProcNameConstants.SP_ListPersonaRightsAndActionsByRoute;

            IEnumerable<PersonaActionRight> personaRights = rpcache.GetFromCache<IEnumerable<PersonaActionRight>>(cacheKey, 120, () =>
            {
                // load from api
                dynamic param = new
                {
                    personaId,
                    routeId
                };

                using (var repository = GetRepository())
                {
                    var result = repository.GetMany<PersonaActionRight>(procName, param);
                    return result;
                }
            });
            return personaRights;
        }
    }
}