using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Dto
{
	public class ProductDetailDto
	{
		[Required(ErrorMessage = "{0} is a required field.")]
		[MaxLength(5, ErrorMessage = "{0} shouldn't be more than {1} characters.")]
		public string ProductCode { get; set; }
		public List<string> PropertiesAssigned { get; set; }
		public List<string> RolesAssigned { get; set; }
		public List<string> RegionsAssigned { get; set; }

		public Dictionary<string, string> AdditionalFields = new Dictionary<string, string>();
		public bool IsAssigned { get; set; } = true;
	}
}