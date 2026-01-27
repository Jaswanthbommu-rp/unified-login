using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties;
using UnifiedLogin.BusinessLogic.Logic.Helper;
using UnifiedLogin.BusinessLogic.Logic.Product;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UnifiedLogin.SharedObjects.Product.RealConnect;

namespace UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties
{
    /// <summary>
    /// Strategy for handling RealConnect product batches
    /// </summary>
    public class RealConnectBatchStrategy : IProductBatchStrategy
    {
        private readonly ILogger<RealConnectBatchStrategy> _logger;
        private readonly IProductRepository _productRepository;
        private readonly DefaultUserClaim _userClaim;

        public int ProductId => (int)ProductEnum.RealConnect;

        public RealConnectBatchStrategy(
            ILogger<RealConnectBatchStrategy> logger,
            IProductRepository productRepository,
            DefaultUserClaim userClaim)
        {
            _logger = logger;
            _productRepository = productRepository;
            _userClaim = userClaim;
        }

        public async Task<ProductBatch> CreateBatchAsync(ProductBatchContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Processing RealConnect batch for user {SubjectPersonaId}", context.SubjectPersonaId);

            var productBatchRecord = BatchHelper.CreateProductBatchRecord(
                context.PropertiesResponse,
                context.RolesResponse,
                context.ProductId,
                context.UsePrimaryProperties,
                context.IntegrationType);

            // Get RealConnect specific license information
            var positionsToAdd = await GetRealConnectLicensesAsync(
                context.SubjectPersonaId,
                context.EditorPersonaId,
                cancellationToken);

            productBatchRecord.InputJson.RCLicenseDetails = new RCProductBatch
            {
                LearnerLicenseId = positionsToAdd,
                ManagerLicenseId = new List<string>()
            };

            return productBatchRecord;
        }

        private async Task<List<string>> GetRealConnectLicensesAsync(
            long subjectUserPersonaId,
            long editorPersonaId,
            CancellationToken cancellationToken)
        {
            var positionsToAdd = new List<string>();

            // Update user claim for RealConnect service
            var tempPersonaId = _userClaim.PersonaId;
            _userClaim.PersonaId = editorPersonaId;

            try
            {
                var manageProductRealConnect = new ManageProductRealConnect(_userClaim);
                var productAttributes = await Task.Run(() =>
                    _productRepository.ListPersonaProductsSamlDetails(subjectUserPersonaId),
                    cancellationToken);

                if (productAttributes == null)
                {
                    return positionsToAdd;
                }

                var realConnectProduct = productAttributes.FirstOrDefault(p => p.ProductId == ProductId);
                if (realConnectProduct == null)
                {
                    return positionsToAdd;
                }

                // Get learner and license information
                Task<RealConnectUser> learnerInfoTask = realConnectProduct.LearnerId != null
                    ? manageProductRealConnect.GetUser(realConnectProduct.LearnerId)
                    : Task.FromResult<RealConnectUser>(null);

                var clientLicenseInfoTask = manageProductRealConnect
                    .GetClientLicenseDetailsForPanoramaCached(_userClaim.OrganizationPartyId);

                await Task.WhenAll(learnerInfoTask, clientLicenseInfoTask);

                var learnerInfo = await learnerInfoTask;
                var clientLicenseInfo = await clientLicenseInfoTask;

                if (learnerInfo == null || clientLicenseInfo?.Licenses == null)
                {
                    return positionsToAdd;
                }

                var positionLicenses = clientLicenseInfo.Licenses
                    .Where(a => a.Ref1 == "position")
                    .ToList();

                // Add licenses that are already allocated but not in position list
                foreach (var item in learnerInfo.AllocatedLicenses)
                {
                    if (!positionLicenses.Any(a => a.Id == item.LicenseId))
                    {
                        positionsToAdd.Add(item.LicenseId);
                    }
                }

                return positionsToAdd;
            }
            finally
            {
                // Restore original persona ID
                _userClaim.PersonaId = tempPersonaId;
            }
        }
    }
}