using System;
using System.ComponentModel.DataAnnotations;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class OrganizationDelete
    {
        /// <summary>
        /// Used to store the books id for the company
        /// </summary>
        [Required(ErrorMessage = "The UPFM company id is required.", AllowEmptyStrings = false)]
        [RegularExpression("^((?!00000000-0000-0000-0000-000000000000).)*$", ErrorMessage = "Cannot use default Guid for company id")]
        public Guid? OrganizationRealPageId { get; set; }

        /// <summary>
        /// Used to store the company type id for the company
        /// </summary>
        [Required(ErrorMessage = "RequestedBy is required.")]
        public string RequestedBy { get; set; }

    }
}
