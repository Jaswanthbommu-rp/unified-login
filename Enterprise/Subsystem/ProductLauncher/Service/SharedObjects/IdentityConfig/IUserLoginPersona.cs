using System;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.IdentityConfig
{
	public interface IUserLoginPersona
	{
		DateTime? FromDate { get; set; }
		bool PrimaryOrganization { get; set; }
		DateTime? StatusThruDate { get; set; }
		int StatusTypeId { get; set; }
		DateTime? ThruDate { get; set; }
		long UserLoginId { get; set; }
		long UserLoginPersonaId { get; set; }
	}
}