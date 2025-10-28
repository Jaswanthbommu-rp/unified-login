using System.Net;
using System.Xml.Serialization;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.Accounting
{
    /// <summary>
    /// Used to call the Accounting web services to create/edit users
    /// </summary>
	public interface IOneSiteAccountingProductService
	{
        NetworkCredential Credentials { get; set; }
        bool PreAuthenticate { get; set; }
		string Url { get; set; }
		bool UseDefaultCredentials { get; set; }

		//event AssignPropertiesToUserCompletedEventHandler AssignPropertiesToUserCompleted;
		//event AssignRolePermissionsCompletedEventHandler AssignRolePermissionsCompleted;
		//event AssignRolesToUserCompletedEventHandler AssignRolesToUserCompleted;
		//event ChangeClaimStatusCompletedEventHandler ChangeClaimStatusCompleted;
		//event CheckIfUserIDIsUsedCompletedEventHandler CheckIfUserIDIsUsedCompleted;
		//event CreateRoleCompletedEventHandler CreateRoleCompleted;
		//event CreateUserCompletedEventHandler CreateUserCompleted;
		//event DeleteRoleCompletedEventHandler DeleteRoleCompleted;
		//event DeleteUserCompletedEventHandler DeleteUserCompleted;
		//event DisableUserCompletedEventHandler DisableUserCompleted;
  //      event DisableGreenBookUserCompletedEventHandler DisableGreenBookUserCompleted;
  //      event EnableUserCompletedEventHandler EnableUserCompleted;
  //      event EnableGreenBookUserCompletedEventHandler EnableGreenBookUserCompleted;
  //      event GetAllPropertiesCompletedEventHandler GetAllPropertiesCompleted;
		//event GetAllRolesCompletedEventHandler GetAllRolesCompleted;
		//event GetAllUsersCompletedEventHandler GetAllUsersCompleted;
		//event GetApplicationPermissionsCompletedEventHandler GetApplicationPermissionsCompleted;
		//event GetApplicationsCompletedEventHandler GetApplicationsCompleted;
		//event GetPermissionRolesCompletedEventHandler GetPermissionRolesCompleted;
		//event GetRolePermissionsCompletedEventHandler GetRolePermissionsCompleted;
		//event GetUserCompletedEventHandler GetUserCompleted;
		//event GetUserPropertiesCompletedEventHandler GetUserPropertiesCompleted;
		//event GetUserRolesCompletedEventHandler GetUserRolesCompleted;
		//event RemovePropertiesFromUserCompletedEventHandler RemovePropertiesFromUserCompleted;
		//event RemoveRolesFromUserCompletedEventHandler RemoveRolesFromUserCompleted;
		//event UpdateUserCompletedEventHandler UpdateUserCompleted;
		//event ValidateUserCompletedEventHandler ValidateUserCompleted;
  //      event getCompaniesAPICompletedEventHandler getCompaniesAPICompleted;        
  //      event getPropertiesAPICompletedEventHandler getPropertiesAPICompleted;
		//event GetAllPropertyGroupsCompletedEventHandler GetAllPropertyGroupsCompleted;
		//event GetAllPropertyGroupMembersCompletedEventHandler GetAllPropertyGroupMembersCompleted;

		string AssignPropertiesToUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void AssignPropertiesToUserAsync(NameValuePair[] User);
		void AssignPropertiesToUserAsync(NameValuePair[] User, object userState);
		NameValuePair[] AssignRolePermissions([XmlElement("User")] User[] User, [XmlArrayItem(IsNullable = false)] RolePermission[] RolePermissions);
		void AssignRolePermissionsAsync(User[] User, RolePermission[] RolePermissions);
		void AssignRolePermissionsAsync(User[] User, RolePermission[] RolePermissions, object userState);
		string AssignRolesToUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void AssignRolesToUserAsync(NameValuePair[] User);
		void AssignRolesToUserAsync(NameValuePair[] User, object userState);
		void CancelAsync(object userState);
		void ChangeClaimStatus(string SystemIdentifier, bool IsLinked, string Login, string Password, string FederatedID);
		void ChangeClaimStatusAsync(string SystemIdentifier, bool IsLinked, string Login, string Password, string FederatedID);
		void ChangeClaimStatusAsync(string SystemIdentifier, bool IsLinked, string Login, string Password, string FederatedID, object userState);
		string CheckIfUserIDIsUsed([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void CheckIfUserIDIsUsedAsync(NameValuePair[] User);
		void CheckIfUserIDIsUsedAsync(NameValuePair[] User, object userState);
		NameValuePair[] CreateRole([XmlArrayItem(IsNullable = false)] NameValuePair[] Role);
		void CreateRoleAsync(NameValuePair[] Role);
		void CreateRoleAsync(NameValuePair[] Role, object userState);
		NameValuePair[] CreateUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void CreateUserAsync(NameValuePair[] User);
		void CreateUserAsync(NameValuePair[] User, object userState);
		NameValuePair[] DeleteRole([XmlArrayItem(IsNullable = false)] NameValuePair[] Role);
		void DeleteRoleAsync(NameValuePair[] Role);
		void DeleteRoleAsync(NameValuePair[] Role, object userState);
		string DeleteUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void DeleteUserAsync(NameValuePair[] User);
		void DeleteUserAsync(NameValuePair[] User, object userState);
		string DisableUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void DisableUserAsync(NameValuePair[] User);
		void DisableUserAsync(NameValuePair[] User, object userState);

        string DisableGreenBookUser([System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)] NameValuePair[] User);
        void DisableGreenBookUserAsync(NameValuePair[] User);
        void DisableGreenBookUserAsync(NameValuePair[] User, object userState);

        string EnableUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void EnableUserAsync(NameValuePair[] User);
		void EnableUserAsync(NameValuePair[] User, object userState);

        string EnableGreenBookUser([System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)] NameValuePair[] User);
        void EnableGreenBookUserAsync(NameValuePair[] User);
        void EnableGreenBookUserAsync(NameValuePair[] User, object userState);

        CompanyID[] getCompaniesAPI([System.Xml.Serialization.XmlElementAttribute("Company")] Company[] Company);
        void getCompaniesAPIAsync(Company[] Company);
        void getCompaniesAPIAsync(Company[] Company, object userState);
        EntityID[] getPropertiesAPI([System.Xml.Serialization.XmlElementAttribute("Company")] Company[] Company);
        void getPropertiesAPIAsync(Company[] Company);
        void getPropertiesAPIAsync(Company[] Company, object userState);
        
        LocationID[] GetAllProperties([XmlElement("Property")] Property[] Property, FilterSortParameters FilterSortParameters, [XmlElement("ResultParameters")] out TotalRows[] ResultParameters);
		void GetAllPropertiesAsync(Property[] Property, FilterSortParameters FilterSortParameters);
		void GetAllPropertiesAsync(Property[] Property, FilterSortParameters FilterSortParameters, object userState);
		
		LocationGroupID[] GetAllPropertyGroups([XmlElement("Property")] Property[] Property, FilterSortParameters FilterSortParameters, [XmlElement("ResultParameters")] out TotalRows[] ResultParameters);
		void GetAllPropertyGroupsAsync(Property[] Property, FilterSortParameters FilterSortParameters);
		void GetAllPropertyGroupsAsync(Property[] Property, FilterSortParameters FilterSortParameters, object userState);

		LocationGroupID[] GetAllPropertyGroupMembers([XmlElementAttribute("Property")] Property[] Property, FilterSortParameters FilterSortParameters, [XmlElementAttribute("ResultParameters")] out TotalRows[] ResultParameters);
		void GetAllPropertyGroupMembersAsync(Property[] Property, FilterSortParameters FilterSortParameters);
		void GetAllPropertyGroupMembersAsync(Property[] Property, FilterSortParameters FilterSortParameters, object userState);

		RoleName[] GetAllRoles([XmlElement("Role")] Role[] Role, FilterSortParameters FilterSortParameters, [XmlElement("ResultParameters")] out TotalRows[] ResultParameters);
		void GetAllRolesAsync(Role[] Role, FilterSortParameters FilterSortParameters);
		void GetAllRolesAsync(Role[] Role, FilterSortParameters FilterSortParameters, object userState);
		UserName[] GetAllUsers([XmlElement("User")] User[] User, FilterSortParameters FilterSortParameters, [XmlElement("ResultParameters")] out TotalRows[] ResultParameters);
		void GetAllUsersAsync(User[] User, FilterSortParameters FilterSortParameters);
		void GetAllUsersAsync(User[] User, FilterSortParameters FilterSortParameters, object userState);
		PermissionID[] GetApplicationPermissions([XmlElement("Permissions")] Permissions[] Permissions);
		void GetApplicationPermissionsAsync(Permissions[] Permissions);
		void GetApplicationPermissionsAsync(Permissions[] Permissions, object userState);
		ApplicationID[] GetApplications([XmlElement("Applications")] Applications[] Applications);
		void GetApplicationsAsync(Applications[] Applications);
		void GetApplicationsAsync(Applications[] Applications, object userState);
		PermissionuID[] GetPermissionRoles([XmlElement("Permission")] Permissions[] Permission);
		void GetPermissionRolesAsync(Permissions[] Permission);
		void GetPermissionRolesAsync(Permissions[] Permission, object userState);
		PermissionID[] GetRolePermissions([XmlElement("Permissions")] Permissions[] Permissions);
		void GetRolePermissionsAsync(Permissions[] Permissions);
		void GetRolePermissionsAsync(Permissions[] Permissions, object userState);
		NameValuePair[] GetUser([XmlElement("User")] User[] User);
		void GetUserAsync(User[] User);
		void GetUserAsync(User[] User, object userState);
		LocationID[] GetUserProperties([XmlElement("Property")] Property[] Property, FilterSortParameters FilterSortParameters, [XmlElement("ResultParameters")] out TotalRows[] ResultParameters);
		void GetUserPropertiesAsync(Property[] Property, FilterSortParameters FilterSortParameters);
		void GetUserPropertiesAsync(Property[] Property, FilterSortParameters FilterSortParameters, object userState);
		RoleName[] GetUserRoles([XmlElement("Role")] Role[] Role, FilterSortParameters FilterSortParameters, [XmlElement("ResultParameters")] out TotalRows[] ResultParameters);
		void GetUserRolesAsync(Role[] Role, FilterSortParameters FilterSortParameters);
		void GetUserRolesAsync(Role[] Role, FilterSortParameters FilterSortParameters, object userState);
		string RemovePropertiesFromUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void RemovePropertiesFromUserAsync(NameValuePair[] User);
		void RemovePropertiesFromUserAsync(NameValuePair[] User, object userState);
		string RemoveRolesFromUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void RemoveRolesFromUserAsync(NameValuePair[] User);
		void RemoveRolesFromUserAsync(NameValuePair[] User, object userState);
		string UpdateUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
		void UpdateUserAsync(NameValuePair[] User);
		void UpdateUserAsync(NameValuePair[] User, object userState);
        string UpdateUserDetails([XmlArrayItem(IsNullable = false)] NameValuePair[] User);
        void UpdateUserDetailsAsync(NameValuePair[] User);
        void UpdateUserDetailsAsync(NameValuePair[] User, object userState);
        string ValidateUser([XmlArrayItem(IsNullable = false)] NameValuePair[] User, string Password);
		void ValidateUserAsync(NameValuePair[] User, string Password);
		void ValidateUserAsync(NameValuePair[] User, string Password, object userState);
	}
}