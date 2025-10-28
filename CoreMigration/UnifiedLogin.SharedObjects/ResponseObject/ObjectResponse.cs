using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.ResponseObject
{
	/// <summary>
	/// Used to return single object response
	/// </summary>
	public class ObjectResponse : ResponseBase
	{
		public object Data { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public ObjectResponseMeta Meta { get; set; }
	}

	public class ObjectResponseMeta
	{
		/// <summary>
		/// Key-Value pair - dictionary
		/// </summary>
		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public object Sources { get; set; }
	}
}
