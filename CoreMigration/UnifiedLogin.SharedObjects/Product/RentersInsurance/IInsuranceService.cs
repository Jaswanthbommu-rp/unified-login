namespace UnifiedLogin.SharedObjects.Product.RentersInsurance;


public interface IInsuranceService
{
    string Url { get; set; }
    bool UseDefaultCredentials { get; set; }

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/ValidateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    bool ValidateUser(string User, string Password);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/ValidateUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<bool> ValidateUserAsync(string User, string Password);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/ValidateUserDetails", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.GetUserByIDResponse ValidateUserDetails(RentersInsurance.ValidUserReq ValidateUser);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/ValidateUserDetails", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.GetUserByIDResponse> ValidateUserDetailsAsync(RentersInsurance.ValidUserReq ValidateUser);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/MigrateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    string MigrateUser(RentersInsurance.MigrateUserrequest[] aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/MigrateUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<string> MigrateUserAsync(RentersInsurance.MigrateUserrequest[] aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.UserInfo GetUser(string User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.UserInfo> GetUserAsync(string User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/ChangeClaimStatus", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    void ChangeClaimStatus(string SystemIdentifier, bool IsLinked);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/ChangeClaimStatus", ReplyAction = "*")]
    System.Threading.Tasks.Task ChangeClaimStatusAsync(string SystemIdentifier, bool IsLinked);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetUserByLogin", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.GetUserByIDResponse GetUserByLogin(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetUserByLogin", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.GetUserByIDResponse> GetUserByLoginAsync(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetUserByID", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.GetUserByIDResponse GetUserByID(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetUserByID", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.GetUserByIDResponse> GetUserByIDAsync(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/AddUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.UserAPIResponse AddUser(RentersInsurance.AddUpdateUserRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/AddUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.UserAPIResponse> AddUserAsync(RentersInsurance.AddUpdateUserRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/UpdateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.UserAPIResponse UpdateUser(RentersInsurance.AddUpdateUserRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/UpdateUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.UserAPIResponse> UpdateUserAsync(RentersInsurance.AddUpdateUserRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/UnlockUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.UserAPIResponse UnlockUser(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/UnlockUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.UserAPIResponse> UnlockUserAsync(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/DisableUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.UserAPIResponse DisableUser(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/DisableUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.UserAPIResponse> DisableUserAsync(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/EnableUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.UserAPIResponse EnableUser(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/EnableUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.UserAPIResponse> EnableUserAsync(RentersInsurance.UserActionRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/UpdateUserPassword", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.UserAPIResponse UpdateUserPassword(RentersInsurance.UpdatePasswordRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/UpdateUserPassword", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.UserAPIResponse> UpdateUserPasswordAsync(RentersInsurance.UpdatePasswordRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/CheckUserLogin", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.ErrorInfoType CheckUserLogin(RentersInsurance.CheckUserLoginExists UserLogin);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/CheckUserLogin", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.ErrorInfoType> CheckUserLoginAsync(RentersInsurance.CheckUserLoginExists UserLogin);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetListOfUserRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.ListOfUserRolesResponse GetListOfUserRoles();

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetListOfUserRoles", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.ListOfUserRolesResponse> GetListOfUserRolesAsync();

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetListOfPMC", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.ListOfPMCResponse GetListOfPMC();

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetListOfPMC", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.ListOfPMCResponse> GetListOfPMCAsync();

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetUsersByPMC", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.ListOfUserResponse GetUsersByPMC(RentersInsurance.UserActionByPMCIDRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetUsersByPMC", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.ListOfUserResponse> GetUsersByPMCAsync(RentersInsurance.UserActionByPMCIDRequest aRequest);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetListPropertyByPMCID", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [System.ServiceModel.ServiceKnownTypeAttribute(typeof(AuthenticationType))]
    RentersInsurance.ListPropertyByPMCIDResponse GetListPropertyByPMCID(int PMCID);

    [System.ServiceModel.OperationContractAttribute(Action = "http://webservices.leasingdesk.com/GetListPropertyByPMCID", ReplyAction = "*")]
    System.Threading.Tasks.Task<RentersInsurance.ListPropertyByPMCIDResponse> GetListPropertyByPMCIDAsync(int PMCID);
}