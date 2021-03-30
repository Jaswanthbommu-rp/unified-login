using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public interface ISettingTable
	{
		string name { get; set; }
		bool editable { get; set; }
		bool hidden { get; set; }
		string hideColumns { get; set; }
		List<ISettingRow> Value { get; set; }
	}
}
