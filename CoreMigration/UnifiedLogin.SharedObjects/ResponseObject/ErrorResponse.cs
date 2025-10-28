using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.ResponseObject
{
	public class ErrorResponse
	{
		public IList<Error> Errors { get; set; }
	}
}
