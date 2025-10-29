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
    public interface IPersonRepository
    {
        /// <summary>
        /// Create a new Password Policy
        /// </summary>
        RepositoryResponse CreatePerson(IPerson person);

        /// <summary>
        /// Get the person detail by unique identifier
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>User object</returns>
        Person GetPerson(Guid realPageId);


        /// <summary>
        /// Update Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="person">Person object of the parameter values</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse UpdatePerson(Guid realPageId, IPerson person);

        /// <summary>
        /// Get the person detail by unique identifier
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>TelecommunicationNumber object</returns>
        List<TelecommunicationNumber> GetPersonPhone(Guid realPageId);
    }

}