using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Batch
{
	public class EnterpriseRoleBatch
	{
		public int EnterpriseRoleBatchProcessId { get; set; }
		public long EditorUserPersonaId { get; set; }
		public long SubjectUserPersonaId { get; set; }
		public int EnterpriseRoleTemplateId { get; set; }
		public int StatusTypeId { get; set; }
		public int BatchProcessTypeId { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
