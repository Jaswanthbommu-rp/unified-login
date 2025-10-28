using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Status Type
	/// </summary>
	public class StatusType : IStatusType
	{
		/// <summary>
		/// Status Type Id
		/// </summary>
		[JsonProperty(PropertyName = "StatusTypeId")]
		public int StatusTypeId { get; set; }

		/// <summary>
		/// Status Type Name
		/// </summary>
		[JsonProperty(PropertyName = "Name")]
		public string Name { get; set; }
	}
}
