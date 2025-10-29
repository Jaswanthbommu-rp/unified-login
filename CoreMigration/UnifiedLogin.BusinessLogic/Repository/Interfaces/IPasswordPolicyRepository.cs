using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository.Interfaces
{
	/// <summary>
	/// Interface for Password Policy Repository
	/// </summary>
	public interface IPasswordPolicyRepository
    {
        /// <summary>
        /// Create a new Password Policy
        /// </summary>
        /// <param name="passwordPolicy">Password Policy object of the parameter values</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse CreatePasswordPolicy(IPasswordPolicy passwordPolicy);

        /// <summary>
        /// Get Password Policy
        /// </summary>
        /// <param name="partyId">Party Id</param>
        /// <returns>Password Policy object</returns>
        PasswordPolicy GetPasswordPolicy(long partyId);

        /// <summary>
        /// Update Password Policy
        /// </summary>
        /// <param name="passwordPolicy">Password Policy object of the parameter values</param>
        /// <returns>Repository response object</returns>
        RepositoryResponse UpdatePasswordPolicy(IPasswordPolicy passwordPolicy);
	}
}