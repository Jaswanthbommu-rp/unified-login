using System;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
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