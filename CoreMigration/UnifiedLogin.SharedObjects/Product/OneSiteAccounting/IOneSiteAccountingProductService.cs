using System.Net;

namespace UnifiedLogin.SharedObjects.Product.OneSiteAccounting;

[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "8.0.0")]
[System.ServiceModel.ServiceContractAttribute(Namespace = "http://realpage.com/webservices", ConfigurationName = "OneSiteAccountingProductServicePortBinding")]
public interface IOneSiteAccountingProductService
{
    NetworkCredential Credentials { get; set; }
    bool PreAuthenticate { get; set; }
    string Url { get; set; }
    //bool UseDefaultCredentials { get; set; }

    // CODEGEN: Parameter 'UserInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSiteAccounting.GetUserResponse GetUser(OneSiteAccounting.GetUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSiteAccounting.NameValuePair[] GetUser(OneSiteAccounting.User[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetUserResponse> GetUserAsync(OneSiteAccounting.GetUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.GetUserRolesResponse GetUserRoles(OneSiteAccounting.GetUserRolesRequest request);

    // CODEGEN: Generating message contract since the operation has multiple return values.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserRoles", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetUserRolesResponse> GetUserRolesAsync(OneSiteAccounting.GetUserRolesRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllUsers", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.GetAllUsersResponse GetAllUsers(OneSiteAccounting.GetAllUsersRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllUsers", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.UserName[] GetAllUsers(OneSiteAccounting.User[] User, OneSiteAccounting.FilterSortParameters FilterSortParameters, out OneSiteAccounting.TotalRows[] ResultParameters);

    // CODEGEN: Generating message contract since the operation has multiple return values.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllUsers", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetAllUsersResponse> GetAllUsersAsync(OneSiteAccounting.GetAllUsersRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.GetAllRolesResponse GetAllRoles(OneSiteAccounting.GetAllRolesRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.RoleName[] GetAllRoles(OneSiteAccounting.Role[] Role, OneSiteAccounting.FilterSortParameters FilterSortParameters, out OneSiteAccounting.TotalRows[] ResultParameters);

    // CODEGEN: Generating message contract since the operation has multiple return values.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllRoles", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetAllRolesResponse> GetAllRolesAsync(OneSiteAccounting.GetAllRolesRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllProperties", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.GetAllPropertiesResponse GetAllProperties(OneSiteAccounting.GetAllPropertiesRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllProperties", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.LocationID[] GetAllProperties(OneSiteAccounting.Property[] Property, OneSiteAccounting.FilterSortParameters FilterSortParameters, out OneSiteAccounting.TotalRows[] ResultParameters);

    // CODEGEN: Generating message contract since the operation has multiple return values.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllProperties", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetAllPropertiesResponse> GetAllPropertiesAsync(OneSiteAccounting.GetAllPropertiesRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllPropertyGroups", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.GetAllPropertyGroupsResponse GetAllPropertyGroups(OneSiteAccounting.GetAllPropertyGroupsRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllPropertyGroups", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.LocationGroupID[] GetAllPropertyGroups(OneSiteAccounting.Property[] Property, OneSiteAccounting.FilterSortParameters FilterSortParameters, out OneSiteAccounting.TotalRows[] ResultParameters);

    // CODEGEN: Generating message contract since the operation has multiple return values.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllPropertyGroups", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetAllPropertyGroupsResponse> GetAllPropertyGroupsAsync(OneSiteAccounting.GetAllPropertyGroupsRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllPropertyGroupMembers", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.GetAllPropertyGroupMembersResponse GetAllPropertyGroupMembers(OneSiteAccounting.GetAllPropertyGroupMembersRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllPropertyGroupMembers", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.LocationGroupID[] GetAllPropertyGroupMembers(OneSiteAccounting.Property[] Property, OneSiteAccounting.FilterSortParameters FilterSortParameters, out OneSiteAccounting.TotalRows[] ResultParameters);

    // CODEGEN: Generating message contract since the operation has multiple return values.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetAllPropertyGroupMembers", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetAllPropertyGroupMembersResponse> GetAllPropertyGroupMembersAsync(OneSiteAccounting.GetAllPropertyGroupMembersRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserProperties", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    OneSiteAccounting.GetUserPropertiesResponse GetUserProperties(OneSiteAccounting.GetUserPropertiesRequest request);

    // CODEGEN: Generating message contract since the operation has multiple return values.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetUserProperties", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetUserPropertiesResponse> GetUserPropertiesAsync(OneSiteAccounting.GetUserPropertiesRequest request);

