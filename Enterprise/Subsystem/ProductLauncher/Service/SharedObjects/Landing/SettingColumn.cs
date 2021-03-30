using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public class SettingColumn : ISettingColumn
	{
		public SettingColumn()
		{

		}

		public string Name { get; set; }
		public string Value { get; set; }
	}
}
