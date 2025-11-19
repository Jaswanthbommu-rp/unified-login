using System.Net;
using System.ServiceModel;

namespace UnifiedLogin.SharedObjects.Product.OneSite;

[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "8.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace = "http://realpage.com/webservices", ConfigurationName = "OneSiteProductService")]
public interface IOneSiteProductService
{

    NetworkCredential Credentials { get; set; }
    string Url { get; set; }
    bool PreAuthenticate { get; set; }

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ValidateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    bool ValidateUser(OneSite.NameValuePair[] User, string Password);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ValidateUser", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    System.Threading.Tasks.Task<bool> ValidateUserAsync(OneSite.NameValuePair[] User, string Password);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CheckIfUserIDIsUsed", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserExists")]
    OneSite.UserExists CheckIfUserIDIsUsed(OneSite.NameValuePair[] uiArgs);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CheckIfUserIDIsUsed", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserExists")]
    System.Threading.Tasks.Task<OneSite.UserExists> CheckIfUserIDIsUsedAsync(OneSite.NameValuePair[] uiArgs);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetPMCUrl", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PMCInfo")]
    OneSite.PMCInfo GetPMCUrl(int PMCID);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetPMCUrl", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PMCInfo")]
    System.Threading.Tasks.Task<OneSite.PMCInfo> GetPMCUrlAsync(int PMCID);

    // CODEGEN: Parameter 'UserInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateSuperuser", Name = "CreateSuperuserRequest", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.CreateSuperuserResponse CreateSuperuser(OneSite.CreateSuperuserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateSuperuser", Name = "CreateSuperUserNameValuePair", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.NameValuePair[] CreateSuperuser(OneSite.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateSuperuser", Name = "CreateSuperuserAsync", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSite.CreateSuperuserResponse> CreateSuperuserAsync(OneSite.CreateSuperuserRequest request);

    // CODEGEN: Parameter 'UserInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateUser", Name = "CreateUserRequest", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.CreateUserResponse CreateUser(OneSite.CreateUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateUser", Name = "CreateUserNameValuePair", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.NameValuePair[] CreateUser(OneSite.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateUser", Name = "CreateUserRequestAsync", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSite.CreateUserResponse> CreateUserAsync(OneSite.CreateUserRequest request);

    // CODEGEN: Parameter 'UserInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Name = "UpdateSuperuserRequest", Action = "http://realpage.com/webservices/UpdateSuperuser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.UpdateSuperuserResponse UpdateSuperuser(OneSite.UpdateSuperuserRequest request);

    [System.ServiceModel.OperationContractAttribute(Name = "UpdateSuperuserNameValuePair", Action = "http://realpage.com/webservices/UpdateSuperuser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.NameValuePair[] UpdateSuperuser(string SystemIdentifier, OneSite.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Name = "UpdateSuperuserRequestAsync", Action = "http://realpage.com/webservices/UpdateSuperuser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSite.UpdateSuperuserResponse> UpdateSuperuserAsync(OneSite.UpdateSuperuserRequest request);

    // CODEGEN: Parameter 'UserInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Name = "UpdateUserRequest", Action = "http://realpage.com/webservices/UpdateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.UpdateUserResponse UpdateUser(OneSite.UpdateUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Name = "UpdateUserNameValuePair", Action = "http://realpage.com/webservices/UpdateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.NameValuePair[] UpdateUser(string SystemIdentifier, OneSite.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Name = "UpdateUserRequestAsync", Action = "http://realpage.com/webservices/UpdateUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSite.UpdateUserResponse> UpdateUserAsync(OneSite.UpdateUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignPropertiesToUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    OneSite.AssignStatus AssignPropertiesToUser(string SystemIdentifier, string SelectedPropertyIds);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignPropertiesToUser", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    System.Threading.Tasks.Task<OneSite.AssignStatus> AssignPropertiesToUserAsync(string SystemIdentifier, string SelectedPropertyIds);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemovePropertiesFromUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    OneSite.AssignStatus RemovePropertiesFromUser(string SystemIdentifier, string PropertyIdsToRemove);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemovePropertiesFromUser", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    System.Threading.Tasks.Task<OneSite.AssignStatus> RemovePropertiesFromUserAsync(string SystemIdentifier, string PropertyIdsToRemove);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRolesToUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    OneSite.AssignStatus AssignRolesToUser(string SystemIdentifier, string SelectedRoleIds);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRolesToUser", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    System.Threading.Tasks.Task<OneSite.AssignStatus> AssignRolesToUserAsync(string SystemIdentifier, string SelectedRoleIds);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemoveRolesFromUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    OneSite.AssignStatus RemoveRolesFromUser(string SystemIdentifier, string RoleIdsToRemove);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemoveRolesFromUser", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    System.Threading.Tasks.Task<OneSite.AssignStatus> RemoveRolesFromUserAsync(string SystemIdentifier, string RoleIdsToRemove);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnableUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    void EnableUser(string SystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnableUser", ReplyAction = "*")]
    System.Threading.Tasks.Task EnableUserAsync(string SystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ResetVerificationCode", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    void ResetVerificationCode(string SystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ResetVerificationCode", ReplyAction = "*")]
    System.Threading.Tasks.Task ResetVerificationCodeAsync(string SystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisableUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    void DisableUser(string SystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisableUser", ReplyAction = "*")]
    System.Threading.Tasks.Task DisableUserAsync(string SystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllPropertiesWithSortFilters", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PropertyList")]
    OneSite.PropertyList GetAllProperties(OneSite.NameValuePair[] uiArgs, string SystemIdentifier, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllPropertiesWithSortFilters", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PropertyList")]
    System.Threading.Tasks.Task<OneSite.PropertyList> GetAllPropertiesAsync(OneSite.NameValuePair[] uiArgs, string SystemIdentifier, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetDefaultProperties", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PropertyList")]
    OneSite.PropertyList GetDefaultProperties(OneSite.NameValuePair[] uiArgs, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetDefaultProperties", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PropertyList")]
    System.Threading.Tasks.Task<OneSite.PropertyList> GetDefaultPropertiesAsync(OneSite.NameValuePair[] uiArgs, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserProperties", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PropertyList")]
    OneSite.PropertyList GetUserProperties(string SystemIdentifier, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserProperties", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PropertyList")]
    System.Threading.Tasks.Task<OneSite.PropertyList> GetUserPropertiesAsync(string SystemIdentifier, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUsersForProperty", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserList")]
    OneSite.UserList GetUsersForProperty(string SystemIdentifier, int PropertyID, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUsersForProperty", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserList")]
    System.Threading.Tasks.Task<OneSite.UserList> GetUsersForPropertyAsync(string SystemIdentifier, int PropertyID, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    OneSite.RoleList GetAllRoles(OneSite.NameValuePair[] uiArgs, string SystemIdentifier, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllRoles", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    System.Threading.Tasks.Task<OneSite.RoleList> GetAllRolesAsync(OneSite.NameValuePair[] uiArgs, string SystemIdentifier, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetDefaultRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    OneSite.RoleList GetDefaultRoles(OneSite.NameValuePair[] uiArgs, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetDefaultRoles", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    System.Threading.Tasks.Task<OneSite.RoleList> GetDefaultRolesAsync(OneSite.NameValuePair[] uiArgs, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    OneSite.RoleList GetUserRoles(string SystemIdentifier, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserRoles", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    System.Threading.Tasks.Task<OneSite.RoleList> GetUserRolesAsync(string SystemIdentifier, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUsersForRole", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserList")]
    OneSite.UserList GetUsersForRole(string SystemIdentifier, int RoleId, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUsersForRole", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserList")]
    System.Threading.Tasks.Task<OneSite.UserList> GetUsersForRoleAsync(string SystemIdentifier, int RoleId, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    void DeleteUser(string SystemIdentifier, string DeleteUserSystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteUser", ReplyAction = "*")]
    System.Threading.Tasks.Task DeleteUserAsync(string SystemIdentifier, string DeleteUserSystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ClaimUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    void ClaimUser(string SystemIdentifier, bool IsLinked);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ClaimUser", ReplyAction = "*")]
    System.Threading.Tasks.Task ClaimUserAsync(string SystemIdentifier, bool IsLinked);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ClaimUserUL", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    void ClaimUserUL(string SystemIdentifier, bool IsULLinked);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ClaimUserUL", ReplyAction = "*")]
    System.Threading.Tasks.Task ClaimUserULAsync(string SystemIdentifier, bool IsULLinked);

    // CODEGEN: Parameter 'UserInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Name = "GetUserRequest", Action = "http://realpage.com/webservices/GetUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.GetUserResponse GetUser(OneSite.GetUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Name = "GetUserNameValuePair", Action = "http://realpage.com/webservices/GetUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSite.NameValuePair[] GetUser(OneSite.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Name = "GetUserRequestAsync", Action = "http://realpage.com/webservices/GetUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSite.GetUserResponse> GetUserAsync(OneSite.GetUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetRightsCentersList", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RightCenter")]
    OneSite.RightCenter GetRightsCentersList(OneSite.NameValuePair[] uiArgs, string SystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetRightsCentersList", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RightCenter")]
    System.Threading.Tasks.Task<OneSite.RightCenter> GetRightsCentersListAsync(OneSite.NameValuePair[] uiArgs, string SystemIdentifier);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetRightsList", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RightList")]
    OneSite.RightList GetRightsList(OneSite.NameValuePair[] uiArgs, string SystemIdentifier, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetRightsList", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RightList")]
    System.Threading.Tasks.Task<OneSite.RightList> GetRightsListAsync(OneSite.NameValuePair[] uiArgs, string SystemIdentifier, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRightToRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    OneSite.AssignStatus ModifyRightToRoles(string SystemIdentifier, int RightId, string SelectedRoleIds, bool AssignRight);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRightToRoles", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    System.Threading.Tasks.Task<OneSite.AssignStatus> ModifyRightToRolesAsync(string SystemIdentifier, int RightId, string SelectedRoleIds, bool AssignRight);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ModifyRoleToRights", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    OneSite.AssignStatus ModifyRoleToRights(string SystemIdentifier, int RoleId, string RightIdsToAdd, string RightIdsToRemove);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ModifyRoleToRights", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignStatus")]
    System.Threading.Tasks.Task<OneSite.AssignStatus> ModifyRoleToRightsAsync(string SystemIdentifier, int RoleId, string RightIdsToAdd, string RightIdsToRemove);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetRolesForRight", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    OneSite.RoleList GetRolesForRight(OneSite.NameValuePair[] uiArgs, int RightId, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetRolesForRight", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    System.Threading.Tasks.Task<OneSite.RoleList> GetRolesForRightAsync(OneSite.NameValuePair[] uiArgs, int RightId, bool AssignedOnly, OneSite.FilterSortParameters filterSortParameters);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AddUpdateRole", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    OneSite.RoleList AddUpdateRole(string SystemIdentifier, string RoleId, string RoleName, string InheritRoleID);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AddUpdateRole", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleList")]
    System.Threading.Tasks.Task<OneSite.RoleList> AddUpdateRoleAsync(string SystemIdentifier, string RoleId, string RoleName, string InheritRoleID);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteRole", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    void DeleteRole(string SystemIdentifier, int RoleID);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteRole", ReplyAction = "*")]
    System.Threading.Tasks.Task DeleteRoleAsync(string SystemIdentifier, int RoleID);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserInLeasingAgentList", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    bool GetUserInLeasingAgentList(string SystemIdentifier, int SiteID);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserInLeasingAgentList", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    System.Threading.Tasks.Task<bool> GetUserInLeasingAgentListAsync(string SystemIdentifier, int SiteID);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/authenticate", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    bool authenticate(string username, string password, string additionalInfo);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/authenticate", ReplyAction = "*")]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    System.Threading.Tasks.Task<bool> authenticateAsync(string username, string password, string additionalInfo);

}