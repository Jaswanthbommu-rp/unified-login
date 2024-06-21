using Newtonsoft.Json;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.ResponseObject
{
	/// <summary>
	/// Used to return single object response
	/// </summary>
	public class ObjectResponse : ResponseBase
	{
		public object Data { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
		public ObjectResponseMeta Meta { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long PersonaId { get; set; }
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
