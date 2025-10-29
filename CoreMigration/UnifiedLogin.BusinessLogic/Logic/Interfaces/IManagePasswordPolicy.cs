using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic.Interfaces
{
	/// <summary>
	/// Interface for Manage PasswordPolicy repository calls
	/// </summary>
	public interface IManagePasswordPolicy
    {
		/// <summary>
		/// Create a Password Policy
		/// </summary>
		/// <param name="passwordPolicy">Password policy data object</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse CreatePasswordPolicy(IPasswordPolicy passwordPolicy);

        /// <summary>
        /// Get Password policy for an Portfolio
        /// </summary>
        /// <param name="partyId">Party Id</param>
        /// <returns>Password policy object</returns>
        PasswordPolicy GetPasswordPolicy(long partyId);

		/// <summary>
		/// Update an existing Password policy
		/// </summary>
		/// <param name="passwordPolicy">Password policy data object</param>
		/// <returns>RepositoryResponse object</returns>
		RepositoryResponse UpdatePasswordPolicy(IPasswordPolicy passwordPolicy);
	}
}