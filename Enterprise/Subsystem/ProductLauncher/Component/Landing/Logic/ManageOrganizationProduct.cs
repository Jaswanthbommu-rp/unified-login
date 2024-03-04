using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RP.Enterprise.Foundation.DataAccess.Component;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository;
using RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Repository.Interfaces;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Audit.Common;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Constants;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enum;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using Serilog;
using Serilog.Events;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.Landing.Logic
{
	/// <summary>
	/// ManageOrganizationProduct class
	/// </summary>
	public class ManageOrganizationProduct : IManageOrganizationProduct
	{
		#region Private Variables
		IOrganizationProductRepository _organizationProductRepository;
		IManageBlueBook _manageBlueBook;
		IManageProduct _manageProduct;
		IProductRepository _productRepository;

		private DefaultUserClaim _defaultUserClaim;
		#endregion

		#region Constructors
		/// <summary>
		/// Manage Organization Product Constructor (Default)
		/// </summary>
		/// <param name="userClaim"></param>
		public ManageOrganizationProduct(DefaultUserClaim userClaim)
		{
			_organizationProductRepository = new OrganizationProductRepository();
			_defaultUserClaim = userClaim;
			_manageBlueBook = new ManageBlueBook(userClaim);
			_manageProduct = new ManageProduct(userClaim);
			_productRepository = new ProductRepository(userClaim);
		}

        /// <summary>
        /// Create a basic instance of the ManageOrganizationProduct class
        /// </summary>
        /// <param name="userClaim"></param>
        /// <param name="repository"></param>
        /// <param name="manageBlueBook"></param>
        /// <param name="manageProduct"></param>

        public ManageOrganizationProduct(DefaultUserClaim userClaim, IRepository repository, IManageBlueBook manageBlueBook, IManageProduct manageProduct)
		{
			_organizationProductRepository = new OrganizationProductRepository(repository);
			_manageBlueBook = manageBlueBook;
			_manageProduct = manageProduct;
			_productRepository = new ProductRepository(repository, userClaim);
			_defaultUserClaim = userClaim;
		}
		#endregion

		#region Public methods

		/// <summary>
		/// Used to add a list of products to the given company
		/// </summary>
		/// <param name="org"></param>
		/// <param name="productList"></param>
		/// <returns></returns>
		public IRepositoryResponse InsertUpdateOrganizationProduct(Organization org, List<int> productList)
		{
            IRepositoryResponse response = new RepositoryResponse();
			foreach (int product in productList)
			{
				var productInternalSettings = _manageProduct.GetProductInternalSettings((int)product);
				var updateinUDM = productInternalSettings.Where(x => x.Name.ToUpper() == "UPDATEPRODUCTINUDM").FirstOrDefault();

				if (updateinUDM != null && updateinUDM.Value == "1")
				{
					var systemProductCenter = new SystemProductCenter()
					{
						Id = 0,
						CompanyInstanceSourceId = org.RealPageId.ToString().ToLower(),
						CreatedBy = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform) + " Automation",
						ProductCenterSourceId = product.ToString(),
						PropertyInstanceSourceId = null,
						Source = ProductEnumHelper.StringValueOf(ProductEnum.UnifiedPlatform)

					};
					var isUpdated = _manageBlueBook.ProductCenterEnable(systemProductCenter);

					if (!isUpdated)
					{
						response.ErrorMessage = "Unable to update product in UDM";
						return response;
					}
				}

				response = InsertUpdateOrganizationProduct(partyId: org.PartyId, product: product, configurationId: null, fromDate: null, thruDate: null, orgName: org.Name);
				if (!string.IsNullOrEmpty(response.ErrorMessage))
				{
					return response;
				}
			}
			return response;
		}

		/// <summary>
		/// Used to insert a new product to an Organization
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="product"></param>
		/// <param name="configurationId"></param>
		/// <param name="fromDate"></param>
		/// <param name="thruDate"></param>
		/// <param name="orgName"></param>
		/// <returns></returns>
		public IRepositoryResponse InsertUpdateOrganizationProduct(long partyId, int product, int? configurationId, DateTime? fromDate, DateTime? thruDate, string orgName)
		{
			var response = _organizationProductRepository.InsertUpdateOrganizationProduct(partyId, product, configurationId, fromDate, thruDate);

			if (response.ErrorMessage.Length == 0)
			{
				var products = _productRepository.GetAllProducts();
				var productName = products.FirstOrDefault(p => p.ProductId == (int)product)?.Name;
				var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} enabled {productName} for {orgName}";
				LogAuditActivity(LogActivityTypeConstants.PRODUCT_ENABLED_FOR_COMPANY, LogActivityCategoryType.CompanySetup, message);
			}
			
			return response;
		}

		/// <summary>
		/// Used to insert a new product to an Organization from provisioning
		/// </summary>
		/// <param name="product"></param>
		/// <param name="configurationId"></param>
		/// <param name="fromDate"></param>
		/// <param name="thruDate"></param>
		/// <param name="org"></param>
		/// <returns></returns>
		public IRepositoryResponse InsertUpdateOrganizationProductFromProvisioning(int product, int? configurationId, DateTime? fromDate, DateTime? thruDate, Organization org)
		{
			var response = _organizationProductRepository.InsertUpdateOrganizationProduct(org.PartyId, product, configurationId, fromDate, thruDate);

			if (response.ErrorMessage.Length == 0)
            {
				var products = _productRepository.GetAllProducts();
				var productName = products.FirstOrDefault(p => p.ProductId == (int)product)?.Name;
				var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} enabled {productName} for {org.Name}";
				LogAuditActivity(LogActivityTypeConstants.PRODUCT_ENABLED_FOR_COMPANY, LogActivityCategoryType.CompanySetup, message);
			}

			return response;
		}

		/// <summary>
		/// Used to delete a product from an Organization
		/// </summary>
		/// <param name="partyId"></param>
		/// <param name="product"></param>
		/// <param name="org"></param>
		/// <returns></returns>
		public IRepositoryResponse DeleteOrganizationProduct(long partyId, ProductEnum product, Organization org)
		{
			var response = _organizationProductRepository.DeleteOrganizationProduct(partyId, product);

			if (response.ErrorMessage.Length == 0)
			{
				var products = _productRepository.GetAllProducts();
				var productName = products.FirstOrDefault(p => p.ProductId == (int)product)?.Name;
				var message = $"{_defaultUserClaim.FirstName} {_defaultUserClaim.LastName} disabled {productName} for {org.Name}";
				LogAuditActivity(LogActivityTypeConstants.PRODUCT_DISABLED_FOR_COMPANY, LogActivityCategoryType.CompanySetup, message);
			}

			return response;
		}

		/// <summary>
		/// Used to delete users for product for an Organization
		/// </summary>
		/// <param name="partyId">The organization id for the product</param>
		/// <param name="product">The product Id</param>
		/// <returns></returns>
		public IRepositoryResponse DisableUsersForProduct(long partyId, ProductEnum product)
		{
			return _organizationProductRepository.DisableUsersForProduct(partyId, product);
		}

		private void LogAuditActivity(string logActivityType, LogActivityCategoryType logActivityCategoryType, string message)
		{
			try
			{
				LogActivity.WriteActivity(new ActivityDetails
				{
					LogActivityTypeName = logActivityType,
					LogCategoryName = logActivityCategoryType.ToString(),
					CorrelationId = _defaultUserClaim.CorrelationId.ToString(),
					BooksMasterOrganizationId = _defaultUserClaim.OrganizationMasterId,
					OrganizationPartyId = _defaultUserClaim.OrganizationPartyId,
					Message = message,

					FromUserLoginName = _defaultUserClaim.LoginName,
					FromUserLoginId = _defaultUserClaim.UserId,
					FromUserRealpageId = _defaultUserClaim.UserRealPageGuid.ToString(),
					FromUserFirstName = _defaultUserClaim.FirstName,
					FromUserLastName = _defaultUserClaim.LastName,

					ToUserLoginName = null,
					ToUserLoginId = null,
					ToUserFirstName = null,
					ToUserLastName = null,
					ToUserRealpageId = null
				});
			}
			catch (Exception ex)
            {
                WriteToLog(LogEventLevel.Error, "{methodName} - {state}", exception: ex, messageProperties: new object[] { "LogAuditActivity", $"Error while adding activity message. BooksMasterOrganizationId{_defaultUserClaim.OrganizationName}, author user login name {_defaultUserClaim.LoginName}" });
            }
		}

        /// <summary>
        /// Used to write to the central log
        /// </summary>
        /// <param name="logType">Log Type</param>
        /// <param name="message">Message template</param>
        /// <param name="logData">Dictionary of additional properties to log</param>
        /// <param name="exception">Exception details</param>
        /// <param name="messageProperties">Message properties</param>
        private void WriteToLog(LogEventLevel logType, string message, Dictionary<string, object> logData = null, Exception exception = null, object[] messageProperties = null)
        {
			try
			{
				string correlationId = "";
				if (_defaultUserClaim != null)
				{
					correlationId = (_defaultUserClaim.CorrelationId != Guid.Empty) ? _defaultUserClaim.CorrelationId.ToString() : "";
				}
				var logger = Log.Logger;
				if (logData?.Keys != null)
				{
					logger = logger.ForContext("AdditionalInfo", JsonConvert.SerializeObject(logData, Formatting.Indented), false);
				}
				logger = logger.ForContext("ProductModule", this.GetType());
				logger = logger.ForContext("CorrelationId", correlationId);

                logger.Write(level: logType, exception: exception, messageTemplate: message, propertyValues: messageProperties);
            }
			catch
			{
				/*ignored*/
			}
		}
		#endregion
	}
}