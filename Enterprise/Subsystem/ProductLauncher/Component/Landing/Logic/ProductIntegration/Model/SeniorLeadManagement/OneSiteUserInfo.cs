using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.ProductIntegration.Model.SeniorLeadManagement
{
    /// <summary>
    /// Model for One site user info
    /// </summary>
    public sealed class OneSiteUserInfo
    {
        #region "Properties"

        public int UserId { get; set; }
        public string FirstName { get; set; }
        public int LastName { get; set; }
        public List<string> Properties { get; set; }

        #endregion
    }
}