    // CODEGEN: Parameter 'Companies' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/getCompaniesAPI", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "Companies")]
    OneSiteAccounting.getCompaniesAPIResponse getCompaniesAPI(OneSiteAccounting.getCompaniesAPIRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/getCompaniesAPI", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "Companies")]
    OneSiteAccounting.CompanyID[] getCompaniesAPI(OneSiteAccounting.Company[] Company);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/getCompaniesAPI", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.getCompaniesAPIResponse> getCompaniesAPIAsync(OneSiteAccounting.getCompaniesAPIRequest request);

    // CODEGEN: Parameter 'Entities' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/getPropertiesAPI", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "Entities")]
    OneSiteAccounting.getPropertiesAPIResponse getPropertiesAPI(OneSiteAccounting.getPropertiesAPIRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/getPropertiesAPI", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "Entities")]
    OneSiteAccounting.EntityID[] getPropertiesAPI(OneSiteAccounting.Company[] Company);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/getPropertiesAPI", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.getPropertiesAPIResponse> getPropertiesAPIAsync(OneSiteAccounting.getPropertiesAPIRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ValidateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.ValidateUserResponse ValidateUser(OneSiteAccounting.ValidateUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ValidateUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.ValidateUserResponse> ValidateUserAsync(OneSiteAccounting.ValidateUserRequest request);

    // CODEGEN: Parameter 'UserInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSiteAccounting.CreateUserResponse CreateUser(OneSiteAccounting.CreateUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "UserInfo")]
    OneSiteAccounting.NameValuePair[] CreateUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.CreateUserResponse> CreateUserAsync(OneSiteAccounting.CreateUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/UpdateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.UpdateUserResponse UpdateUser(OneSiteAccounting.UpdateUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/UpdateUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string UpdateUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/UpdateUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.UpdateUserResponse> UpdateUserAsync(OneSiteAccounting.UpdateUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/UpdateUserDetails", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.UpdateUserDetailsResponse UpdateUserDetails(OneSiteAccounting.UpdateUserDetailsRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/UpdateUserDetails", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string UpdateUserDetails(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/UpdateUserDetails", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.UpdateUserDetailsResponse> UpdateUserDetailsAsync(OneSiteAccounting.UpdateUserDetailsRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.DeleteUserResponse DeleteUser(OneSiteAccounting.DeleteUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string DeleteUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.DeleteUserResponse> DeleteUserAsync(OneSiteAccounting.DeleteUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnableUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.EnableUserResponse EnableUser(OneSiteAccounting.EnableUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnableUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string EnableUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnableUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.EnableUserResponse> EnableUserAsync(OneSiteAccounting.EnableUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnableGreenBookUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.EnableGreenBookUserResponse EnableGreenBookUser(OneSiteAccounting.EnableGreenBookUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnableGreenBookUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string EnableGreenBookUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnableGreenBookUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.EnableGreenBookUserResponse> EnableGreenBookUserAsync(OneSiteAccounting.EnableGreenBookUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnablePortalUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.EnablePortalUserResponse EnablePortalUser(OneSiteAccounting.EnablePortalUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/EnablePortalUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.EnablePortalUserResponse> EnablePortalUserAsync(OneSiteAccounting.EnablePortalUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisableUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.DisableUserResponse DisableUser(OneSiteAccounting.DisableUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisableUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string DisableUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisableUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.DisableUserResponse> DisableUserAsync(OneSiteAccounting.DisableUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisableGreenBookUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.DisableGreenBookUserResponse DisableGreenBookUser(OneSiteAccounting.DisableGreenBookUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisableGreenBookUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string DisableGreenBookUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisableGreenBookUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.DisableGreenBookUserResponse> DisableGreenBookUserAsync(OneSiteAccounting.DisableGreenBookUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisablePortalUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.DisablePortalUserResponse DisablePortalUser(OneSiteAccounting.DisablePortalUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DisablePortalUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.DisablePortalUserResponse> DisablePortalUserAsync(OneSiteAccounting.DisablePortalUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ChangeClaimStatus", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    void ChangeClaimStatus(string SystemIdentifier, bool IsLinked, string Login, string Password, string FederatedID);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/ChangeClaimStatus", ReplyAction = "*")]
    System.Threading.Tasks.Task ChangeClaimStatusAsync(string SystemIdentifier, bool IsLinked, string Login, string Password, string FederatedID);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CheckIfUserIDIsUsed", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.CheckIfUserIDIsUsedResponse CheckIfUserIDIsUsed(OneSiteAccounting.CheckIfUserIDIsUsedRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CheckIfUserIDIsUsed", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string CheckIfUserIDIsUsed(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CheckIfUserIDIsUsed", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.CheckIfUserIDIsUsedResponse> CheckIfUserIDIsUsedAsync(OneSiteAccounting.CheckIfUserIDIsUsedRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignPropertiesToUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.AssignPropertiesToUserResponse AssignPropertiesToUser(OneSiteAccounting.AssignPropertiesToUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignPropertiesToUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string AssignPropertiesToUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignPropertiesToUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.AssignPropertiesToUserResponse> AssignPropertiesToUserAsync(OneSiteAccounting.AssignPropertiesToUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemovePropertiesFromUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.RemovePropertiesFromUserResponse RemovePropertiesFromUser(OneSiteAccounting.RemovePropertiesFromUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemovePropertiesFromUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string RemovePropertiesFromUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemovePropertiesFromUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.RemovePropertiesFromUserResponse> RemovePropertiesFromUserAsync(OneSiteAccounting.RemovePropertiesFromUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRolesToUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.AssignRolesToUserResponse AssignRolesToUser(OneSiteAccounting.AssignRolesToUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRolesToUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string AssignRolesToUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRolesToUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.AssignRolesToUserResponse> AssignRolesToUserAsync(OneSiteAccounting.AssignRolesToUserRequest request);

    // CODEGEN: Parameter 'User' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlArrayItemAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemoveRolesFromUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    OneSiteAccounting.RemoveRolesFromUserResponse RemoveRolesFromUser(OneSiteAccounting.RemoveRolesFromUserRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemoveRolesFromUser", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "return")]
    string RemoveRolesFromUser(OneSiteAccounting.NameValuePair[] User);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/RemoveRolesFromUser", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.RemoveRolesFromUserResponse> RemoveRolesFromUserAsync(OneSiteAccounting.RemoveRolesFromUserRequest request);

    // CODEGEN: Parameter 'ApplicationInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetApplications", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "ApplicationInfo")]
    OneSiteAccounting.GetApplicationsResponse GetApplications(OneSiteAccounting.GetApplicationsRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetApplications", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "ApplicationInfo")]
    OneSiteAccounting.ApplicationID[] GetApplications(OneSiteAccounting.Applications[] Applications);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetApplications", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetApplicationsResponse> GetApplicationsAsync(OneSiteAccounting.GetApplicationsRequest request);

    // CODEGEN: Parameter 'PermissionInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetApplicationPermissions", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PermissionInfo")]
    OneSiteAccounting.GetApplicationPermissionsResponse GetApplicationPermissions(OneSiteAccounting.GetApplicationPermissionsRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetApplicationPermissions", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PermissionInfo")]
    OneSiteAccounting.PermissionID[] GetApplicationPermissions(OneSiteAccounting.Permissions[] Permissions);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetApplicationPermissions", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetApplicationPermissionsResponse> GetApplicationPermissionsAsync(OneSiteAccounting.GetApplicationPermissionsRequest request);

    // CODEGEN: Parameter 'PermissionInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetRolePermissions", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PermissionInfo")]
    OneSiteAccounting.GetRolePermissionsResponse GetRolePermissions(OneSiteAccounting.GetRolePermissionsRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetRolePermissions", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PermissionInfo")]
    OneSiteAccounting.PermissionID[] GetRolePermissions(OneSiteAccounting.Permissions[] Permissions);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetRolePermissions", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetRolePermissionsResponse> GetRolePermissionsAsync(OneSiteAccounting.GetRolePermissionsRequest request);

    // CODEGEN: Parameter 'PermissionInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetPermissionRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PermissionInfo")]
    OneSiteAccounting.GetPermissionRolesResponse GetPermissionRoles(OneSiteAccounting.GetPermissionRolesRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetPermissionRoles", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "PermissionInfo")]
    OneSiteAccounting.PermissionuID[] GetPermissionRoles(OneSiteAccounting.Permissions[] Permission);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/GetPermissionRoles", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.GetPermissionRolesResponse> GetPermissionRolesAsync(OneSiteAccounting.GetPermissionRolesRequest request);

    // CODEGEN: Parameter 'RoleInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateRole", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleInfo")]
    OneSiteAccounting.CreateRoleResponse CreateRole(OneSiteAccounting.CreateRoleRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateRole", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleInfo")]
    OneSiteAccounting.NameValuePair[] CreateRole(OneSiteAccounting.NameValuePair[] Role);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/CreateRole", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.CreateRoleResponse> CreateRoleAsync(OneSiteAccounting.CreateRoleRequest request);

    // CODEGEN: Parameter 'RoleInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteRole", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleInfo")]
    OneSiteAccounting.DeleteRoleResponse DeleteRole(OneSiteAccounting.DeleteRoleRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteRole", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "RoleInfo")]
    OneSiteAccounting.NameValuePair[] DeleteRole(OneSiteAccounting.NameValuePair[] Role);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/DeleteRole", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.DeleteRoleResponse> DeleteRoleAsync(OneSiteAccounting.DeleteRoleRequest request);

    // CODEGEN: Parameter 'AssignPermissionsInfo' requires additional schema information that cannot be captured using the parameter mode. The specific attribute is 'Microsoft.Xml.Serialization.XmlElementAttribute'.
    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRolePermissions", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignPermissionsInfo")]
    OneSiteAccounting.AssignRolePermissionsResponse AssignRolePermissions(OneSiteAccounting.AssignRolePermissionsRequest request);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRolePermissions", ReplyAction = "*")]
    [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
    [return: System.ServiceModel.MessageParameterAttribute(Name = "AssignPermissionsInfo")]
    OneSiteAccounting.NameValuePair[] AssignRolePermissions(OneSiteAccounting.User[] User, OneSiteAccounting.RolePermission[] RolePermissions);

    [System.ServiceModel.OperationContractAttribute(Action = "http://realpage.com/webservices/AssignRolePermissions", ReplyAction = "*")]
    System.Threading.Tasks.Task<OneSiteAccounting.AssignRolePermissionsResponse> AssignRolePermissionsAsync(OneSiteAccounting.AssignRolePermissionsRequest request);
}
