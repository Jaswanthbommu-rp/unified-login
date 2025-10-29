namespace UnifiedLogin.SharedObjects.Product.OneSite;

public interface IOneSiteProductService
{
    Task<bool> ValidateUserAsync(NameValuePair[] user, string password);
    Task<UserExists> CheckIfUserIDIsUsedAsync(NameValuePair[] uiArgs);
    Task<PMCInfo> GetPMCUrlAsync(int pmcId);
    Task<CreateSuperuserResponse> CreateSuperuserAsync(NameValuePair[] user);
    Task<CreateUserResponse> CreateUserAsync(NameValuePair[] user);
    Task<UpdateSuperuserResponse> UpdateSuperuserAsync(string systemIdentifier, NameValuePair[] user);
    Task<UpdateUserResponse> UpdateUserAsync(string systemIdentifier, NameValuePair[] user);
    Task<AssignStatus> AssignPropertiesToUserAsync(string systemIdentifier, string selectedPropertyIds);
    Task<AssignStatus> RemovePropertiesFromUserAsync(string systemIdentifier, string propertyIdsToRemove);
    Task<AssignStatus> AssignRolesToUserAsync(string systemIdentifier, string selectedRoleIds);
    Task<AssignStatus> RemoveRolesFromUserAsync(string systemIdentifier, string roleIdsToRemove);
    Task EnableUserAsync(string systemIdentifier);
    Task ResetVerificationCodeAsync(string systemIdentifier);
    Task DisableUserAsync(string systemIdentifier);
    Task<PropertyList> GetAllPropertiesAsync(NameValuePair[] uiArgs, string systemIdentifier, FilterSortParameters filterSortParameters);
    Task<PropertyList> GetDefaultPropertiesAsync(NameValuePair[] uiArgs, FilterSortParameters filterSortParameters);
    Task<PropertyList> GetUserPropertiesAsync(string systemIdentifier, bool assignedOnly, FilterSortParameters filterSortParameters);
    Task<UserList> GetUsersForPropertyAsync(string systemIdentifier, int propertyId, bool assignedOnly, FilterSortParameters filterSortParameters);
    Task<RoleList> GetAllRolesAsync(NameValuePair[] uiArgs, string systemIdentifier, FilterSortParameters filterSortParameters);
    Task<RoleList> GetDefaultRolesAsync(NameValuePair[] uiArgs, FilterSortParameters filterSortParameters);
    Task<RoleList> GetUserRolesAsync(string systemIdentifier, bool assignedOnly, FilterSortParameters filterSortParameters);
    Task<UserList> GetUsersForRoleAsync(string systemIdentifier, int roleId, bool assignedOnly, FilterSortParameters filterSortParameters);
    Task DeleteUserAsync(string systemIdentifier, string deleteUserSystemIdentifier);
    Task ClaimUserAsync(string systemIdentifier, bool isLinked);
    Task ClaimUserULAsync(string systemIdentifier, bool isUlLinked);
    Task<GetUserResponse> GetUserAsync(NameValuePair[] user);
    Task<RightCenter> GetRightsCentersListAsync(NameValuePair[] uiArgs, string systemIdentifier);
    Task<RightList> GetRightsListAsync(NameValuePair[] uiArgs, string systemIdentifier, FilterSortParameters filterSortParameters);
    Task<AssignStatus> ModifyRightToRolesAsync(string systemIdentifier, int rightId, string selectedRoleIds, bool assignRight);
    Task<AssignStatus> ModifyRoleToRightsAsync(string systemIdentifier, int roleId, string rightIdsToAdd, string rightIdsToRemove);
    Task<RoleList> GetRolesForRightAsync(NameValuePair[] uiArgs, int rightId, bool assignedOnly, FilterSortParameters filterSortParameters);
    Task<RoleList> AddUpdateRoleAsync(string systemIdentifier, string roleId, string roleName, string inheritRoleId);
    Task DeleteRoleAsync(string systemIdentifier, int roleId);
    Task<bool> GetUserInLeasingAgentListAsync(string systemIdentifier, int siteId);
    Task<bool> AuthenticateAsync(string username, string password, string additionalInfo);

}