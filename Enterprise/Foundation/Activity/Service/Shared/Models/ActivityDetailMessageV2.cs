using System.Runtime.Serialization;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models
{
	public class ActivityDetailMessageV2 : ActivityDetailMessage
	{
		[DataMember]
		public string Key { get; set; }

		[DataMember]
		public string Value { get; set; }

		[DataMember]
		public string OldValue { get; set; }

		[DataMember]
		public string NewValue { get; set; }
	}
}
