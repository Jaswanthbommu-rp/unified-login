namespace UnifiedLogin.SharedObjects.Landing
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
	}
}