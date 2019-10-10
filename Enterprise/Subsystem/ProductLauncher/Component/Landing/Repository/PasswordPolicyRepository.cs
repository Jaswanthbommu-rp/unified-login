using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Password Policy Repository
	/// </summary>
	public class PasswordPolicyRepository : BaseRepository, IPasswordPolicyRepository
	{
		#region Constructor
		/// <summary>
		/// Password Policy base Constructor
		/// </summary>
		public PasswordPolicyRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public PasswordPolicyRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        #region public PasswordPolicy methods
        /// <summary>
        /// Get Password Policy
        /// </summary>
        /// <param name="partyId">party id</param>
        /// <returns>Password Policy object</returns>
        public PasswordPolicy GetPasswordPolicy(long partyId)
		{
			using (var repository = GetRepository())
			{
				return repository.GetOne<PasswordPolicy>(StoredProcNameConstants.SP_GetPasswordPolicy, new { partyId });
			}
		}

		/// <summary>
		/// Update Password Policy
		/// </summary>
		/// <param name="passwordPolicy">Password Policy object of the parameter values</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse UpdatePasswordPolicy(IPasswordPolicy passwordPolicy)
		{
			dynamic param = new
			{
				passwordPolicy.PasswordPolicyId,
				passwordPolicy.MinimumLength,
				passwordPolicy.MaximumLength,
				passwordPolicy.MinimumLowercase,
				passwordPolicy.MinimumUppercase,
				passwordPolicy.MinimumNumeric,
				passwordPolicy.MinimumSpecialCharacter,
				passwordPolicy.AllowUsersToChangeOwnPassword,
				passwordPolicy.EnablePasswordExpiration,
				passwordPolicy.PasswordExpirationPeriodInDays,
				passwordPolicy.PreventPasswordReuse,
				passwordPolicy.NumberOfPasswordsToRemember,
				passwordPolicy.UserId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_UpdatePasswordPolicy, param);
				return result;
			}
		}

		/// <summary>
		/// Create a new Password Policy
		/// </summary>
		/// <param name="passwordPolicy">Password Policy object of the parameter values</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreatePasswordPolicy(IPasswordPolicy passwordPolicy)
		{
			dynamic param = new
			{
				passwordPolicy.PartyId,
				passwordPolicy.MinimumLength,
				passwordPolicy.MaximumLength,
				passwordPolicy.MinimumLowercase,
				passwordPolicy.MinimumUppercase,
				passwordPolicy.MinimumNumeric,
				passwordPolicy.MinimumSpecialCharacter,
				passwordPolicy.AllowUsersToChangeOwnPassword,
				passwordPolicy.EnablePasswordExpiration,
				passwordPolicy.PasswordExpirationPeriodInDays,
				passwordPolicy.PreventPasswordReuse,
				passwordPolicy.NumberOfPasswordsToRemember,
				passwordPolicy.UserId
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreatePasswordPolicy, param);
				return result;
			}
		}
		#endregion
	}
}