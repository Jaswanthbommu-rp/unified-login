namespace UnifiedLogin.SharedObjects.Landing.Export
{
	/// <summary>
	/// Interface for User data to Export
	/// </summary>
	public interface IUser
	{
		/// <summary>
		/// Enabled Custom Field value with the smallest sequence
		/// </summary>
		string CustomField { get; set; }

		/// <summary>
		/// Firstname
		/// </summary>
		string FirstName { get; set; }

		/// <summary>
		/// User last login date
		/// </summary>
		string LastLogin { get; set; }

		/// <summary>
		/// Lastname
		/// </summary>
		string LastName { get; set; }

		/// <summary>
		/// LoginName
		/// </summary>
		string LoginName { get; set; }

		/// <summary>
		/// Middle initial
		/// </summary>
		string MiddleName { get; set; }

		/// <summary>
		/// Sortable user statuses (Active|Disabled|Pending|Expired).
		/// </summary>
		string Status { get; set; }

		/// <summary>
		/// Use third party identity service provider.  Default to true so that the toggle switch is on on the Add new User.
		/// </summary>
		string IDP { get; set; }

		/// <summary>
		/// Number products the user is authorized to access 
		/// </summary>
		int Products { get; set; }

		/// <summary>
		/// User type as it appears on the User details such Regular User, Regular User (no email), RealPage System Administrator
		/// </summary>
		string UserType { get; set; }

		/// <summary>
		/// When the account can be used
		/// </summary>
		string EffectiveDate { get; set; }

		/// <summary>
		/// When the account can no longer be used
		/// </summary>
		string ExpireDate { get; set; }

		/// <summary>
		/// Operator
		/// </summary>		
		string Operator { get; set; }

		/// <summary>
		/// UserRelationshipType
		/// </summary>		
		string UserRelationshipType { get; set; }

		/// <summary>
		/// CompanyName
		/// </summary>		
		string CompanyName { get; set; }
	}

}