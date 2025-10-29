using System;
using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage Telecommunication Number repository calls
	/// </summary>
	public class ManageTelecommunicationNumber : IManageTelecommunicationNumber
	{
		#region Private Variables
		ITelecommunicationNumberRepository _telecommunicationNumberRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageTelecommunicationNumber Constructor
		/// </summary>
		/// <param name="telecommunicationNumberRepository">Telecommunication Number Repository</param>
		public ManageTelecommunicationNumber(ITelecommunicationNumberRepository telecommunicationNumberRepository)
		{
			_telecommunicationNumberRepository = telecommunicationNumberRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageTelecommunicationNumber Controller class
		/// </summary>
		/// 
		public ManageTelecommunicationNumber()
		{
			_telecommunicationNumberRepository = new TelecommunicationNumberRepository();
		}
		#endregion

		#region Public ManageTelecommunicationNumber methods
		/// <summary>
		/// Link telecommunication number to a person
		/// </summary>
		/// <param name="telecommunicationNumber">Person's telecommunication number parameter values</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateTelecommunicationNumber(ITelecommunicationNumber telecommunicationNumber)
		{
			if (telecommunicationNumber == null)
			{
				throw new ArgumentNullException(nameof(telecommunicationNumber), "Null TelecommunicationNumber.");
			}

			return _telecommunicationNumberRepository.CreateTelecommunicationNumber(telecommunicationNumber);
		}

		/// <summary>
		/// List telecommunication number details for a person
		/// </summary>
		/// <param name="realPageId">User unique identifier</param>
		/// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
		/// <returns>List of telecommunication number details for a person</returns>
		public IList<TelecommunicationNumber> ListTelecommunicationNumberForPerson(Guid realPageId, string ContactMechanismUsageTypeName)
		{
			if (realPageId == Guid.Empty)
			{
				throw new Exception("Invalid parameter realPageId.");
			}

			return _telecommunicationNumberRepository.ListTelecommunicationNumberForPerson(realPageId, ContactMechanismUsageTypeName);
		}
		#endregion
	}
}