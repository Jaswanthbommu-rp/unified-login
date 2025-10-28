using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
	public class PropertyInstanceAck
	{	
		public int Id { get; set; }
		public string PropertyInstanceSourceId { get; set; }
		public string Source { get; set; }
		public string PropertyName { get; set; }
		public PropertyInstanceAddress Address { get; set; }
		public bool IsActive { get; set; }
		public string ModifiedBy { get; set; }		
	}

    public class BulkPropertyInstanceStatusAck
    {  
        public List<string> propertyInstanceSourceIds { get; set; }  
        public bool Status { get; set; }
        public string ModifiedBy { get; set; }
    }
}