using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Hots
{
	public class BaseLineCustomerCompanyUser
	{
		public long UserId { get; set; }
		public long PersonaId { get; set; }
		public Guid UserRealPageId { get; set; }
		public long AdminUserPersonaId { get; set; }
	}
}
