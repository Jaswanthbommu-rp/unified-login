using Newtonsoft.Json;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.BlackBook
{
	public class PropertyInstanceAck
	{	
		public int Id { get; set; }
		public string PropertyInstanceSourceId { get; set; }
		public string Source { get; set; }
		public string PropertyName { get; set; }
		public string ModifiedBy { get; set; }		
	}
}