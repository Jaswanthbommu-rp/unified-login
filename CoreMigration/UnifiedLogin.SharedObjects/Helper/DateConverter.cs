using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnifiedLogin.SharedObjects.Helper
{
	/// <summary>
	/// 
	/// </summary>
	public class DateTimeFormatConverter : JsonConverter
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns></returns>
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(DateTime);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="objectType"></param>
		/// <param name="existingValue"></param>
		/// <param name="serializer"></param>
		/// <returns></returns>
		public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
		{
			if (reader.Value != null)
				return DateTime.ParseExact((string)reader.Value, "MMMM d yyyy h:mmtt", CultureInfo.InvariantCulture);

			return "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="value"></param>
		/// <param name="serializer"></param>
		public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
		{
			DateTime d = (DateTime)value;

			if (d.Ticks > 0)
			{
				writer.WriteValue(d.ToString("MMMM d yyyy h:mmtt"));
			}
			else
			{
				writer.WriteNull();
			}
		}
	}
}
