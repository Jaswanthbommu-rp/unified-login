using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Street Address Repository
	/// </summary>
	public class StreetAddressRepository : BaseRepository, IStreetAddressRepository
	{
		#region Constructor
		/// <summary>
		/// Street Address base Constructor
		/// </summary>
		public StreetAddressRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}

        public StreetAddressRepository(IRepository repository) : base(repository)
        {
        }

        #endregion

        #region public StreetAddressRepository methods
        /// <summary>
        /// Create the StreetAddress for a Person
        /// </summary>
        /// <param name="streetAddress">StreetAddress Object.</param>
        /// <returns>Repository response object</returns>
        public RepositoryResponse CreateStreetAddress(IStreetAddress streetAddress)
		{
			dynamic param = new
			{
				streetAddress.ContactMechanismId,
				streetAddress.StreetAddress1,
				streetAddress.StreetAddress2,
				streetAddress.StreetAddress3
			};

			using (var repository = GetRepository())
			{
				var result = repository.GetOne<RepositoryResponse>(StoredProcNameConstants.SP_CreateStreetAddress, param);
				return result;
			}
		}
		#endregion
	}
}