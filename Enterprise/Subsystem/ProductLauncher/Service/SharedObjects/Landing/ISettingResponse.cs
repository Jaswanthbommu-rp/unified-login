using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public interface ISettingResponse
	{
		List<ISetting> Keys { get; set; }
		List<ISettingTable> Tables { get; set; }
	}
}
