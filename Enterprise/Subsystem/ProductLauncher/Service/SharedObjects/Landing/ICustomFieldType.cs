namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public interface ICustomFieldType
	{
		string Description { get; set; }
		long FieldTypeId { get; set; }
		string Name { get; set; }
	}
}