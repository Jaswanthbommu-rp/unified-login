using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	public class UserDetails
	{
		public int PersonaId { get; set; }
		public long PersonPartyId { get; set; }
		public string LoginName { get; set; }
		public int UserId { get; set; }
		public Guid UserRealPageId { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public int BooksMasterId { get; set; }
        public int BooksCustomerMasterId { get; set; }
        public string ProductUserName { get; set; } // Saml
		public string ProductUserId { get; set; } // Saml
		public string UserRoleType { get; set; }
		public int UserRoleTypeId { get; set; }
		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public int OrganizationPartyId { get; set; }
		public string OrganizationDomain { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ThruDate { get; set; }
		public bool? IsActive { get; set; } = false;
		public List<string> PhoneNumbers { get; set; } = new List<string>();//SLM
        public bool IsRPEmployee { get; set; }
    }
}
