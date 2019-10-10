using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Repository
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        #region Ctor

        public UserRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
        }

        #endregion


        public UserProduct GetUserProductDetailsByUserId(int userId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetOne<UserProduct>("Auth.GetUserProductDetailsByUserId", new { userId });
            }
        }

        internal IList<SecurityQuestion> GetAllSecurityQuestionList()
        {
            using (var repo = GetRepository())
            {
                return repo.GetMany<SecurityQuestion>("Auth.GetAllSecurityQuestions", null).ToList();
            }
        }
         
        internal IList<UserSecurityQuestionAnswer> GetUserSecurityQuestionAnswer(string loginId)
        {
            using (var repo = GetRepository())
            {
                return repo.GetMany<UserSecurityQuestionAnswer>("Auth.GetUserSecurityQuestionAnswer", new { loginId }).ToList();
            }
        }

        internal bool ChangePassword(UserPasswordDetail userPasswordDetail)
        {
            using (var repo = GetRepository())
            {
                return repo.GetOne<bool>("Auth.ChangePassword", new { userPasswordDetail.LoginName, userPasswordDetail.NewPassword }) ;
            }
        }
    }
}