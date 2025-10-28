using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.BlackBook;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Used when creating a new Organization
	/// </summary>
	public class OrganizationCreate
	{
		/// <summary>
		/// Used to store the books id for the company
		/// </summary>
		[Required(ErrorMessage = "The books company id is required.", AllowEmptyStrings = false)]
		public long? BooksCompanyId { get; set; }

        /// <summary>
		/// Used to store the books id for the company
		/// </summary>
		[Required(ErrorMessage = "The books company id is required.", AllowEmptyStrings = false)]
        public long? BooksCustomerMasterId { get; set; }

		/// <summary>
		/// Used to store the company type id for the company
		/// </summary>
		[Required(ErrorMessage = "The Organization Type Id is required")]
		[Range(1, int.MaxValue, ErrorMessage = "Please enter a Organization Type Id greater than {0}")]
		public int OrganizationTypeId { get; set; }

        /// <summary>
        /// Used to store the domain for the company
        /// </summary>
        [Required(ErrorMessage = "The Organization Domain Id is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a Organization Domain Id greater than {0}")]
        public int OrganizationDomainId { get; set; } = 1;

        /// <summary>
        /// Used to store the domain for the company
        /// </summary>
        [Required(ErrorMessage = "The Organization Domain is required")]
        public string OrganizationDomain { get; set; } = "Primary";

		/// <summary>
		/// The name of the Organization
		/// </summary>
		[Required(ErrorMessage = "The company name is required.", AllowEmptyStrings = false)]
		[JsonProperty(PropertyName = "name")]
		[StringLength(150)]
		public string Name { get; set; }

		/// <summary>
		/// The list of products assigned to the organization. OS-OneSite, L2L-Lead2Lease, LVL1-LevelOne, etc.
		/// </summary>
		public List<string> Products { get; set; }

		/// <summary>
		/// The initial super user to create for the company
		/// </summary>
		[Required(ErrorMessage = "The company administrator user info is required.")]
		public OrganizationAdminUser AdminUser { get; set; }

		/// <summary>
		/// The additional Company admin user (Included in the RabbitMQ Message from BlueBook)
		/// </summary>
		public OrganizationAdminUser CompanyAdminUser { get; set; }

        public string CompanyInstancePartner { get; set; } = null;

        public string CompanyInstancePartnerSourceId { get; set; } = null;

		public CompanyInstanceAddress CompanyAddress { get; set; }

		/// <summary>
		/// IsActive
		/// </summary>		
		public int IsActive { get; set; } = 1;

        /// <summary>
        /// Use Primary Properties
        /// </summary>		
        public int EnablePrimaryProperties { get; set; } = 0;

        /// <summary>
        /// Use Enterprise Roles
        /// </summary>		
        public int EnableEnterpriseRoles { get; set; } = 0;
    }

	/// <summary>
	/// Used to create a new Organization administrator
	/// </summary>
	public class OrganizationAdminUser
	{
		/// <summary>
		/// The first name of administrator
		/// </summary>
		[Required(ErrorMessage = "The company administrator first name is required.", AllowEmptyStrings = false)]
		[StringLength(100)]
		public string FirstName { get; set; }

		/// <summary>
		/// The last name of the administrator
		/// </summary>
		[Required(ErrorMessage = "The company administrator last name is required.", AllowEmptyStrings = false)]
		[StringLength(100)]
		public string LastName { get; set; }

		/// <summary>
		/// The title of the administrator
		/// </summary>
		[StringLength(100)]
		public string Title { get; set; }

		/// <summary>
		/// The suffix for the administrator, Jr, Sr. etc
		/// </summary>
		[StringLength(20)]
		public string Suffix { get; set; }

		/// <summary>
		/// The valid working email address of the administrator
		/// </summary>
		[Required(ErrorMessage = "The company administrator email address is required.", AllowEmptyStrings = false)]
		[StringLength(255)]
		public string Email { get; set; }

        /// <summary>
        /// Roleid list
        /// </summary>
        public List<string> RoleIds { get; set; }

    }
}
