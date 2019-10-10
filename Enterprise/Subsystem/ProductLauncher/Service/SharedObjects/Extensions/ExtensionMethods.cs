using System;
using System.Linq;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Extensions
{
	/// <summary>
	/// Extension Methods
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Remove leading, middle, and trailing spaces from a string
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static string TrimWhiteSpace(this String str)
		{
			return new string(str.ToCharArray()
				.Where(c => !Char.IsWhiteSpace(c))
				.ToArray());
		}
	}
}
