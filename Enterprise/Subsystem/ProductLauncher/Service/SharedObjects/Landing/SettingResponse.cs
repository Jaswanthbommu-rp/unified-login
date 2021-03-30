using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	public class SettingResponse : ISettingResponse
	{
		public List<ISetting> Keys { get; set; }
		public List<ISettingTable> Tables { get; set; }

		#region Examples

		public static List<ISetting> GetKeys()
		{
			List<ISetting> settings = new List<ISetting>();

			ISetting setting = new Setting()
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

		public static List<ISettingTable> GetTableSettings()
		{
			List<ISettingTable> tableSettings = new List<ISettingTable>();

			ISettingTable tableSetting = new SettingTable()
			{
				name = "customfield",
				Value = GetSettingRows()
			};

			tableSettings.Add(tableSetting);

			return tableSettings;
		}

		public static List<ISettingRow> GetSettingRows()
		{
			List<ISettingRow> rows = new List<ISettingRow>();

			ISettingRow row = new SettingRow()
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

		public static ISettingResponse SettingResponseExampleForKeys()
		{
			ISettingResponse settingResponse = new SettingResponse();
			settingResponse.Keys = GetKeys();
			return settingResponse;
		}

		public static ISettingResponse SettingResponseExampleForTable()
		{
			ISettingResponse settingResponse = new SettingResponse();
			settingResponse.Tables = GetTableSettings();
			return settingResponse;
		}
		#endregion

	}
}
