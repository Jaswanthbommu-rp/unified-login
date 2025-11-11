using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.SharedObjects.Constants;

namespace UnifiedLogin.BusinessLogic.Repository
{
    /// <summary>
    /// Person Repository
    /// </summary>
    public class PersonRepository : BaseRepository, IPersonRepository
    {
        #region Constructor
        /// <summary>
        /// Person base Constructor
        /// </summary>
        public PersonRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        /// <summary>
        /// Person base Constructor
        /// </summary>
        public PersonRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        #region public Person methods
        /// <summary>
        /// Create a new Person
        /// </summary>
        /// <param name="person">Person object of the parameter values</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreatePerson(IPerson person)
        {
            dynamic param = new
            {
                person.FirstName,
                person.MiddleName,
                person.LastName,
                person.Title,
                person.Suffix,
                person.PreferredContactMethodId,
                person.RealPageId
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePerson, param);
                return result;
            }
        }

        /// <summary>
        /// Get the person detail by unique identifier
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>Person object</returns>
        public Person GetPerson(Guid realPageId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetOne<Person>(StoredProcNameConstants.SP_GetPerson, new { realPageId });
            }
        }

        /// <summary>
        /// Update Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="person">Person object of the parameter values</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse UpdatePerson(Guid realPageId, IPerson person)
        {
            dynamic param = new
            {
                realPageId,
                person.FirstName,
                person.MiddleName,
                person.LastName,
                person.Title,
                person.Suffix,
                person.PreferredContactMethodId
            };

            using (var repository = GetRepository())
            {
                var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePerson, param);
                return result;
            }
        }

        /// <summary>
        /// Get the person detail by unique identifier
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>TelecommunicationNumber object</returns>
        public List<TelecommunicationNumber> GetPersonPhone(Guid realPageId)
        {
            using (var repository = GetRepository())
            {
                return repository.GetMany<TelecommunicationNumber>(StoredProcNameConstants.SP_ListTelecommunicationNumbersForPerson, new { realPageId }).ToList<TelecommunicationNumber>();

            }
        }
        #endregion
    }
}