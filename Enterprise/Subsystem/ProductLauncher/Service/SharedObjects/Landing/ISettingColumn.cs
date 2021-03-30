using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public interface ISettingColumn
	{
		string Name { get; set; }
		string Value { get; set; }
	}
}
