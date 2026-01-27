using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnifiedLogin.BusinessLogic.Repository;
using UnifiedLogin.BusinessLogic.Repository.Interfaces;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Product;
using UL = UnifiedLogin.SharedObjects.Product.UnifiedLogin;

namespace UnifiedLogin.BusinessLogic.Logic.EnterpriseRolesProperties
{
    /// <summary>
    /// Strategy for handling Unified Platform product batches
    /// </summary>
    public class UnifiedPlatformBatchStrategy : IProductBatchStrategy
    {
        private readonly ILogger<UnifiedPlatformBatchStrategy> _logger;
        private readonly IBatchProductBulkUpdateRepository _enterpriseRoleProductRepository;
        private readonly IUserRoleRightRepository _userRoleRightRepository;

        public int ProductId => (int)ProductEnum.UnifiedPlatform;

        public UnifiedPlatformBatchStrategy(
            ILogger<UnifiedPlatformBatchStrategy> logger,
            IBatchProductBulkUpdateRepository enterpriseRoleProductRepository,
            IUserRoleRightRepository userRoleRightRepository)
        {
            _logger = logger;
            _enterpriseRoleProductRepository = enterpriseRoleProductRepository;
            _userRoleRightRepository = userRoleRightRepository;
        }

        public async Task<ProductBatch> CreateBatchAsync(ProductBatchContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Processing Unified Platform batch for user {SubjectPersonaId}", context.SubjectPersonaId);

            // Get roles to delete
            var userRolesToDelete = await GetAssignedRolesForPersonaAsync(
                context.ProductId,
                context.SubjectPersonaId,
                context.UserPersona.OrganizationPartyId,
                cancellationToken);

            // Delete existing roles
            var rolesToDelete = userRolesToDelete.Select(p => p.RoleID).ToList();
            foreach (var platformRole in rolesToDelete)
            {
                await Task.Run(() =>
                    _enterpriseRoleProductRepository.UpdateUnifiedPlatFormRole(
                        (int)platformRole,
                        context.EditorPersona.UserId,
                        context.SubjectPersonaId,
                        true),
                    cancellationToken);
            }

            // Insert new roles
            var rolesToInsert = context.ProductRoles.Select(p => int.Parse(p.ID)).ToList();
            foreach (var platformRole in rolesToInsert)
            {
                await Task.Run(() =>
                    _enterpriseRoleProductRepository.UpdateUnifiedPlatFormRole(
                        platformRole,
                        context.EditorPersona.UserId,
                        context.SubjectPersonaId,
                        false),
                    cancellationToken);
            }

            // Return null as batch is handled directly
            return null;
        }

        private async Task<List<UL.Role>> GetAssignedRolesForPersonaAsync(
            int productId,
            long userPersonaId,
            long organizationPartyId,
            CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
                _userRoleRightRepository.ListRoleByPersona(productId, userPersonaId, organizationPartyId),
                cancellationToken);
        }
    }
}
