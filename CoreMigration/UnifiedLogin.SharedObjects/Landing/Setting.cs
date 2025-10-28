namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Security Setting
	/// </summary>
	public class Setting : ISetting
	{
		/// <summary>
		/// SecuritySetting Name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// SecuritySetting Value
		/// </summary>
		public string Value { get; set; }

		/// <summary>
		/// Access right (0-Read-Write Access, 1-Read Only Access, 2- No Access)
		/// </summary>
		public int Right { get; set; } = 0;

		/// <summary>
		/// Is Setting Editable
		/// </summary>
		public bool Editable { get; set; } = true;
		/// <summary>
		/// Is Setting Hidden
		/// </summary>
		public bool Hidden { get; set; } = false;

		/// <summary>
		/// Category
		/// </summary>
		public string Category { get; set; } = string.Empty;
	}
}
