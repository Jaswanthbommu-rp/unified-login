#nullable enable
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.ComponentModel.DataAnnotations;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    /// <summary>
    /// Shared User Data Transfer Object attributes (migrated to .NET 9)
    /// </summary>
    public class UserDataDtoCommon
    {
        public Guid UnityRealPageUserId { get; set; }

        [Required(ErrorMessage = "{0} is a required field.")]
        [MaxLength(50, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
        public string? FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = "{0} is a required field.")]
        [MaxLength(50, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
        public string? LastName { get; set; }

        public bool IsExternalIdp { get; set; }

        [DisplayFormat(ConvertEmptyStringToNull = false)]
        [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is a required field.")]
        [MaxLength(255, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
        public string LoginName { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage = "The email address is not valid")]
        public string? Email { get; set; }

        [DataType(DataType.Date)]
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
        public DateTime? UserEffectiveDate { get; set; }

        [DataType(DataType.Date)]
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
        public DateTime? UserExpirationDate { get; set; }

        [JsonProperty("EmployeeId", NullValueHandling = NullValueHandling.Ignore)]
        public string? EmployeeId { get; set; }

        [DataType(DataType.Date)]
        [JsonConverter(typeof(DateFormatConverter), "yyyy-MM-dd")]
        public DateTime? LastLogin { get; set; }
    }

    public class DateFormatConverter : IsoDateTimeConverter
    {
        public DateFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }
}
