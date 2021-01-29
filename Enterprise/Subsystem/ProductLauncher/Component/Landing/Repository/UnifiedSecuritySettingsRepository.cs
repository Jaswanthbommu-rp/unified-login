using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using RP.Enterprise.Foundation.DataAccess.Component;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
    public class UnifiedSecuritySettingsRepository : BaseRepository
    {
        #region Constructor
        /// <summary>
        /// Security Settings base Constructor
        /// </summary>
        public UnifiedSecuritySettingsRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        public UnifiedSecuritySettingsRepository(IRepository repository) : base(repository)
        {

        }
        #endregion
    }
}
