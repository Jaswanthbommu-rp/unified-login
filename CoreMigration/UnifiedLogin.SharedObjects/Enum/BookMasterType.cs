using System.ComponentModel;

namespace UnifiedLogin.SharedObjects.Enum
{
	/// <summary>
	/// Book Master Types
	/// </summary>
	public enum BookMasterType : int
	{
		/// <summary>
		/// BlackBook Integration Application
		/// </summary>
		[Description("BlackBook MasterId")]
		CompanyMasterId = 1,

		/// <summary>
		/// BlueBook Integration application
		/// </summary>
		[Description("BlueBook MasterId")]
		CustomerMasterId = 2,
	}
}
