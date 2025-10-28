namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	public class ClientClaim
	{
		public int Id { get; set; }

		public int ClientId { get; set; }

		public string Type { get; set; }

		public string Value { get; set; }

		public int ProductId { get; set; }

		public string ProductName { get; set; }
	}
}
