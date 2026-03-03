using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing
{
	public class CustomField : ICustomField
	{
		public long FieldId { get; set; }

		public long OrganizationId { get; set; }

		public bool Enabled { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public byte FieldTypeId { get; set; }

		public string FieldTypeName { get; set; }

		public bool? Required { get; set; }

		public bool? ReadOnly { get; set; }

		public string DefaultValue { get; set; }

		public string SyncField { get; set; }

		public short Sequence { get; set; }

		public string HelpText { get; set; }

		public int? MinCharLength { get; set; }

		public int? MaxCharLength { get; set; }
	}

	public class CustomFieldValue : CustomField, ICustomFieldValue
	{
		public long? FieldValueId { get; set; }

		public long UserLoginPersonaId { get; set; }

		public string Value { get; set; }
	}

}
