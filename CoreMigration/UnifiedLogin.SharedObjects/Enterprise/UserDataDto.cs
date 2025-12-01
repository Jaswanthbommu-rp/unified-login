#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    /// <summary>
    /// UserData Data Transfer Object (migrated to .NET 9)
    /// </summary>
    public class UserDataDto : UserDataDtoCommon
    {
        [MaxLength(12, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
        public string? Suffix { get; set; }

        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public string? Phone { get; set; }

        [MaxLength(50, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
        public string? Title { get; set; }

        [EnumDataType(typeof(UserTypeDto), ErrorMessage = "UserType should be Regular or NoEmail.")]
        public UserTypeDto UserType { get; set; }

        public Dictionary<string, string> CustomFields { get; set; } = new();
        public Dictionary<string, string> AdditionalFields { get; set; } = new();

        [JsonProperty("SendInvitationEmail", NullValueHandling = NullValueHandling.Ignore)]
        public bool? SendInvitationEmail { get; set; }

        [JsonProperty("doNotForceChangePassword", NullValueHandling = NullValueHandling.Ignore)]
        public bool doNotForceChangePassword { get; set; }
    }
}
