using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Product.UnifiedLogin
{
	/// <summary>
	/// Property Role
	/// </summary>
	public class PropertyRole
    {
		/// <summary>
		/// RoleId
		/// </summary>
        public long RoleID { get; set; }

		/// <summary>
		/// PropertyId
		/// </summary>
        public long PropID { get; set; }
    }

	/// <summary>
	/// User Property
	/// </summary>
    public class UserProperty
    {
		/// <summary>
		/// PropertyId
		/// </summary>
		public long PropID { get; set; }

		/// <summary>
		/// IsAssigned to property
		/// </summary>
        public bool IsAssigned { get; set; }
    }

	/// <summary>
	/// Property
	/// </summary>
    public class Property
    {
		/// <summary>
		/// PropertyId
		/// </summary>
		public long PropID { get; set; }
    }

	/// <summary>
	/// The role assigned to products using the UnifiedLogin role/rights system
	/// </summary>
    public class Role
    {
		/// <summary>
		/// The id of the role
		/// </summary>
        public long RoleID { get; set; }

		/// <summary>
		/// The name of the role
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Role ShortName (NickName)
		/// </summary>
		public string RoleNickName { get; set; }
		
		/// <summary>
		/// PersoanId
		/// </summary>
		public string PersonaId { get; set; }

		/// <summary>
		/// List of User Personas
		/// </summary>
		[JsonProperty("Right", NullValueHandling = NullValueHandling.Ignore)]
		public IList<Right> Right { get; set; } = new List<Right>();
	}
}
