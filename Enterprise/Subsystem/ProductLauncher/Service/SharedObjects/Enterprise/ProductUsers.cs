using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise
{
    /// <summary>
    /// List of users by company and products
    /// </summary>
    public class ProductUsers
    {
        /// <summary>
        /// UserId
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// LoginName
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// FirstName
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// LastName
        /// </summary>
        public string LastName { get; set; }      

        /// <summary>
        /// PersonaId
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? PersonaId { get; set; }

        /// <summary>
        /// PreferredPhoneNumber
        /// </summary>
        public string PreferredPhoneNumber { get; set; }

        [JsonIgnore]
        public int TotalRecords { get; set; }

        /// <summary>
        /// Email Notification
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// UserType
        /// </summary>
        public string UserType { get; set; } = null;

        /// <summary>
        /// Email Notification
        /// </summary>
        public string UserStatus { get; set; } = null;
    }
}
