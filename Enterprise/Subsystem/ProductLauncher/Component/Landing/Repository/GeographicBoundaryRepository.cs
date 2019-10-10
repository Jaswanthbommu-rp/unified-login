using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository
{
	/// <summary>
	/// Geographic Boundary Repository
	/// </summary>
	public class GeographicBoundaryRepository : BaseRepository, IGeographicBoundaryRepository
	{
		#region Constructor
		/// <summary>
		/// Geographic Boundary base Constructor
		/// </summary>
		public GeographicBoundaryRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}
		#endregion

		#region public GeographicBoundaryRepository methods
		/// <summary>
		/// Create Geographic Boundary for a Person (e.g. City: Richardson)
		/// </summary>
		/// <param name="geographicBoundary">GeographicBoundary Object.</param>
		/// <returns>Repository response object</returns>
		public RepositoryResponse CreateGeographicBoundary(IGeographicBoundary geographicBoundary)
		{
			dynamic param = new
			{
				TypeName = geographicBoundary.GeographicBoundaryType.TypeName,
				Value = geographicBoundary.Name,
				Code = geographicBoundary.GeographicBoundaryCode,
				Abbreviation = geographicBoundary.Abbreviation
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateGeographicBoundary, param);
				return result;
			}
		}
		#endregion
	}
}