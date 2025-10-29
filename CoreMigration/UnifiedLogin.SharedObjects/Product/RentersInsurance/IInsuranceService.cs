namespace UnifiedLogin.SharedObjects.Product.RentersInsurance;

public interface IInsuranceService
{
    Task<bool> ValidateUserAsync(string user, string password);
    Task<GetUserByIDResponse> ValidateUserDetailsAsync(ValidUserReq validateUserRequest);
    Task<string> MigrateUserAsync(MigrateUserrequest[] requests);
    Task<UserInfo> GetUserAsync(string user);
    Task ChangeClaimStatusAsync(string systemIdentifier, bool isLinked);
    Task<GetUserByIDResponse> GetUserByLoginAsync(UserActionRequest request);
    Task<GetUserByIDResponse> GetUserByIDAsync(UserActionRequest request);
    Task<UserAPIResponse> AddUserAsync(AddUpdateUserRequest request);
    Task<UserAPIResponse> UpdateUserAsync(AddUpdateUserRequest request);
    Task<UserAPIResponse> UnlockUserAsync(UserActionRequest request);
    Task<UserAPIResponse> DisableUserAsync(UserActionRequest request);
    Task<UserAPIResponse> EnableUserAsync(UserActionRequest request);
    Task<UserAPIResponse> UpdateUserPasswordAsync(UpdatePasswordRequest request);
    Task<ErrorInfoType> CheckUserLoginAsync(CheckUserLoginExists request);
    Task<ListOfUserRolesResponse> GetListOfUserRolesAsync();
    Task<ListOfPMCResponse> GetListOfPMCAsync();
    Task<ListOfUserResponse> GetUsersByPMCAsync(UserActionByPMCIDRequest request);
    Task<ListPropertyByPMCIDResponse> GetListPropertyByPMCIDAsync(int pmcId);
}