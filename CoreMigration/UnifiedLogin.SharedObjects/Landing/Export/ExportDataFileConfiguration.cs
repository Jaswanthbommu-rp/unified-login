using Newtonsoft.Json;

namespace UnifiedLogin.SharedObjects.Landing.Export
{
	/// <summary>
	/// ExportConfiguration
	/// </summary>
	public class ExportDataFileConfiguration
	{
		/// <summary>
		/// File Header
		/// </summary>
		public string Header { get; set; }

		/// <summary>
		///Mapped Field
		/// </summary>
		public string MappedField { get; set; }

		/// <summary>
		/// PDF Column Width
		/// </summary>
		public string PDFColumnWidth { get; set; }

		/// <summary>
		/// Preference is to specify order of columns
		/// </summary>
		public int Preference { get; set; }
	}
}
