using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.UnifiedLogin
{
	public class UnifiedLoginRoleRights
	{
		/// <summary>
		/// User Role Rights list
		/// </summary>
		public UnifiedLoginRoleRights()
		{
			UserRights = new List<UnifiedLoginRight>();
		}

		/// <summary>
		/// Unique RoleId
		/// </summary>
		[JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
		public int RoleId { get; set; }

		/// <summary>
		/// RoleName
		/// </summary>
		[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
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
		public IList<UnifiedLoginRight> UserRights { get; set; }
	}
}
