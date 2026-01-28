using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using System;
using UnifiedLogin.DataAccess;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
    /// <summary>
    /// Manage Person repository calls
    /// </summary>
    public class ManagePerson : IManagePerson
    {
        #region Private Variables
        IPersonRepository _personRepository;
        #endregion

        #region Constructors
        /// <summary>
        /// ManagePerson Constructor
        /// </summary>
        /// <param name="personRepository">Person Repository</param>
        public ManagePerson(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        /// <summary>
        /// Create a basic instance of the ManagePerson Controller class
        /// </summary>
        public ManagePerson()
        {
            _personRepository = new PersonRepository();
        }

        /// <summary>
        /// Create a basic instance of the ManagePerson Controller class
        /// </summary>
        public ManagePerson(IRepository repository)
        {
            _personRepository = new PersonRepository(repository);
        }

        #endregion

        #region Public ManagePerson methods
        /// <summary>
        /// Create a Person
        /// </summary>
        /// <param name="person">Person data object</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse CreatePerson(IPerson person)
        {
            if (person == null)
            {
                throw new ArgumentNullException(nameof(person), "Null Person.");
            }

            return _personRepository.CreatePerson(person);
        }

        /// <summary>
        /// Get Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <returns>Person object</returns>
        public Person GetPerson(Guid realPageId)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }

            return _personRepository.GetPerson(realPageId);
        }

        /// <summary>
        /// Update an existing Person
        /// </summary>
        /// <param name="realPageId">User unique identifier</param>
        /// <param name="person">Person object of the parameter values</param>
        /// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdatePerson(Guid realPageId, IPerson person)
        {
            if (realPageId == Guid.Empty)
            {
                throw new Exception("Invalid parameter realPageId.");
            }

            if (person == null)
            {
                throw new ArgumentNullException(nameof(person), "Null Person.");
            }

            return _personRepository.UpdatePerson(realPageId, person);
        }


        #endregion
    }
}