#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    public class ProductDetailDto
    {
        [Required(ErrorMessage = "{0} is a required field.")]
        [MaxLength(5, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
        public string ProductCode { get; set; } = string.Empty;

        public List<string> PropertiesAssigned { get; set; } = new();
        public List<string> RolesAssigned { get; set; } = new();
        public List<string> RegionsAssigned { get; set; } = new();

        public Dictionary<string, string> AdditionalFields { get; set; } = new();
        public bool IsAssigned { get; set; } = true;
    }
}
