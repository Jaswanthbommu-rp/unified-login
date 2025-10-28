using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for User Profile
	/// </summary>
	public interface IProfile : IPerson
	{
		/// <summary>
		/// PartyRole (e.g. User Job Title) attributes
		/// </summary>
		PartyRole PartyRole { get; set; }

		/// <summary>
		/// Contact mechanisim telecommunication number attributes
		/// </summary>
		IList<TelecommunicationNumber> TelecommunicationNumber { get; set; }

        /// <summary>
		/// Contact mechanisim Electronic attributes
		/// </summary>        
        IList<ElectronicAddress> EmailContacts { get; set; }


        /// <summary>
        /// UserLogin attributes
        /// </summary>
        IUserLogin userLogin { get; set; }

        /// <summary>
		/// Impersonate Profile Edit
		/// </summary>       
        bool IsImpersonated { get; set; }
    }
}