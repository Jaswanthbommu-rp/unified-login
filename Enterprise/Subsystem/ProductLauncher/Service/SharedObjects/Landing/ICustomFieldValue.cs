namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public interface ICustomFieldValue
	{
		long FieldValueId { get; set; }
		long UserLoginPersonaId { get; set; }
		string Value { get; set; }
	}
}