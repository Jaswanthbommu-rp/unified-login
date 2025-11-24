#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    /// <summary>
    /// Central DataAnnotations validator for DTO objects.
    /// Upgraded from legacy RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Helpers version.
    /// </summary>
    public static class DtoValidator
    {
        /// <summary>
        /// Validate a single object using DataAnnotations, returning all validation results.
        /// Null instances produce a single ValidationResult indicating the object is null.
        /// </summary>
        public static IEnumerable<ValidationResult> ValidateObject(object? source)
        {
            var results = new List<ValidationResult>();
            if (source is null)
            {
                results.Add(new ValidationResult("Request object is null."));
                return results;
            }
            var context = new ValidationContext(source);
            Validator.TryValidateObject(source, context, results, validateAllProperties: true);
            return results;
        }

        /// <summary>
        /// Validate a collection of objects aggregating all validation errors.
        /// Skips null entries but records them as individual errors.
        /// </summary>
        public static IEnumerable<ValidationResult> ValidateAll(IEnumerable<object?>? sources)
        {
            var aggregate = new List<ValidationResult>();
            if (sources is null) return aggregate;
            foreach (var s in sources)
            {
                if (s is null)
                {
                    aggregate.Add(new ValidationResult("Encountered null item in collection."));
                    continue;
                }
                var context = new ValidationContext(s);
                Validator.TryValidateObject(s, context, aggregate, validateAllProperties: true);
            }
            return aggregate;
        }
    }
}
