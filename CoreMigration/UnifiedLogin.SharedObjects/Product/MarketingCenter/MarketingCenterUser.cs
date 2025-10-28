using Newtonsoft.Json;
using System.Collections.Generic;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.MarketingCenter
{
    /// <summary>
    /// THe Marketing Center user
    /// </summary>
    public class MarketingCenterUser
    {
        /// <summary>
        /// The id for the company in Marketing center
        /// </summary>
        [JsonProperty(PropertyName = "companyId")]
        public int CompanyId { get; set; }
        /// <summary>
        /// The id of the role given to the user
        /// </summary>
        [JsonProperty(PropertyName = "contactRoleId")]
        public long? ContactRoleId { get; set; }
        /// <summary>
        /// The id of the role given to the user
        /// </summary>
        [JsonProperty(PropertyName = "contactRoleName", NullValueHandling = NullValueHandling.Ignore)]
        public string ContactRoleName { get; set; }
        /// <summary>
        /// The first name of the user
        /// </summary>
        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }
        /// <summary>
        /// The last name of the user
        /// </summary>
        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }
        /// <summary>
        /// The middle initial of the user
        /// </summary>
        [JsonProperty(PropertyName = "middleInitial", NullValueHandling = NullValueHandling.Ignore)]
        public string MiddleInitial {get;set;}
        /// <summary>
        /// The email address of the user
        /// </summary>
        [JsonProperty(PropertyName = "emailAddress")]
		public string EmailAddress { get; set; }
		/// <summary>
		/// The Notification email address of the user
		/// </summary>
		[JsonProperty(PropertyName = "leadEmailAddress")]
		public string LeadEmailAddress { get; set; }
        /// <summary>
        /// A list of properties to assign to the user
        /// </summary>
        [JsonProperty(PropertyName = "assignPropertyIds")]
        public List<int> AssignPropertyIds { get; set; }
        /// <summary>
        /// A list of properties to unassign from the user
        /// </summary>
        [JsonProperty(PropertyName = "unassignPropertyIds", NullValueHandling = NullValueHandling.Ignore)]
        public List<int> UnassignPropertyIds { get; set; }
        /// <summary>
        /// A flag to determine if the properties are being assigned or unassigned
        /// </summary>
        [JsonProperty(PropertyName = "assignUnassignProperties", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AssignUnassignProperties { get; set; }
        /// <summary>
        /// Has the customer already been sent the welcome email. In this case we want true so it doesn't send one out. 
        /// </summary>
        [JsonProperty(PropertyName = "welcomeEmailSent")]
        public bool WelcomeEmailSent { get; set; } = true;
		/// <summary>
		/// A flag to assign all company properties to the contact
		/// </summary>
		[JsonProperty(PropertyName = "assignAllProperties", NullValueHandling = NullValueHandling.Ignore)]
		public bool? AssignAllProperties { get; set; }
		/// <summary>
		/// A flag to unassign all properties from the contact
		/// </summary>
		[JsonProperty(PropertyName = "unassignAllProperties", NullValueHandling = NullValueHandling.Ignore)]
		public bool? UnassignAllProperties { get; set; }
        /// <summary>
        /// A flag to unassign all properties from the contact
        /// </summary>
        [JsonProperty(PropertyName = "assignNewProperty", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AssignNewProperty { get; set; }
    }
}
