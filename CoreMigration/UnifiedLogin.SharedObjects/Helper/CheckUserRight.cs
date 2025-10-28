using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Helper
{
	public class CheckUserRight
    {
        /// <summary>
		/// Checks if User Has Access to Right
		/// </summary> 
		public static bool CheckUserHasAccess(List<string> userRights, string userRight)
        {
            foreach (var right in userRights)
            {
                if (right?.Length > 0)
                {
                    if (right.ToLower().Equals(userRight.ToLower())){
                        return true;
                    }                    
                }
            }
            return false;
        }
    }
}
