using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
	/// <summary>
	/// SendGrid Email
	/// </summary>
	public class SendGridEmail : ISendGridEmail
	{
		/// <summary>
		/// Email Attachment
		/// </summary>
		[JsonProperty("attachment", NullValueHandling = NullValueHandling.Ignore)]
		public Attachment attachment{ get; set; }

		/// <summary>
		/// List of BCC email address
		/// </summary>
		[JsonProperty("bccAddress", NullValueHandling = NullValueHandling.Ignore)]
		public IList<EmailAddress> bccAddress { get; set; }

		/// <summary>
		/// Category: Custom Variable - To identify/Capture product specific emails
		/// </summary>
		public string category { get; set; }

		/// <summary>
		/// List of CC email address
		/// </summary>
		[JsonProperty("ccAddress", NullValueHandling = NullValueHandling.Ignore)]
		public IList<EmailAddress> ccAddress { get; set; }

		/// <summary>
		/// Email subject
		/// </summary>
		public string emailSubject { get; set; }

		/// <summary>
		/// from email address: Supports user specified and Default email address
		/// </summary>
		public EmailAddress fromAddress { get; set; }

		/// <summary>
		/// Message: Plain Text Email and HTML Email must be a string
		/// </summary>
		public string message { get; set; }

		/// <summary>
		/// Email of High or Low Importance
		/// </summary>
		public string priority { get; set; } = "low";

		/// <summary>
		/// List of To email address
		/// </summary>
		public IList<EmailAddress> toAddress { get; set; }

		/// <summary>
		/// Email transactiuon Id
		/// </summary>
		public string transId { get; set; }
	}

	/// <summary>
	/// Email address properties
	/// </summary>
	public class EmailAddress : IEmailAddress
	{
		/// <summary>
		/// Email address
		/// </summary>
		public string email { get; set; }

		/// <summary>
		/// Specify the name to be used for this Address.
		/// </summary>
		public string name { get; set; }
	}

	/// <summary>
	/// Email attachment properties
	/// </summary>
	public class Attachment : IAttachment
	{
		/// <summary>
		/// Attachment FileName
		/// </summary>
		public string attachmentFileName { get; set; }

		/// <summary>
		/// Attachment File content type (e.g. image/csv)
		/// </summary>
		public string attachmentContentType { get; set; }

		/// <summary>
		/// Attachement Content (e.g. base64 String)
		/// </summary>
		public string attachmentContent { get; set; }
	}
}
