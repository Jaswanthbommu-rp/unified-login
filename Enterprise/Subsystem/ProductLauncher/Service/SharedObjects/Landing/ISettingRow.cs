using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public interface ISettingRow
	{
		bool Editable { get; set; }
		bool Deletable { get; set; }
		bool Selectable { get; set; }
		List<Setting> Columns { get; set; }
	}
}
