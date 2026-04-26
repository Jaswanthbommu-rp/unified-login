namespace UnifiedLogin.BusinessLogic.LogicAsync.Interfaces;

public interface IManageEnterpriseRolesPrimaryPropertiesAsync
{
    Task<string> ProcessEnterpriseRolesAndPrimaryPropertiesDataAsync(
        long editorUserPersonaId,
        long subjectUserPersonaId,
        int? enterpriseRoleTemplateId = null,
        DateTime? createdDateTime = null,
        int batchProcessTypeId = 0,
        bool isUnassignAllProducts = false,
        CancellationToken cancellationToken = default);
}
