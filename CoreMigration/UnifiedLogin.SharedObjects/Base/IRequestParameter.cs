namespace UnifiedLogin.SharedObjects.Base
{
	/// <summary>
	/// Used to filter, sort and limit the number of records being returned by the request
	/// </summary>
	public interface IRequestParameter
	{
		/// <summary>
		/// A list of key/value pairs to be used to filter the data in json format i.e. {"name":"john doe", "active": 0}
		/// </summary>
		System.Collections.Generic.Dictionary<string, string> FilterBy { get; set; }
		/// <summary>
		/// A list of key/value pairs to be used to sort the data in json format i.e. {"firstname":"asc", "inactive":"asc"}
		/// </summary>
		PageRequest Pages { get; set; }
		/// <summary>
		/// Contains the details about the number of records, start record and total rows being returned by the request
		/// </summary>
		System.Collections.Generic.Dictionary<string, string> SortBy { get; set; }
	}
}