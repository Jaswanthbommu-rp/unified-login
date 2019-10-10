using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic;
using System.Threading.Tasks;
using System.Web;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class AuthenticateService
    {
        private static IManageUserLoginIdentity _userLoginLogic;

        public AuthenticateService(ManageUserLoginIdentity userLoginLogic)
        {
            _userLoginLogic = userLoginLogic;
        }

        public async Task<AuthenticateUserResponse> AuthenticateUser(string enterpriseUserName, string password)
        {
            return await AuthenticateUserResponse(enterpriseUserName, password);
        }

        public async Task<AuthenticateUserResponse> AuthenticateExternalUser(string enterpriseUserName)
        {
            return await AuthenticateUserResponse(enterpriseUserName, "");
        }

        private static async Task<AuthenticateUserResponse> AuthenticateUserResponse(string username, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                password = "";
            }

            var authUserDetails = new AuthUserDetails
            {
                EnterpriseUserName = username,
                Password = password,
                UserDeviceDetails = UserDeviceDetails.ParseUserDeviceDetails(HttpContext.Current.Request)
            };

            AuthenticateUserResponse authenticateUserResponse = _userLoginLogic.GetAuthenticatedUser(authUserDetails, !string.IsNullOrEmpty(password));
            //if (!string.IsNullOrEmpty(password))
            //{
            //    authenticateUserResponse = _userLoginLogic.GetAuthenticatedUser(authUserDetails);
            //}
            //else
            //{
            //    authenticateUserResponse = _userLoginLogic.GetAuthenticatedExternalUser(authUserDetails);
            //}

            if (authenticateUserResponse.UserLogin != null)
            {
                authenticateUserResponse.UserLogin.PasswordHash = "";
                authenticateUserResponse.UserLogin.PasswordSalt = "";
            }

            return authenticateUserResponse;
        }
    }
}
