using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Enterprise
{
	public class SettingResponse
	{
		public List<Setting> Keys { get; set; }
		public List<SettingTable> Tables { get; set; }

		#region Examples

		public static List<Setting> GetKeys()
		{
			List<Setting> settings = new List<Setting>();

			Setting setting = new Setting()
			{
				Name = "mfa",
				Value = "ACTIVE",
				Editable = true
			};

			settings.Add(setting);

			setting = new Setting()
			{
				Name = "mfa",
				Value = "1",
				Editable = true
			};

			settings.Add(setting);

			return settings;
		}

		public static List<SettingTable> GetTableSettings()
		{
			List<SettingTable> tableSettings = new List<SettingTable>();

			SettingTable tableSetting = new SettingTable()
			{
				name = "customfield",
				Value = GetSettingRows()
			};

			tableSettings.Add(tableSetting);

			return tableSettings;
		}

		public static List<SettingRow> GetSettingRows()
		{
			List<SettingRow> rows = new List<SettingRow>();

			SettingRow row = new SettingRow()
			{
				Editable = true,
				Deletable = true,
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
				Name = "Name",
				Value = "Central Standard Time Chicago(GMT - 6)",
				Editable = true
			};

			columns.Add(columnSetting);

			return columns;
		}

		public static SettingResponse SettingResponseExampleForKeys()
		{
			SettingResponse settingResponse = new SettingResponse();
			settingResponse.Keys = GetKeys();
			return settingResponse;
		}

		public static SettingResponse SettingResponseExampleForTable()
		{
			SettingResponse settingResponse = new SettingResponse();
			settingResponse.Tables = GetTableSettings();
			return settingResponse;
		}
		#endregion
	}
}
