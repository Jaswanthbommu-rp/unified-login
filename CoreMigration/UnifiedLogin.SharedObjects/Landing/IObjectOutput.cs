namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for ObjectOutput
	/// </summary>
	/// <typeparam name="T1">data Generic type</typeparam>
	/// <typeparam name="T2">error Generic type</typeparam>
	public interface IObjectOutput<T1, T2>
	{
		/// <summary>
		/// Error status object - API/UI Call Success/Error Communication
		/// </summary>
		Status<T2> Status { get; set; }

		/// <summary>
		/// List of collection of data
		/// </summary>
		T1 obj { get; set; }
	}
}