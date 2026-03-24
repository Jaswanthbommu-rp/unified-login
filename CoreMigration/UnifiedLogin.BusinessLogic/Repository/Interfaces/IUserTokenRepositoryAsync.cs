using System;
using System.Threading;
using System.Threading.Tasks;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// 
    /// </summary>
    public interface IUserTokenRepositoryAsync
    {
        /// <summary>
        /// Get User token
        /// </summary>
        Task<string> GetUserActivityTokenAsync(Guid realPageId, int activityTypeId, long partyId, CancellationToken cancellationToken = default);
    }
}