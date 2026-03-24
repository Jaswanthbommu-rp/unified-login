using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
    /// <summary>
    /// Interface for Person Repository
    /// </summary>	
    public interface IPersonRepositoryAsync
    {
        /// <summary>
        /// Create a new Password Policy
        /// </summary>
        Task<RepositoryResponse> CreatePersonAsync(IPerson person, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the person detail by unique identifier
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>User object</returns>
        Task<Person> GetPersonAsync(Guid realPageId, CancellationToken cancellationToken = default);


        /// <summary>
        /// Update Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="person">Person object of the parameter values</param>
        /// <returns>Repository response object</returns>
        Task<RepositoryResponse> UpdatePersonAsync(Guid realPageId, IPerson person, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the person detail by unique identifier
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>TelecommunicationNumber object</returns>
        Task<IList<TelecommunicationNumber>> GetPersonPhoneAsync(Guid realPageId, CancellationToken cancellationToken = default);
    }

}