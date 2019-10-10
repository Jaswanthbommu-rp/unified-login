using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserTokenRepository
    {
        /// <summary>
        /// Get User token
        /// </summary>
        string GetUserActivityToken(Guid realPageId,int activityId,long partyId);
    }
}