using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
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

    }
}
