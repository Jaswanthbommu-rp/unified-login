namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
	/// <summary>
	/// Interface for Right detail
	/// </summary>
	public interface IRight
	{
		/// <summary>
		/// Unique RightId
		/// </summary>
		int RightId { get; set; }

		/// <summary>
		/// Right Name
		/// </summary>
		string RightName { get; set; }

		/// <summary>
		/// Right ShortName (NickName)
		/// </summary>
		string RightNickName { get; set; }

		/// <summary>
		/// Right Value TypeId
		/// </summary>
		int RightValueTypeId { get; set; }
		/// <summary>
		/// THe id of the right
		/// </summary>
		int ID { get; set; }
		/// <summary>
		/// The description of the right
		/// </summary>
		string Description { get; set; }
	}
}