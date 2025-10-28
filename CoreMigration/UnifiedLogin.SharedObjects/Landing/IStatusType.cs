namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Status Type
	/// </summary>
	public interface IStatusType
	{
		/// <summary>
		/// Status Type Name
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Status Type Id
		/// </summary>
		int StatusTypeId { get; set; }
	}
}