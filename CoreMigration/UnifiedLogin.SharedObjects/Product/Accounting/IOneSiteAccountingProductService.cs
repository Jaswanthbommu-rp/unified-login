namespace UnifiedLogin.SharedObjects.Product.Accounting
{
    /// <summary>
    /// Used to call the Accounting web services to create/edit users
    /// </summary>
	public interface IOneSiteAccountingProductService
	{
        Task<GetUserResponse> GetUserAsync(User[] user);
        Task<GetUserRolesResponse> GetUserRolesAsync(GetUserRolesRequest request);
        Task<GetAllUsersResponse> GetAllUsersAsync(GetAllUsersRequest request);
        Task<GetAllRolesResponse> GetAllRolesAsync(GetAllRolesRequest request);
        Task<GetAllPropertiesResponse> GetAllPropertiesAsync(GetAllPropertiesRequest request);
        Task<GetAllPropertyGroupsResponse> GetAllPropertyGroupsAsync(GetAllPropertyGroupsRequest request);
        Task<GetAllPropertyGroupMembersResponse> GetAllPropertyGroupMembersAsync(GetAllPropertyGroupMembersRequest request);
        Task<GetUserPropertiesResponse> GetUserPropertiesAsync(GetUserPropertiesRequest request);
        Task<getCompaniesAPIResponse> GetCompaniesApiAsync(Company[] companies);
        Task<getPropertiesAPIResponse> GetPropertiesApiAsync(Company[] companies);
        Task<ValidateUserResponse> ValidateUserAsync(NameValuePair[] user, string password);
        Task<CreateUserResponse> CreateUserAsync(NameValuePair[] user);
        Task<UpdateUserResponse> UpdateUserAsync(NameValuePair[] user);
        Task<UpdateUserDetailsResponse> UpdateUserDetailsAsync(NameValuePair[] user);
        Task<DeleteUserResponse> DeleteUserAsync(NameValuePair[] user);
        Task<EnableUserResponse> EnableUserAsync(NameValuePair[] user);
        Task<EnableGreenBookUserResponse> EnableGreenBookUserAsync(NameValuePair[] user);
        Task<EnablePortalUserResponse> EnablePortalUserAsync(NameValuePair[] user);
        Task<DisableUserResponse> DisableUserAsync(NameValuePair[] user);
        Task<DisableGreenBookUserResponse> DisableGreenBookUserAsync(NameValuePair[] user);
        Task<DisablePortalUserResponse> DisablePortalUserAsync(NameValuePair[] user);
        Task ChangeClaimStatusAsync(string systemIdentifier, bool isLinked, string login, string password, string federatedId);
        Task<CheckIfUserIDIsUsedResponse> CheckIfUserIDIsUsedAsync(NameValuePair[] user);
        Task<AssignPropertiesToUserResponse> AssignPropertiesToUserAsync(NameValuePair[] user);
        Task<RemovePropertiesFromUserResponse> RemovePropertiesFromUserAsync(NameValuePair[] user);
        Task<AssignRolesToUserResponse> AssignRolesToUserAsync(NameValuePair[] user);
        Task<RemoveRolesFromUserResponse> RemoveRolesFromUserAsync(NameValuePair[] user);
        Task<GetApplicationsResponse> GetApplicationsAsync(Applications[] applications);
        Task<GetApplicationPermissionsResponse> GetApplicationPermissionsAsync(Permissions[] permissions);
        Task<GetRolePermissionsResponse> GetRolePermissionsAsync(Permissions[] permissions);
        Task<GetPermissionRolesResponse> GetPermissionRolesAsync(Permissions[] permission);
        Task<CreateRoleResponse> CreateRoleAsync(NameValuePair[] role);
        Task<DeleteRoleResponse> DeleteRoleAsync(NameValuePair[] role);
        Task<AssignRolePermissionsResponse> AssignRolePermissionsAsync(User[] user, RolePermission[] rolePermissions);
    }
}