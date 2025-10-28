using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for ProductUsers
	/// </summary>
	public interface IProductUsers
	{
		/// <summary>
		/// List of User Personas
		/// </summary>
		IList<PersonaCommon> persona { get; set; }

		/// <summary>
		/// UserLogin attributes common to UserLogin and ProductUsers classes
		/// </summary>
		UserLoginCommon userLogin { get; set; }
	}
}