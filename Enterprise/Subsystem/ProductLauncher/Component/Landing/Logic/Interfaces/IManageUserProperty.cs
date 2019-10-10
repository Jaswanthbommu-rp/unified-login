using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
    public interface IManageUserProperty
    {
        /// <summary>
        /// Update User Detail and Products
        /// </summary>
        /// <param name="userPersonaId">User Persona ID</param>		
        /// <param name="productId">Product ID</param>		
        /// <returns>Property Role object</returns>
        ListResponse GetAssignedPropertyForPersona(long userPersonaId, long productId);
    }
}
