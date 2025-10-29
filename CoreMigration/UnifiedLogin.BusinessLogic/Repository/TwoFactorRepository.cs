using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.BusinessLogic.Repository
{
    public class TwoFactorRepository : BaseRepository, ITwoFactorRepository
    {
        /// <summary>
        /// Base constructor
        /// </summary>
        public TwoFactorRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public TwoFactorRepository(IRepository repository) : base(repository)
        {
        }

        public int ResetAuthenticatorKey(long userId, string authenticatorKey)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    UserId = userId,
                    LoginProvider =  "AppAuth",
                    Name = "AuthenticatorKey",
                    Value = authenticatorKey
                };
                return repository.ExecuteNonQuery(StoredProcNameConstants.SP_CreateUpdateUserTokenDetail, param);
            }
        }

        /// <summary>
        /// Used to update the users two factor status, 0 disabled, 1 active, 2 pending
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="status"></param>
        public int UpdateUserTwoFactorStatus(long userId, int status)
        {
            using (var repository = GetRepository())
            {
                dynamic param = new
                {
                    UserId = userId,
                    Status = status
                };
                return repository.ExecuteNonQuery(StoredProcNameConstants.SP_UpdateUserLoginTwoFactor, param);
            }
        }

    }
}
