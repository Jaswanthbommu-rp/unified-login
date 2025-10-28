using Newtonsoft.Json;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{	
	/// <summary>
	/// Response when creating a new user
	/// </summary>
	public class CreateUserResponse<T> : ICreateUserResponse<T>
	{
		/// <summary>
		/// Email Template
		/// </summary>
		public string EmailTemplate { get; set; }

		/// <summary>
		/// Email Status
		/// </summary>
		public string EmailStatus { get; set; }

		/// <summary>
		/// User Status
		/// </summary>
		public string UserStatus { get; set; }

		/// <summary>
		/// User Token
		/// </summary>
		public string UserToken { get; set; }

		/// <summary>
		/// Persona Unique Id
		/// </summary>
		public long PersonaId { get; set; }

		/// <summary>
		/// System Party Contact Mechanism Id
		/// </summary>
		public long PartyContactMechanismIdFrom { get; set; }

		/// <summary>
		/// New User Party Contact Mechanism Id
		/// </summary>
		public long PartyContactMechanismIdTo { get; set; }

		/// <summary>
		/// Error status object - API/UI Call Success/Error Communication
		/// </summary>
		[JsonProperty("Status", NullValueHandling = NullValueHandling.Ignore)]
		public Status<T> Status { get; set; }

        /// <summary>
        /// Created user realpage guid
        /// </summary>
        public Guid UserRealPageGuid { get; set; }
    }
}

