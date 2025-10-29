using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects;

namespace UnifiedLogin.BusinessLogic.Repository
{
	/// <summary>
	/// StatusType Repository
	/// </summary>
	public class StatusTypeRepository : BaseRepository, IStatusTypeRepository
	{
		#region Constructor
		/// <summary>
		/// StatusType base Constructor
		/// </summary>
		public StatusTypeRepository() : base(DbConnectionEnum.IdpConfigurationDb)
		{
		}
		#endregion

		#region public Security Settings methods
		/// <summary>
		/// List StatusTypes
		/// </summary>
		/// <param name="CategoryTypeName">Category TypeName (e.g. Status)</param>
		/// <param name="CategoryName">Category Name (e.g. User Status)</param>
		/// <returns>List of StatusType object</returns>
		public IList<StatusType> GetStatusType(string CategoryTypeName, string CategoryName)
		{
			if (string.IsNullOrWhiteSpace(CategoryTypeName))
			{
				throw new Exception("Invalid Category TypeName.");
			}

			if (string.IsNullOrWhiteSpace(CategoryName))
			{
				throw new Exception("Invalid Category name.");
			}

			dynamic param = new
			{
				StatusTypeCategoryTypeName  = CategoryTypeName,
				StatusTypeCategoryName = CategoryName
			};

			using (var repository = GetRepository())
			{
				return repository.GetMany<StatusType>(StoredProcNameConstants.SP_GetStatusTypes, param);
			}
		}
		#endregion
	}
}
