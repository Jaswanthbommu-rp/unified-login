using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public class BulkProductsRemove
	{
		public long EditorPersonaId { get; set; }
		public List<long> SubjectUserPersonaList { get; set; }
		public List<int> ProductList { get; set; }
	}
}
