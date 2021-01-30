using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class TwoFactorLogic : ITwoFactorLogic
    {
        private readonly ITwoFactorRepository _twoFactorRepository;
        private readonly IUserLoginRepository _userLoginRepository;
        private readonly DefaultUserClaim _userClaim;

        public TwoFactorLogic(DefaultUserClaim userClaim, IRepository repository)
        {
            _twoFactorRepository = repository == null ? new TwoFactorRepository() : new TwoFactorRepository(repository);
            _userLoginRepository = repository == null ? new UserLoginRepository() : new UserLoginRepository(repository);
            _userClaim = userClaim;
        }

        public int DeleteUserAppAuthToken(Guid realPageId)
        {
            var userLogin = _userLoginRepository.GetUserLoginOnly(realPageId);
            if (userLogin != null)
            {
                return _twoFactorRepository.ResetAuthenticatorKey(userLogin.UserId, string.Empty);
            }

            return 0;
        }
    }
}
