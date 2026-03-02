using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.SharedObjects.Kafka
{
    /// <summary>
    /// Provides lookup for user login information.
    /// </summary>
    public interface IUserLoginLookup
    {
        /// <summary>
        /// Gets user login information by RealPage identifier.
        /// </summary>
        /// <param name="realPageId">The user's RealPage unique identifier.</param>
        /// <returns>UserLoginOnly object.</returns>
        UserLoginOnly GetUserLoginOnly(Guid realPageId);
    }
}
