namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
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
	}
}
