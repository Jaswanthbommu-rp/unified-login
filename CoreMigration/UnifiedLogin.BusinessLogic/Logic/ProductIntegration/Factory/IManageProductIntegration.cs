using System.Collections.Generic;
using UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model;
using UnifiedLogin.SharedObjects.Audit.Common;
using UnifiedLogin.SharedObjects.Base;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product.Migration;

namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Factory
{
	/// <summary>
	/// Product Integration Interface  should be implemented on product specific class
	/// Used in Factory
	/// </summary>
	public interface IManageProductIntegration
	{
		/// <summary>
		/// Get Product Roles
		/// </summary>
		ListResponse GetProductRoles(RequestParameter datafilter, string baseUrlAndQuery = null);

        /// <summary>
		/// Get Roles based Rights
		/// </summary>
        ListResponse GetProductRightsForRole(RequestParameter dataFilter, string roleId, string baseUrlAndQuery = null);

        /// <summary>
        /// Get Product Properties
        /// </summary>
        ListResponse GetProductProperties(RequestParameter datafilter, string baseUrlAndQuery = null);

		/// <summary>
		/// Get Product Regions
		/// </summary>
		ListResponse GetProductPropertyGroups(RequestParameter datafilter, string baseUrlAndQuery = null);

		/// <summary>
		/// UnassignUser
		/// </summary>
		/// <returns>Empty string if success</returns>
		string UnassignUser();

		/// <summary>
		/// Change User type - reg to admin and vice versa
		/// </summary>
		/// <returns>Empty string if success</returns>
		string ChangeProductUserType(ProductUserRolePropertiesGroups rolePropertiesGroups, BatchProcessType batchProcessType);

		/// <summary>
		/// Create or Update ProductUser based on IsAssigned flag in productUserRolePropertiesGroups
		/// </summary>
		/// <returns>Empty string if success</returns>
		string CreateUpdateProductUser(ProductUserRolePropertiesGroups productUserRolePropertiesRegion, out List<AdditionalParameters> additionalParameters, BatchProcessType batchProcessType = BatchProcessType.CreateUpdateProductUser);

		/// <summary>
		/// Get Product Properties
		/// </summary>
		ListResponse GetProductPropertiesByGroup(string groupId, RequestParameter datafilter, string baseUrlAndQuery = null);

		/// <summary>
		/// Update Product User Profile information (called from green-book)
		/// </summary>
		/// <returns>Empty string if success</returns>
		string UpdateProductUserProfile();

		/// <summary>
		/// Get product user
		/// </summary>
		/// <param name="baseUrlAndQuery"></param>
		IntegrationProductUser GetProductUser(string baseUrlAndQuery = null, bool isThrowOnError = true);

		/// <summary>
		/// For a product, returns all organizations or by given organizationId -used in ClickPay
		/// </summary>
		ListResponse GetProductOrganizations(string organizationRoleId, string organizationType, string baseUrlAndQuery = null);

		/// <summary>
		/// Gets the migration users.
		/// </summary>
		/// <param name="datafilter">The datafilter.</param>
		/// <returns></returns>
		ListResponse GetMigrationUsers(RequestParameter dataFilter);

		/// <summary>
		/// Updates the users migration status.
		/// </summary>
		/// <param name="migrateUsers">The migrate users.</param>
		/// <returns></returns>
		MigrateResponse UpdateUsersMigrationStatus(IList<MigrateUser> migrateUsers);

		/// <summary>
		/// Direct call to product to change profile including isActive (mainly used to activate-deactivate from Migration tool)
		/// </summary>
		/// <param name="productUserProfile">Product user information</param>
		/// <returns>string.Empty if success else response contents.</returns>
		bool ExternalProductUserProfileChange(ProductUserProfile productUserProfile);

		/// <summary>
		/// Returns Product Rights for a Company
		/// </summary>
		/// <param name="dataFilter">Request parameters</param>
		/// <param name="baseUrlAndQuery">Base url</param>
		/// <returns>A response list</returns>
		ListResponse GetAllRights(RequestParameter dataFilter, string baseUrlAndQuery = null);

		/// <summary>
		/// Getting user groups by ProductId.
		/// </summary>
		/// <param name="dataFilter"></param>
		/// <param name="baseUrlAndQuery"></param>
		/// <returns></returns>
		ListResponse GetProductUserGroups(RequestParameter dataFilter, string baseUrlAndQuery = null);
	}
}