using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using System.Linq;
using UnifiedLogin.SharedObjects.Landing.Enum;
using UnifiedLogin.SharedObjects.Constants;

namespace UnifiedLogin.BusinessLogic.Repository
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
			var settings = GetUnifiedSettings(partyId, "Security");

			//There should be a setting for each one of the properties. If a setting is missing, then something is wrong with the data in the DB
			var policy = new PasswordPolicy()
			{
				PartyId = partyId,
				MinimumLength = Byte.Parse(settings.FirstOrDefault(a => a.Name == "MinimumLength").Value),
				MaximumLength = Byte.Parse(settings.FirstOrDefault(a => a.Name == "MaximumLength").Value),
				MinimumLowercase = Byte.Parse(settings.FirstOrDefault(a => a.Name == "MinimumLowercase").Value),
				MinimumUppercase = Byte.Parse(settings.FirstOrDefault(a => a.Name == "MinimumUppercase").Value),
				MinimumNumeric = Byte.Parse(settings.FirstOrDefault(a => a.Name == "MinimumNumeric").Value),
				MinimumSpecialCharacter = Byte.Parse(settings.FirstOrDefault(a => a.Name == "MinimumSpecialCharacter").Value),
				EnablePasswordExpiration = settings.FirstOrDefault(a => a.Name == "EnablePasswordExpiration").Value == "1",
				PasswordExpirationPeriodInDays = Int16.Parse(settings.FirstOrDefault(a => a.Name == "PasswordExpirationPeriodInDays").Value),
				PreventPasswordReuse = settings.FirstOrDefault(a => a.Name == "PreventPasswordReuse").Value == "1",
				NumberOfPasswordsToRemember = Byte.Parse(settings.FirstOrDefault(a => a.Name == "NumberOfPasswordsToRemember").Value),
				AllowUsersToChangeOwnPassword = settings.FirstOrDefault(a => a.Name == "AllowUsersToChangeOwnPassword").Value == "1" ? true : false
			};

			return policy;
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

		private IList<Setting> GetUnifiedSettings(long PartyId, string Category)
		{
			dynamic param = new
			{
				PartyId = PartyId,
				Category = Category
			};

			using (var repository = GetRepository())
			{
				return repository.GetMany<Setting>(StoredProcNameConstants.SP_GetUnifiedSetting, param);
			}
		}
		#endregion
	}
}