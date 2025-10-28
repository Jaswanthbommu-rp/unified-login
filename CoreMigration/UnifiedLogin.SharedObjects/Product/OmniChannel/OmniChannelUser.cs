using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;


namespace UnifiedLogin.SharedObjects.Product.OmniChannel
{
    public class OmniChannelUser
    {
        public string ID { get; set; }

        public string UserCode { get; set; }

        public string Username { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Password { get; set; }

        public string CompanyId { get; set; }
        //public string CompanyCode { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? CompanyDivisionId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AccessLevel { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AccessLevelCode { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Phone { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //public bool Locked { get; set; }//Not IsLocked in product
        public bool EMailNotifyInsurance { get; set; }

        public bool EMailNotifyRecommendation { get; set; }

       
        /// <summary>
        /// List of property id
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<UserLocation> UserLocations { get; set; }
    }

    public class UserLocation
    {
        public string PropertyId { get; set; }
    }

    public class UserAccessGroup
    {
        public string AccessGroupCode { get; set; }
        //public bool IsAssigned { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AccessGroupName { get; set; }
    }

    /// <summary>
    /// Product mapping class to send through product API
    /// </summary>
    public class ProductPropertyRole
    {
        /// <summary>
        /// A list of properties to assign to the user
        /// </summary>
        public List<UserLocation> PropertyList { get; set; } //-1 = all

       

        /// <summary>
        /// A list of roles to assign to the user
        /// </summary>
        public List<UserAccessGroup> UserAccessGroups { get; set; }

        
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
