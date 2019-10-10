using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Helpers
{
	public static class DtoValidator
	{
		public static IEnumerable<ValidationResult> ValidateObject(object source)
		{
			var result = new List<ValidationResult>();

			if (source == null)
			{
				result.Add(new ValidationResult("Request object is null."));
				return result;
			}

			// Dto Validation
			var valContext = new ValidationContext(source, null, null);
			Validator.TryValidateObject(source, valContext, result, true);
			return result;
		}
	}
}