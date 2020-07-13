using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing.Security;
using System.Collections.Generic;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using System.Linq;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Security
{
    /// <summary>
    /// 
    /// </summary>
    public class PersonaRightRepository : BaseRepository, IPersonaRightRepository
    {

        #region Ctor

        /// <summary>
        /// User base Constructor
        /// </summary>
        public PersonaRightRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
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

            IProductInternalSettingRepository productInternalSettingRepository = new ProductInternalSettingRepository();
            var productInternalSettingList = productInternalSettingRepository.GetProductInternalSettings((int)ProductEnum.UnifiedPlatform);
            string schemaName =  productInternalSettingList.FirstOrDefault(s => s.Name.Equals("RolesRightsSchemaName", StringComparison.OrdinalIgnoreCase))?.Value;
            var procName = schemaName?.Length > 0 ? $"{schemaName}.ListPersonaRightsAndActionsByRoute" : StoredProcNameConstants.SP_ListPersonaRightsAndActionsByRoute;

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