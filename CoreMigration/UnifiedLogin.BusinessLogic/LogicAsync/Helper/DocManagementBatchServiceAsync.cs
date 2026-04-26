using Microsoft.Extensions.Logging;
using UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;
using UnifiedLogin.SharedObjects.Landing;
using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Product;

namespace UnifiedLogin.BusinessLogic.LogicAsync.Helper;

/// <summary>
/// Builds <see cref="ProductBatch"/> records for RP Document Management.
/// <para>
/// Replaces <c>BatchHelper.CreateDocManagementBatchRecords(DefaultUserClaim, …)</c>.
/// Uses <see cref="IManageProductRPDocumentManagementAsync"/> instead of inline
/// <c>new ManageProductRPDocumentManagement(userClaim)</c>.
/// </para>
/// <para><b>DI registration:</b> <c>Scoped</c>.</para>
/// </summary>
public sealed class DocManagementBatchServiceAsync : IDocManagementBatchServiceAsync
{
    private readonly IManageProductRPDocumentManagementAsync _rpDocManagement;
    private readonly ILogger<DocManagementBatchServiceAsync> _logger;

    public DocManagementBatchServiceAsync(
        IManageProductRPDocumentManagementAsync rpDocManagement,
        ILogger<DocManagementBatchServiceAsync> logger)
    {
        _rpDocManagement = rpDocManagement ?? throw new ArgumentNullException(nameof(rpDocManagement));
        _logger          = logger          ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ProductBatch> CreateDocManagementBatchRecordAsync(
        long editorPersonaId,
        long subjectPersonaId,
        bool usePrimaryProperties,
        CancellationToken cancellationToken = default)
    {
        var inputJson = new RolePropertyList { IsAssigned = true };
        var lstRoleProperties = new List<PAMRolePropertyList>();

        // Fetch the assigned roles for the subject persona
        var rolesResult = await _rpDocManagement.GetPropertyRolesAsync(
            editorPersonaId, subjectPersonaId, null!, cancellationToken).ConfigureAwait(false);

        if (rolesResult?.Records is { Count: > 0 })
        {
            var assignedRoles = rolesResult.Records
                .Cast<SharedObjects.Product.ProductRole>()
                .Where(r => r.IsAssigned)
                .ToList();

            foreach (var role in assignedRoles)
            {
                var objRole = new PAMRolePropertyList { RoleId = role.ID };

                // Roles with a Roletype carry additional classifier properties
                if (role.Roletype is not null)
                {
                    var classifierResult = await _rpDocManagement.GetRoleClassifierDatasetAsync(
                        editorPersonaId, subjectPersonaId, role.ID, null!, cancellationToken)
                        .ConfigureAwait(false);

                    if (classifierResult?.Records is { Count: > 0 })
                    {
                        objRole.PropertyIds = classifierResult.Records
                            .Cast<ProductProperty>()
                            .Where(p => p.IsAssigned == true)
                            .Select(p => p.ID)
                            .ToList();
                    }
                }

                lstRoleProperties.Add(objRole);
            }

            inputJson.RolePropertiesList = lstRoleProperties;
        }

        inputJson.UsePrimaryProperties = usePrimaryProperties;

        return new ProductBatch
        {
            ProductId    = (int)ProductEnum.RPDocumentManagement,
            StatusTypeId = 5,
            RetryCount   = 0,
            InputJson    = inputJson
        };
    }
}
