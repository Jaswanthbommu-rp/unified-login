using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// UI Output JSON.  Used for the result UI.
	/// </summary>
	/// <typeparam name="T1">data Generic type</typeparam>
	/// <typeparam name="T2">error Generic type</typeparam>
	[ExcludeFromCodeCoverage]
	public class ObjectOutput<T1, T2> : IObjectOutput<T1, T2>
	{
		/// <summary>
		/// Electronic Address data
		/// </summary>
		[JsonProperty(PropertyName = "data")]
		public T1 obj { get; set; }

		/// <summary>
		/// Error status object - API/UI Call Success/Error Communication
		/// </summary>
		[JsonProperty("Status", NullValueHandling = NullValueHandling.Ignore)]
		public Status<T2> Status { get; set; }
	}
}
