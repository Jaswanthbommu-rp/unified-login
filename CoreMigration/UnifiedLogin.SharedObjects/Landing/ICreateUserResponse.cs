using UnifiedLogin.SharedObjects.Landing;
using System;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Interface for Create User Response
    /// </summary>
    public interface ICreateUserResponse<T>
    {
        /// <summary>
        /// Returns the Email Template for New Users
        /// </summary>		
        string EmailTemplate { get; set; }

        /// <summary>
        /// Returns the Status if Email was sent successfully
        /// </summary>		
        string EmailStatus { get; set; }

        /// <summary>
        /// Returns the Status if User was created successfully
        /// </summary>		
        string UserStatus { get; set; }

        /// <summary>
        /// Returns the User Token for New Users
        /// </summary>		
        string UserToken { get; set; }

        /// <summary>
        /// Returns the Persona Id
        /// </summary>		
        long PersonaId { get; set; }

		/// <summary>
		/// System Party Contact Mechanism Id
		/// </summary>
		long PartyContactMechanismIdFrom { get; set; }

		/// <summary>
		/// New User Party Contact Mechanism Id
		/// </summary>
		long PartyContactMechanismIdTo { get; set; }
		
		/// <summary>
		/// Error status object - API/UI Call Success/Error Communication
		/// </summary>
		Status<T> Status { get; set; }

        /// <summary>
        /// Created user realpage guid
        /// </summary>
        Guid UserRealPageGuid { get; set; }
    }
}