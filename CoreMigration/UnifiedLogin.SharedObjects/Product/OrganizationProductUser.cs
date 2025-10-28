using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product
{
	public class OrganizationProductUser
	{
		public int UserId { get; set; }
		/// <summary>
		/// The login name of the product user
		/// </summary>
		public string LoginNmae { get; set; }
		/// <summary>
		/// The users first name
		/// </summary>		
		public string FirstName { get; set; }
		/// <summary>
		/// The users last name
		/// </summary>		
		public string LastName { get; set; }
		/// <summary>
		/// ProductId
		/// </summary>		
		public int ProductId { get; set; }
		/// <summary>
		/// The name of the product user
		/// </summary>
		public string ProductUserName { get; set; }
	}
}
