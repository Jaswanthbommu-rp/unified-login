using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.BlackBook
{
	/// <summary>
	/// EasyELM (ELMS) attributes
	/// </summary>
	public class InstanceAttribute
	{
		/// <summary>
		/// The instance id for the company
		/// </summary>
		public int CompanyInstanceId { get; set; }

		/// <summary>
		/// Property InstanceId
		/// </summary>
		public int PropertyInstanceId { get; set; }

		/// <summary>
		/// Product's CompanyId
		/// </summary>
		public string CompanyInstanceSourceId { get; set; }

		/// <summary>
		/// Property Instance SourceId
		/// </summary>
		public string PropertyInstanceSourceId { get; set; }

		/// <summary>
		/// Source
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// CreatedBy
		/// </summary>
		public string CreatedBy { get; set; }

		/// <summary>
		/// Attribute Name (e.g. API Code, API Key)
		/// </summary>
		public string AttributeName { get; set; }

		/// <summary>
		/// AttributeType (e.g. Unified Login)
		/// </summary>
		public string AttributeType { get; set; }

		/// <summary>
		/// Attribute Value (e.g. 113745, PPOFNRCAZCVQQOS1NI)
		/// </summary>
		public string AttributeValue { get; set; }
	}
}
