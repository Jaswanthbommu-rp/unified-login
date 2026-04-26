namespace UnifiedLogin.BusinessLogic.Logic.ProductIntegration.Model
{
    public enum ProductEntityEndpointKeyEnum
    {
        GetRoleEndpoint,
        GetRightEndpoint,
        /// <summary>Plural form — used by DepositAlternativeManagement and PortfolioManagement.</summary>
        GetPropertyGroupsEndpoint,
        /// <summary>Singular form — used by StandardV1ProductIntegration base class.</summary>
        GetPropertyGroupEndpoint,
        GetUserEndpoint,
        GetUserRoleEndpoint,
        PostUserEndpoint,
        PutUserEndpoint,
        DeleteUserEndpoint,
        PatchProfileEndpoint,
        GetPropertyEndpoint,
        GetPropertyByGroupEndpoint,
        /// <summary>Used by PortfolioManagement for property-by-group lookups.</summary>
        GetPropertyByGroupsEndpoint,
        GetPropertiesByGroupEndpoint,
        GetListUsersEndpoint,
        PatchMigrateUsersEndpoint,
        GetAllUsers,
        GetCompanyEndpoint,
        GetParentCompanyEndpoint,
        GetUserGroupEndpoint,
        GetUserExistEndpoint,
        GetRoleRightsEndpoint,
        GetRightsEndpoint
    }
}