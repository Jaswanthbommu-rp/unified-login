using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.UnifiedLogin
{
	/// <summary>
	/// User Location
	/// </summary>
	public class UserLocation
	{
		/// <summary>
		/// PropertyId
		/// </summary>
		public string PropertyId { get; set; }
	}

	/// <summary>
	/// User Access Group
	/// </summary>
	public class UserAccessGroup
	{
		/// <summary>
		/// Access Group Code
		/// </summary>
		public string AccessGroupCode { get; set; }
		//public bool IsAssigned { get; set; }

		/// <summary>
		/// Description
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string Description { get; set; }

		/// <summary>
		/// Access Group Name
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public string AccessGroupName { get; set; }
	}

	/// <summary>
	/// UL Rights Add Remove List
	/// </summary>
	public class ULRightsAddRemoveList
	{
		/// <summary>
		/// Rights To Add
		/// </summary>
		public List<string> RightsToAdd;

		/// <summary>
		/// Rights To Delete
		/// </summary>
		public List<string> RightsToDelete;
	}

	/// <summary>
	/// Roles Add Remove List
	/// </summary>
	public class RolesAddRemoveList
	{
		/// <summary>
		/// Roles To Add
		/// </summary>
		public List<string> RolesToAdd;

		/// <summary>
		/// Roles To Delete
		/// </summary>
		public List<string> RolesToDelete;
	}

	///// <summary>
	///// Right
	///// </summary>
	//public class Right
	//{
	//	/// <summary>
	//	/// Unique RightId
	//	/// </summary>
	//	public int RightId { get; set; }

	//	/// <summary>
	//	/// Right Name
	//	/// </summary>
	//	public string RightName { get; set; }

	//	/// <summary>
	//	/// Right Value TypeId
	//	/// </summary>
	//	public int RightValueTypeId { get; set; }
	//}

	/// <summary>
	/// Right to Roles Details
	/// </summary>
	public class RightRoleDetail : Right
	{
		/// <summary>
		/// Role Name
		/// </summary>
		public string RoleName { get; set; }

		/// <summary>
		/// Unique RoleId
		/// </summary>
		public int RoleId { get; set; }

		/// <summary>
		/// Role Type
		/// </summary>
		public string RoleType { get; set; }

		/// <summary>
		/// IsAssigned
		/// </summary>
		public bool IsAssigned { get; set; }

		/// <summary>
		/// Right Assined
		/// </summary>
		public string RightsAssigned { get; set; }

		/// <summary>
		/// Role Assigned
		/// </summary>
		public string RolesAssigned { get; set; }

        
        /// <summary>
        /// Is the Default role assigned 
        /// </summary>
        public bool IsDefaultRole { get; set; }
    }

	/// <summary>
	/// Object to Category
	/// </summary>
	public class CategoryType
	{
		/// <summary>
		/// Category Name
		/// </summary>
		public string CategoryName { get; set; }

		/// <summary>
		/// Status
		/// </summary>
		public string Status { get; set; }

		/// <summary>
		/// StatusTypeid
		/// </summary>
		public int StatusTypeid { get; set; }
	}

	/// <summary>
	/// Object to map with Input Json from UI
	/// </summary>
	public class UserAssignProductPropertyRole
	{
		/// <summary>
		/// A list of properties to assign to the user
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> PropertyList { get; set; }


		/// <summary>
		/// A role to assign to the user
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public List<string> RoleList { get; set; }

		/// <summary>
		/// Is product assigned or removed
		/// </summary>
		public bool IsAssigned { get; set; }
	}
}
