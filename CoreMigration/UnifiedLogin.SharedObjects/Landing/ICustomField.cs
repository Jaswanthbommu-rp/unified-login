namespace UnifiedLogin.SharedObjects.Landing
{
	public interface ICustomField
	{
		string DefaultValue { get; set; }
		string Description { get; set; }
		bool Enabled { get; set; }
		long FieldId { get; set; }
		byte FieldTypeId { get; set; }
		string FieldTypeName { get; set; }
		string HelpText { get; set; }
		int? MaxCharLength { get; set; }
		int? MinCharLength { get; set; }
		string Name { get; set; }
		long OrganizationId { get; set; }
		bool? ReadOnly { get; set; }
		bool? Required { get; set; }
		short Sequence { get; set; }
		string SyncField { get; set; }
	}
}