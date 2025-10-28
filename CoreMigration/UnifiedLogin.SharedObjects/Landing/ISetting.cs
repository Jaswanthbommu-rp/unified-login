namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for Security Setting
	/// </summary>
	public interface ISetting
	{
		/// <summary>
		/// SecuritySetting Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// SecuritySetting Value
		/// </summary>
		string Value { get; set; }

		/// <summary>
		/// Access right (0-Read-Write Access, 1-Read Only Access, 2- No Access)
		/// </summary>
		int Right { get; set; }

		/// <summary>
		/// Is Setting Editable
		/// </summary>
		bool Editable { get; set; }
		/// <summary>
		/// Is Setting Hidden
		/// </summary>
		bool Hidden { get; set; }

		/// <summary>
		/// Category
		/// </summary>
		string Category { get; set; }
	}
}