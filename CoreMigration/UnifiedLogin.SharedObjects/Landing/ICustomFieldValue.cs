namespace UnifiedLogin.SharedObjects.Landing
{
	public interface ICustomFieldValue
	{
		long FieldValueId { get; set; }
		long UserLoginPersonaId { get; set; }
		string Value { get; set; }
	}
}