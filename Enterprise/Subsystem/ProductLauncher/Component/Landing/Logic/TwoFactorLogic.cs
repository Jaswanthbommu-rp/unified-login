using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
    public class TwoFactorLogic : ITwoFactorLogic
    {
        private readonly ITwoFactorRepository _twoFactorRepository;
        private readonly IUserLoginRepository _userLoginRepository;
        private readonly DefaultUserClaim _userClaim;
        private readonly IPersonRepository _personRepository;
        private const string TRUSTED_DEVICE_COOKIE_PREFIX = "TrustedDevice_";

        public TwoFactorLogic(DefaultUserClaim userClaim, IRepository repository)
        {
            _twoFactorRepository = repository == null ? new TwoFactorRepository() : new TwoFactorRepository(repository);
            _userLoginRepository = repository == null ? new UserLoginRepository() : new UserLoginRepository(repository);
            _personRepository = repository == null ? new PersonRepository() : new PersonRepository(repository);

            _userClaim = userClaim;
        }

        public int DeleteUserAppAuthToken(Guid realPageId)
        {
          this.RemoveDeviceTrust(HttpContext.Current.Response, realPageId);
			var userLogin = _userLoginRepository.GetUserLoginOnly(realPageId);

            if (userLogin != null)
            {
                var result = _twoFactorRepository.ResetAuthenticatorKey(userLogin.UserId, string.Empty);
                
                if (result > 0 && _userClaim != null)
                    LogDeleteActivity(realPageId, userLogin);

                return result;
            }

            return 0;
        }

		public void RemoveDeviceTrust(HttpResponse response, Guid userId)
		{
			string cookieName = $"TrustedDevice_{userId}";
			if (response.Cookies[cookieName] != null)
			{
				var expiredCookie = new HttpCookie(cookieName)
				{
					Expires = DateTime.Now.AddDays(-1),
					Path = "/"
				};
				response.Cookies.Add(expiredCookie);
			}
		}

        public int UpdateUserTwoFactorStatus(Guid realPageId, int status)
        {
            var userLogin = _userLoginRepository.GetUserLoginOnly(realPageId);
            if (userLogin != null)
            {
                return _twoFactorRepository.UpdateUserTwoFactorStatus(userLogin.UserId, status);
            }

            return 0;
        }

        private void LogDeleteActivity(Guid realPageId, UserLoginOnly userLogin) 
        {
            var person = _personRepository.GetPerson(realPageId);
            if (realPageId == _userClaim.UserRealPageGuid)
            {
                LogActivity.WriteActivity(new ActivityDetails()
                {
                    LogActivityTypeName = "Update User",
                    LogCategoryName = LogActivityCategoryType.User.ToString(),
                    CorrelationId = _userClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                    OrganizationPartyId = _userClaim.OrganizationPartyId,
                    Message = $"Multi-factor authentication method reset by {person.FirstName} {person.LastName}.",

                    FromUserLoginName = _userClaim.LoginName,
                    FromUserLoginId = _userClaim.UserId,
                    FromUserFirstName = _userClaim.FirstName,
                    FromUserLastName = _userClaim.LastName,
                    FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),
                    BooksProductCode = "UPFM"
                });
            }
            else
            {
                LogActivity.WriteActivity(new ActivityDetails()
                {
                    LogActivityTypeName = "Update User",
                    LogCategoryName = LogActivityCategoryType.User.ToString(),
                    CorrelationId = _userClaim.CorrelationId.ToString(),
                    BooksMasterOrganizationId = _userClaim.OrganizationMasterId,
                    OrganizationPartyId = _userClaim.OrganizationPartyId,
                    Message = $"{_userClaim.FirstName} {_userClaim.LastName} reset the multi-factor authentication setup for { person.FirstName} {person.LastName}.",

                    FromUserLoginName = _userClaim.LoginName,
                    FromUserLoginId = _userClaim.UserId,
                    FromUserFirstName = _userClaim.FirstName,
                    FromUserLastName = _userClaim.LastName,
                    FromUserRealpageId = _userClaim.UserRealPageGuid.ToString(),

                    ToUserFirstName = person.FirstName,
                    ToUserLastName = person.LastName,
                    ToUserLoginId = userLogin.UserId,
                    ToUserLoginName = userLogin.LoginName,
                    ToUserRealpageId = userLogin.RealPageId.ToString(),

                    BooksProductCode = "UPFM"
                });
            }
        }
    }
}
