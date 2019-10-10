using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Product.Rum
{
    public class Role
    {
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
