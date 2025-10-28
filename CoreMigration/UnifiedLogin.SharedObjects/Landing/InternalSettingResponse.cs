using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Landing
{
	public class InternalSettingResponse
	{
		public List<Setting> Keys { get; set; } = new List<Setting>();
		public List<SettingTable> Tables { get; set; } = new List<SettingTable>();
	}



	public class SettingTable
	{
		public SettingTable()
		{
			editable = true;
			hidden = false;
			hideColumns = string.Empty;
		}
		
		public long Id { get; set; }
		public string name { get; set; }
		public string hideColumns { get; set; }
		public bool editable { get; set; }
		public bool hidden { get; set; }
		public List<SettingRow> Value { get; set; }
	}

	public class SettingRow
	{
		public SettingRow()
		{
			Editable = true;
			Deletable = true;
			Selectable = true;
		}
		
		public long Id { get; set; }
		public bool Editable { get; set; }
		public bool Deletable { get; set; }
		public bool Selectable { get; set; }
		public List<Setting> Columns { get; set; }

		public static List<SettingRow> GetSettingRows()
		{
			List<SettingRow> rows = new List<SettingRow>();

			SettingRow row = new SettingRow()
			{
				Editable = true,
				Deletable = true,
				Selectable = true,
				Columns = GetSettingColumns()
			};

			rows.Add(row);

			return rows;
		}

		public static List<Setting> GetSettingColumns()
		{
			List<Setting> columns = new List<Setting>();

			Setting columnSetting = new Setting()
			{
				Name = "ID",
				Value = "999",
				Editable = true
			};

			columns.Add(columnSetting);

			columnSetting = new Setting()
			{
				Name = "StandardName",
				Value = "Time zone Mock Data",
				Editable = true
			};

			columns.Add(columnSetting);

			return columns;
		}
	}
}
