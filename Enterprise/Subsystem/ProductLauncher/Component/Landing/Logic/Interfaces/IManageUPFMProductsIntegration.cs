using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.UPFMProduct;
using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces
{
	public interface IManageUPFMProductsIntegration
	{
        /// <summary>
        /// Used to update a user roles and rights for IntelligentBuilding
        /// </summary>
        /// <param name="editorPersonaId"></param>
        /// <param name="userPersonaId"></param>
        /// <param name="userAssignProductPropertyRole"></param>
        /// <param name="isEmpAccess"></param>
        /// <param name="additionalParameters"></param>
        /// <returns></returns>
        string ManageUPFMProductUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole, out List<AdditionalParameters> additionalParameters, bool isEmpAccess = false);

		/// <summary>
		/// Used to unassign a user from IntelligentBuilding
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="userAssignProductPropertyRole"></param>
		/// <returns></returns>
		string UnassignUser(long editorPersonaId, long userPersonaId, UPFMProductPropertyRole userAssignProductPropertyRole);

		/// <summary>
		/// Returns Roles for the given user and company
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="partyId"></param>
		/// <param name="productId"></param>
		/// <returns></returns>
		ListResponse GetRoles(long editorPersonaId, long userPersonaId, long partyId);

		/// <summary>
		/// Returns Rights with selected rights for a roleId
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="partyId"></param>
		/// <param name="roleId"></param>
		/// <returns></returns>
		ListResponse GetRightsByRole(long editorPersonaId, long partyId, long roleId);

		/// <summary>
		/// Used to get the list of properties for the company or for the given user
		/// </summary>		
		/// <param name="userPersonaId"></param>	
		/// <param name="product"></param>
		/// <param name="productCode"></param>
		/// <param name="include"></param>
		/// <param name="isMultiCompany"></param>
		/// <param name="multiCompanyRealPageId"></param>
		/// <returns></returns>
		ListResponse GetEnterpriseUPFMProperties(long userPersonaId, int product, string productCode, string include = null, bool isMultiCompany = false,string multiCompanyRealPageId = null);

		/// <summary>
		/// Get a list of UPFM property instances for the give user
		/// </summary>
		/// <param name="editorPersonaId"></param>
		/// <param name="userPersonaId"></param>
		/// <param name="assignedOnly"></param>
		/// <param name="datafilter"></param>
		/// <returns></returns>
		ListResponse GetUPFMProperties(long editorPersonaId, long userPersonaId, bool assignedOnly, RequestParameter datafilter);

		/// <summary>
		/// Get a companyinstanceSourceId of a product
		/// </summary>
		/// <param name="orgRealPageId"></param>
		/// <param name="booksCustmerMasterId"></param>
		/// <param name="blueBookProductName"></param>
		/// <param name="domain"></param>
		/// <param name="includeExtra"></param>
		/// <param name="useTranslate"></param>
		/// <returns></returns>
		string GetProductCompanyInstanceId(Guid orgRealPageId, long booksCustmerMasterId, string blueBookProductName, string domain, string includeExtra = "", bool useTranslate = true);

		/// <summary>
		/// Get multi company propeties of product
		/// </summary>
		/// <param name="productCode"></param>
		/// <returns></returns>
		List<UserCompaniesProperties> GetUPFMMultiCompanyProperties(string productCode);
	}
}
