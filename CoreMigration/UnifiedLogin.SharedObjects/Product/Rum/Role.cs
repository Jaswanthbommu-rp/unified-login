using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Product.Rum
{
    public class Role
    {
        public int Id { get; set; }
        /// <summary>
        /// Used to store the name of the role
        /// </summary>
        [JsonProperty("RoleName")]
        public string Name { get; set; }

        /// <summary>
        /// Used to store the description of the role
        /// </summary>
        [JsonProperty("RoleDescription")]
        public string Description { get; set; }

        /// <summary>
        /// specifiestype of the  role
        /// </summary>
        [JsonProperty("InternalOnly")]
        public bool IsInternal { get; set; }
        public bool IsAssigned { get; set; }
    }
}
