using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using System;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage PasswordPolicy repository calls
	/// </summary>
	public class ManagePasswordPolicy : IManagePasswordPolicy
    {
		#region Private Variables
		IPasswordPolicyRepository _passwordPolicyRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManagePasswordPolicy Constructor
		/// </summary>
		/// <param name="passwordPolicyRepository">Password Policy Repository</param>
		public ManagePasswordPolicy(IPasswordPolicyRepository passwordPolicyRepository)
        {
            _passwordPolicyRepository = passwordPolicyRepository;
        }

		/// <summary>
		/// Create a basic instance of the ManageUser Controller class
		/// </summary>
		public ManagePasswordPolicy()
        {
            _passwordPolicyRepository = new PasswordPolicyRepository();
        }
		#endregion

		#region Public ManagePasswordPolicy methods
		/// <summary>
		/// Create a Password Policy
		/// </summary>
		/// <param name="passwordPolicy">Password policy data object</param>
		/// <returns>RepositoryResponse object</returns>
		public RepositoryResponse CreatePasswordPolicy(IPasswordPolicy passwordPolicy)
		{
			if (passwordPolicy == null)
			{
				throw new ArgumentNullException(nameof(passwordPolicy), "Null Password Policy.");
			}

			return _passwordPolicyRepository.CreatePasswordPolicy(passwordPolicy);
		}

        /// <summary>
        /// Get Password policy for an Portfolio
        /// </summary>
        /// <param name="partyId">party Id</param>
        /// <returns>Password policy object</returns>
        public PasswordPolicy GetPasswordPolicy(long partyId)
        {
            if (partyId <= 0)
            {
                throw new Exception("Missing Party Id.");
            }

            return _passwordPolicyRepository.GetPasswordPolicy(partyId);
        }

		/// <summary>
		/// Update an existing Password policy
		/// </summary>
		/// <param name="passwordPolicy">Password policy data object</param>
		/// <returns>RepositoryResponse object</returns>
        public RepositoryResponse UpdatePasswordPolicy(IPasswordPolicy passwordPolicy)
        {
            if (passwordPolicy == null)
            {
                throw new ArgumentNullException(nameof(passwordPolicy), "Null Password Policy.");
            }

            return _passwordPolicyRepository.UpdatePasswordPolicy(passwordPolicy);
        }
		#endregion
	}
}