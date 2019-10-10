namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects
{
	public interface IOrganizationClientUserClaim
	{
		string ClientId { get; set; }
		int Id { get; set; }
		int OrganizationId { get; set; }
		Scope Scope { get; set; }
		string Type { get; set; }
		int UserId { get; set; }
		string Value { get; set; }
	}
}