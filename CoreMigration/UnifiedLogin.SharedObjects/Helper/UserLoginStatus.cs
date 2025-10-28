using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.SharedObjects.Helper
{
    public class UserLoginStatus
	{
		/// <summary>
		/// Set the user status
		/// </summary>
		/// <param name="userLogin">user Login</param>
		/// <returns>User object</returns>
		public static UserLogin SetUserLoginStatus(UserLogin userLogin)
        {
            DateTime utcNowWithOffset = DateTime.UtcNow.AddMinutes(userLogin.OffsetMinutes);

			userLogin.IsPending = false;
			userLogin.IsExpired = false;
			userLogin.IsActive = false;
			userLogin.IsLocked = false;
			userLogin.IsForceReSetPassword = false;
			userLogin.Status = UserUiStatusType.UnDefined;

			// see if user is SuperUser ? may need to update later
			userLogin.IsSuperUser = (userLogin.UserRoleType == UserRoleType.SuperUser) ? true : false;
			UserUiStatusType status;
			if (System.Enum.TryParse(userLogin.StatusId.ToString(), true, out status))
			{
				switch (status)
				{
					case UserUiStatusType.ForceResetPassword:
						if (userLogin.StatusThruDate != null && userLogin.StatusThruDate.Value >= utcNowWithOffset)
						{
							userLogin.IsForceReSetPassword = true;
							userLogin.IsActive = true;
							userLogin.IsPending = true;
							userLogin.Status = UserUiStatusType.Active;
							//user never loggedin then set to pending 
							if (userLogin.LastLogin == null)
							{
								userLogin.Status = UserUiStatusType.Pending;
							}							
						}
						else if (userLogin.StatusThruDate != null && userLogin.StatusThruDate.Value < utcNowWithOffset)
						{
							userLogin.IsExpired = true;
							userLogin.IsActive = true;
							userLogin.Status = UserUiStatusType.Expired;
						}
						break;
					case UserUiStatusType.Pending:
                        if (userLogin.StatusThruDate != null && userLogin.StatusThruDate.Value >= utcNowWithOffset && userLogin.PasswordModifiedDate != null)
                        {
                            userLogin.IsPending = true;
                            userLogin.IsActive = true;
                            userLogin.Status = UserUiStatusType.Active;
                        }
                        else if (userLogin.StatusThruDate != null && userLogin.StatusThruDate.Value >= utcNowWithOffset)
                        {
                            userLogin.IsPending = true;
							userLogin.IsActive = true;
							userLogin.Status = UserUiStatusType.Pending;
						}
						else if (userLogin.StatusThruDate != null && userLogin.StatusThruDate.Value < utcNowWithOffset)
						{
							userLogin.IsExpired = true;
							userLogin.IsActive = true;
							userLogin.Status = UserUiStatusType.Expired;
						}
						break;
					case UserUiStatusType.Active:						
						if (userLogin.StatusThruDate == null && userLogin.FromDate <= utcNowWithOffset && (userLogin.ThruDate == null || userLogin.ThruDate.Value >= utcNowWithOffset))
						{
							userLogin.IsActive = true;
							userLogin.Status = UserUiStatusType.Active;
						}
						else if (userLogin.StatusThruDate == null && userLogin.FromDate <= utcNowWithOffset 	&& userLogin.ThruDate.Value < utcNowWithOffset)
						{
							userLogin.IsActive = false;
							userLogin.Status = UserUiStatusType.Disabled;
						}				
						else if (userLogin.StatusThruDate != null && userLogin.StatusThruDate.Value < utcNowWithOffset)
						{
							userLogin.IsExpired = true;
							userLogin.IsActive = true;
							userLogin.Status = UserUiStatusType.Expired;
						}		
						break;
					case UserUiStatusType.Locked:
						userLogin.IsLocked = true;
						userLogin.IsActive = true;
						userLogin.Status = UserUiStatusType.Locked;						
						break;
					case UserUiStatusType.Disabled:
						userLogin.IsActive = false;
						userLogin.Status = UserUiStatusType.Disabled;
						break;
					case UserUiStatusType.Expired:
						userLogin.IsExpired = true;
						userLogin.IsActive = true;
						userLogin.Status = UserUiStatusType.Expired;
						break;
				}
			}

			return userLogin;
		}
	}
}
