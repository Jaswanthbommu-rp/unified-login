using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// User Role Rights
	/// </summary>
    public class UserRoleRights
    {
		/// <summary>
		/// User Role Rights list
		/// </summary>
        public UserRoleRights()
        {
            UserRights = new List<Right>();
        }

		/// <summary>
		/// Unique RoleId
		/// </summary>
        public int RoleId { get; set; }

		/// <summary>
		/// RoleName
		/// </summary>
        public string Role { get; set; }

		/// <summary>
		/// Role ShortName (NickName)
		/// </summary>
        public string RoleNickName { get; set; }

		/// <summary>
		/// Role Type
		/// </summary>
        public string Roletype { get; set; }
		/// <summary>
		/// Is the Default role assigned 
		/// </summary>
		public string DefaultRole { get; set; }
		/// <summary>
		/// Is the role assigned to the user in the product
		/// </summary>
		public bool IsAssigned { get; set; }
		/// <summary>
		/// Role's Rights list
		/// </summary>
		public IList<Right> UserRights { get; set; }

    }
}
