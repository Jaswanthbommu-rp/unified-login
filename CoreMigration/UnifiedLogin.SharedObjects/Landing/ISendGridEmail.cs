using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// Interface for SendGrid Email
	/// </summary>
	public interface ISendGridEmail
	{
		/// <summary>
		/// List of BCC email address
		/// </summary>
		IList<EmailAddress> bccAddress { get; set; }

		/// <summary>
		/// Category: Custom Variable - To identify/Capture product specific emails
		/// </summary>
		string category { get; set; }

		/// <summary>
		/// List of CC email address
		/// </summary>
		IList<EmailAddress> ccAddress { get; set; }

		/// <summary>
		/// Email subject
		/// </summary>
		string emailSubject { get; set; }

		/// <summary>
		/// from email address: Supports user specified and Default email address
		/// </summary>
		EmailAddress fromAddress { get; set; }

		/// <summary>
		/// Message: Plain Text Email and HTML Email must be a string
		/// </summary>
		string message { get; set; }

		/// <summary>
		/// Email of High or Low Importance
		/// </summary>
		string priority { get; set; }

		/// <summary>
		/// List of To email address
		/// </summary>
		IList<EmailAddress> toAddress { get; set; }

		/// <summary>
		/// Email transactiuon Id
		/// </summary>
		string transId { get; set; }
	}

	/// <summary>
	/// Interface for Email address properties
	/// </summary>
	public interface IEmailAddress
	{
		/// <summary>
		/// Email address
		/// </summary>
		string email { get; set; }

		/// <summary>
		/// Specify the name to be used for this Address.
		/// </summary>
		string name { get; set; }
	}

	/// <summary>
	/// Interface for Email attachment properties
	/// </summary>
	public interface IAttachment
	{
		/// <summary>
		/// Attachement Content (e.g. base64 String)
		/// </summary>
		string attachmentContent { get; set; }

		/// <summary>
		/// Attachment File content type (e.g. image/csv)
		/// </summary>
		string attachmentContentType { get; set; }

		/// <summary>
		/// Attachment FileName
		/// </summary>
		string attachmentFileName { get; set; }
	}
}