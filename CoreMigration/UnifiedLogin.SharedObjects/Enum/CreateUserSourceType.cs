using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Enum
{
	/// <summary>
	/// User Creation Source types enum
	/// </summary>
	public enum CreateUserSourceType
	{
		UnifiedPlatform = 24,
		ExcelImport = 25,
		MigrationTool = 26,
		RPX = 27
	}
}
