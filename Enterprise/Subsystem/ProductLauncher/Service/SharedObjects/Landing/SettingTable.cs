using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public class SettingTable : ISettingTable
	{
		public SettingTable()
		{
			editable = true;
			hidden = false;
			hideColumns = string.Empty;
		}
		public string name { get; set; }
		public string hideColumns { get; set; }
		public bool editable { get; set; }
		public bool hidden { get; set; }
		public List<ISettingRow> Value { get; set; }
	}
}
