using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Interfaces;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties
{
    /// <summary>
    /// Strategy for handling Admin Support Portal product batches
    /// </summary>
    public class AdminSupportPortalBatchStrategy : IProductBatchStrategy
    {
        private readonly ILogger<AdminSupportPortalBatchStrategy> _logger;
        private readonly IProductRepository _productRepository;
        private readonly IManageProductBatch _manageProductBatch;
        private readonly DefaultUserClaim _userClaim;

        public int ProductId => (int)ProductEnum.AdminSupportPortal;

        public AdminSupportPortalBatchStrategy(
            ILogger<AdminSupportPortalBatchStrategy> logger,
            IProductRepository productRepository,
            IManageProductBatch manageProductBatch,
            DefaultUserClaim userClaim)
        {
            _logger = logger;
            _productRepository = productRepository;
            _manageProductBatch = manageProductBatch;
            _userClaim = userClaim;
        }

        public async Task<ProductBatch> CreateBatchAsync(ProductBatchContext context, CancellationToken cancellationToken = default)
        {
            if (context.UsePrimaryProperties)
            {
                // Use standard batch creation
                return await _manageProductBatch.GetProductBatchRecordAsync(
                        context.EditorPersonaId,
                        context.SubjectPersonaId,
                        context.ProductRoles,
                        context.PropertiesResponse,
                        context.RolesResponse,
                        context.ProductId,
                        context.UsePrimaryProperties);
            }

            // Special handling for non-primary properties
            _logger.LogDebug("Processing Admin Support Portal with custom property logic");

            var manageProductAdminSupportPortal = new ManageProductAdminSupportPortal(_userClaim);
            var userProperties = manageProductAdminSupportPortal.GetProperties(
                context.EditorPersonaId,
                context.SubjectPersonaId,
                null);

            var productAttributes = await Task.Run(() =>
                _productRepository.ListPersonaProductsSamlDetails(context.SubjectPersonaId),
                cancellationToken);

            if (productAttributes == null)
            {
                return null;
            }

            var adminProduct = productAttributes.FirstOrDefault(p => p.ProductId == ProductId);
            if (adminProduct == null || userProperties?.Records == null)
            {
                return null;
            }

            var assignedProperties = userProperties.Records
                .Cast<ProductProperty>()
                .Where(c => c.IsAssigned == true)
                .ToList();

            if (adminProduct.ProductStatus.Equals("success", StringComparison.OrdinalIgnoreCase) && assignedProperties.Any())
            {
                userProperties.Records = assignedProperties.Cast<object>().ToList();
                var isAllProperties = CheckForAllProperties(userProperties.Additional);

                var updatedPropertiesResponse = BatchHelper.GetUserAssignedPropertiesData(userProperties);

                return await _manageProductBatch.GetProductBatchRecordAsync(
                        context.EditorPersonaId,
                        context.SubjectPersonaId,
                        context.ProductRoles,
                        updatedPropertiesResponse,
                        context.RolesResponse,
                        context.ProductId,
                        context.UsePrimaryProperties);
            }

            return null;
        }

        private bool CheckForAllProperties(object additionalInfo)
        {
            if (additionalInfo?.GetType().Name.Equals("STRING", StringComparison.OrdinalIgnoreCase) == true)
            {
                return false;
            }

            if (additionalInfo is Dictionary<string, bool> additionalDataCollection)
            {
                return additionalDataCollection.TryGetValue("allProperties", out var value) && value;
            }

            return false;
        }
    }
}