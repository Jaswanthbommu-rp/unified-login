namespace UnifiedLogin.SharedObjects.Product.RentersInsurance
{
    public interface IInsuranceService
    {
        string Url { get; set; }
        bool UseDefaultCredentials { get; set; }

        event AddUserCompletedEventHandler AddUserCompleted;
        event ChangeClaimStatusCompletedEventHandler ChangeClaimStatusCompleted;
        event DisableUserCompletedEventHandler DisableUserCompleted;
        event EnableUserCompletedEventHandler EnableUserCompleted;
        event GetListOfPMCCompletedEventHandler GetListOfPMCCompleted;
        event GetListOfUserRolesCompletedEventHandler GetListOfUserRolesCompleted;
        event GetListPropertyByPMCIDCompletedEventHandler GetListPropertyByPMCIDCompleted;
        event GetUserByIDCompletedEventHandler GetUserByIDCompleted;
        event GetUserCompletedEventHandler GetUserCompleted;
        event GetUserByLoginCompletedEventHandler GetUserByLoginCompleted;
        event GetUsersByPMCCompletedEventHandler GetUsersByPMCCompleted;
        event MigrateUserCompletedEventHandler MigrateUserCompleted;
        event UnlockUserCompletedEventHandler UnlockUserCompleted;
        event UpdateUserCompletedEventHandler UpdateUserCompleted;
        event UpdateUserPasswordCompletedEventHandler UpdateUserPasswordCompleted;
        event ValidateUserCompletedEventHandler ValidateUserCompleted;
        event ValidateUserDetailsCompletedEventHandler ValidateUserDetailsCompleted;

        UserAPIResponse AddUser(AddUpdateUserRequest aRequest);
        void AddUserAsync(AddUpdateUserRequest aRequest);
        void AddUserAsync(AddUpdateUserRequest aRequest, object userState);
        void CancelAsync(object userState);
        void ChangeClaimStatus(string SystemIdentifier, bool IsLinked);
        void ChangeClaimStatusAsync(string SystemIdentifier, bool IsLinked);
        void ChangeClaimStatusAsync(string SystemIdentifier, bool IsLinked, object userState);
        UserAPIResponse DisableUser(UserActionRequest aRequest);
        ErrorInfoType CheckUserLogin(CheckUserLogin UserLogin);
        void DisableUserAsync(UserActionRequest aRequest);
        void DisableUserAsync(UserActionRequest aRequest, object userState);
        UserAPIResponse EnableUser(UserActionRequest aRequest);
        void EnableUserAsync(UserActionRequest aRequest);
        void EnableUserAsync(UserActionRequest aRequest, object userState);
        ListOfPMCResponse GetListOfPMC();
        void GetListOfPMCAsync();
        void GetListOfPMCAsync(object userState);
        ListOfUserRolesResponse GetListOfUserRoles();
        void GetListOfUserRolesAsync();
        void GetListOfUserRolesAsync(object userState);
        ListPropertyByPMCIDResponse GetListPropertyByPMCID(int PMCID);
        void GetListPropertyByPMCIDAsync(int PMCID);
        void GetListPropertyByPMCIDAsync(int PMCID, object userState);
        UserInfo GetUser(string User);
        void GetUserAsync(string User);
        void GetUserAsync(string User, object userState);
        GetUserByIDResponse GetUserByID(UserActionRequest aRequest);
        void GetUserByIDAsync(UserActionRequest aRequest);
        void GetUserByIDAsync(UserActionRequest aRequest, object userState);

        GetUserByIDResponse GetUserByLogin(UserActionRequest aRequest);
        void GetUserByLoginAsync(UserActionRequest aRequest);
        void GetUserByLoginAsync(UserActionRequest aRequest, object userState);

        ListOfUserResponse GetUsersByPMC(UserActionByPMCIDRequest aRequest);
        void GetUsersByPMCAsync(UserActionByPMCIDRequest aRequest);
        void GetUsersByPMCAsync(UserActionByPMCIDRequest aRequest, object userState);

        string MigrateUser(MigrateUserrequest[] aRequest);
        void MigrateUserAsync(MigrateUserrequest[] aRequest);
        void MigrateUserAsync(MigrateUserrequest[] aRequest, object userState);

        UserAPIResponse UnlockUser(UserActionRequest aRequest);
        void UnlockUserAsync(UserActionRequest aRequest);
        void UnlockUserAsync(UserActionRequest aRequest, object userState);
        UserAPIResponse UpdateUser(AddUpdateUserRequest aRequest);
        void UpdateUserAsync(AddUpdateUserRequest aRequest);
        void UpdateUserAsync(AddUpdateUserRequest aRequest, object userState);
        UserAPIResponse UpdateUserPassword(UpdatePasswordRequest aRequest);
        void UpdateUserPasswordAsync(UpdatePasswordRequest aRequest);
        void UpdateUserPasswordAsync(UpdatePasswordRequest aRequest, object userState);
        bool ValidateUser(string User, string Password);
        void ValidateUserAsync(string User, string Password);
        void ValidateUserAsync(string User, string Password, object userState);

        GetUserByIDResponse ValidateUserDetails(ValidUserReq ValidateUser1);
        void ValidateUserDetailsAsync(ValidUserReq ValidateUser1);
        void ValidateUserDetailsAsync(ValidUserReq ValidateUser1, object userState);
    }
}