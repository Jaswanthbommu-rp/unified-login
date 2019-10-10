using System.Diagnostics.CodeAnalysis;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
	/// <summary>
	/// Used in Status.cs Contain any data (e.g. Details about the number of rows affected or read by the query)
	/// necessary to construct the error message shown to the user if applicable
	/// </summary>
	[ExcludeFromCodeCoverage]
	public class ErrorData : IErrorData
	{
	}
}
