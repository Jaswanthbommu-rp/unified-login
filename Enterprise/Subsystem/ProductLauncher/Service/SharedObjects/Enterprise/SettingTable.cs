using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise
{
	public class SettingTable
	{
		public SettingTable()
		{
			editable = true;
			hidden = false;
			hideColumns = string.Empty;
		}
		[JsonIgnore]
		public long Id { get; set; }
		public string name { get; set; }
		public string hideColumns { get; set; }
		public bool editable { get; set; }
		public bool hidden { get; set; }
		public List<SettingRow> Value { get; set; }
	}
}
