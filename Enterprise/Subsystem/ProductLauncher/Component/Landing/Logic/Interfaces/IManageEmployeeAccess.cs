using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface IManageEmployeeAccess
    {
        /// <summary>
        /// Returns Companies (Companies in GB)
        /// </summary>
        ListResponse GetCompanies(long editorPersonaId, string filter);

        /// <summary>
        /// Returns Users in Unified Login
        /// </summary>
        ListResponse GetUsers(long editorPersonaId, string filter);

    }
}