using System.Collections.Generic;
using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.VendorServices
{
    /// <summary>
    /// Product mapping class to send through product API
    /// </summary>
    public class ProductPropertyNotification
    {
        /// <summary>
        /// A list of properties to assign to the user
        /// </summary>
        public List<UserLocation> PropertyList { get; set; } //-1 = all

        /// <summary>
        /// A Property Group to assign to the user
        /// </summary>
        public PropertyGroup PropertyGroup { get; set; }

        /// <summary>
        /// A list of roles to assign to the user
        /// </summary>
        public List<UserAccessGroup> UserAccessGroups { get; set; }

        /// <summary>
        /// Notifications to assign to the user
        /// </summary>
        public Notification Notification { get; set; }

        /// <summary>
        /// AccessLevel to assign to the user
        /// </summary>
        public AccessTypeEnum AccessType { get; set; }
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

    public class UserLocation
    {
        public string PropertyId { get; set; }
    }


    /// <summary>
    /// Object to map with Input Json from UI
    /// </summary>
    public class UserProductPropertyNotification
    {
        /// <summary>
        /// A list of properties to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> PropertyList { get; set; } //-1 = all

        /// <summary>
        /// A Property Group to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PropertyGroup> PropertyGroup { get; set; }

        /// <summary>
        /// A list of roles to assign to the user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RoleList { get; set; }

        /// <summary>
        /// IsInsuranceExpired Notification 
        /// </summary>
        public bool IsInsuranceExpired { get; set; }

        /// <summary>
        /// IsVendorRecommendationChanges Notification 
        /// </summary>
        public bool IsVendorRecommendationChanges { get; set; }

        /// <summary>
        /// IsEMailNotifyVendorNotLinkedToAnyProperty Notification 
        /// </summary>
        public bool IsVendorNotLinkedToAnyProperty { get; set; }
                
        /// <summary>
        /// Is product assigned or removed
        /// </summary>
        public bool IsAssigned { get; set; }
    }
}
