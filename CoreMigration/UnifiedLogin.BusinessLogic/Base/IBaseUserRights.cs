using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Base
{
    public interface IBaseUserRights
    {
        List<string> GetUserRightsBy(ClaimsPrincipal userPrincipal, DefaultUserClaim userClaim);
        List<string> GetImpersonatedUserRights(Guid impersonatedBy, DefaultUserClaim userClaims);
        List<string> GetImpersonatedUserRightsByPersona(Persona impersonateUserPersona, DefaultUserClaim userClaims);
    }
}
