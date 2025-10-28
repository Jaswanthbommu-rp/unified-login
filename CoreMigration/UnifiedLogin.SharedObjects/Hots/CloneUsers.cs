using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Hots
{
	public class CloneUsers
	{
		//public Guid BaseLineCustomerCompanyId { get; set; }
		public Guid CloneCustomerUPFMId { get; set; }
		public string CloneCustomerEnvironment { get; set; }
		//public List<CustomerPropertyIdMap> CustomerPropertyIdMap { get; set; }
	}

	public class CustomerPropertyIdMap
	{
		public int BaselineCustomerPropertyId { get; set; }
		public int CloneCustomerPropertyId { get; set; }
	}
}
