using System;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Logic
{
	/// <summary>
	/// Manage GeographicBoundary repository calls
	/// </summary>
	public class ManageGeographicBoundary : IManageGeographicBoundary
	{
		#region Private Variables
		IGeographicBoundaryRepository _geographicBoundaryRepository;
		#endregion

		#region Constructors
		/// <summary>
		/// ManageGeographicBoundary Constructor
		/// </summary>
		/// <param name="geographicBoundaryRepository">Contact Mechanism Repository</param>
		public ManageGeographicBoundary(IGeographicBoundaryRepository geographicBoundaryRepository)
		{
			_geographicBoundaryRepository = geographicBoundaryRepository;
		}

		/// <summary>
		/// Create a basic instance of the ManageGeographicBoundary Controller class
		/// </summary>
		/// 
		public ManageGeographicBoundary()
		{
			_geographicBoundaryRepository = new GeographicBoundaryRepository();
		}
		#endregion

		#region Public GeographicBoundaryMechanism methods
		/// <summary>
		/// Create Geographic Boundary for a Person (e.g. City: Richardson)
		/// </summary>
		/// <param name="geographicBoundary">GeographicBoundary Object.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateGeographicBoundary(IGeographicBoundary geographicBoundary)
		{
			if (geographicBoundary == null)
			{
				throw new ArgumentNullException(nameof(geographicBoundary), "Null GeographicBoundary.");
			}

			return _geographicBoundaryRepository.CreateGeographicBoundary(geographicBoundary);
		}
		#endregion
	}
}