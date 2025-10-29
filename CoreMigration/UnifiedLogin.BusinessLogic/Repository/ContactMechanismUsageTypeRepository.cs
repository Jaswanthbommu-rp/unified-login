using System.Collections.Generic;
using UnifiedLogin.DataAccess;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.IdentityConfig;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// Contact Mechanism Usage Type Repository
	/// </summary>
	public class ContactMechanismUsageTypeRepository : BaseRepository, IContactMechanismUsageTypeRepository
	{
		#region Constructor
		/// <summary>
		/// Contact Mechanism UsageType base Constructor
		/// </summary>
		public ContactMechanismUsageTypeRepository() : base(DbConnectionEnum.IdpConfigurationDb)
        {
		}

        /// <summary>
        /// Unit test constructor
        /// </summary>
        /// <param name="repository"></param>
        public ContactMechanismUsageTypeRepository(IRepository repository) : base(repository)
        {
        }
        #endregion

        #region public ContactMechanismUsageType Repository methods
        /// <summary>
        /// Get a list of Contact Mechanism Usage Types
        /// </summary>
        /// <param name="ContactMechanismUsageTypeName">Contact Mechanism UsageType Name</param>
        /// <returns>List of Contact Mechanism Usage Types</returns>
        public IList<ContactMechanismUsageType> ListContactMechanismUsageType(string ContactMechanismUsageTypeName)
		{
			dynamic param = new
			{
				ContactMechanismUsageTypeName
			};

            try
            {
                using (var repository = GetRepository())
                {
                    IList<ContactMechanismUsageType> result = repository.GetMany<ContactMechanismUsageType>(StoredProcNameConstants.SP_ListContactMechanismUsageType, param);
                    return result;
                }
            }
            catch {
                return null;
            }
		}
		#endregion
	}
}