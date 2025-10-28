using System;

namespace UnifiedLogin.SharedObjects.Exceptions
{
	/// <summary>
	/// Used to throw blue book specific exception so callie can handle accordingly.
	/// </summary>
	public class BlueBookException : Exception
	{
		public BlueBookException()
		{
		}

		public BlueBookException(string message)
			: base(message)
		{
		}

		public BlueBookException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected BlueBookException(System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
