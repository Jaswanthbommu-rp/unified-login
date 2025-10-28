using UnifiedLogin.SharedObjects.Enum;
using UnifiedLogin.SharedObjects.Landing;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
	/// <summary>
	/// Interface for Profile object (Composite object of Person, UserLogin, Contact)
	/// </summary>
	public interface IProfileDetail : IPerson
	{
		/// <summary>
		/// Products assigned to user
		/// </summary>
		IList<PersonaProductUserDetails> AssignedProducts { get; set; }

		/// <summary>
		/// Identity Provider
		/// </summary>
		string AuthenticationType { get; set; }

		/// <summary>
		/// Avatar
		/// </summary>
		string Avatar { get; set; }

		/// <summary>
		/// Contact Mechanism for a person attributes
		/// </summary>
		IList<CommonAddress> contactMechanism { get; set; }

		/// <summary>
		/// Persona disassociated with the Profile
		/// </summary>
		IList<Persona> InactivePersona { get; set; }

		/// <summary>
		/// Password Expiration Detail  
		/// </summary>
		CheckPasswordExpirationResponse PasswordExpirationDetail { get; set; }

		/// <summary>
		/// Notification Email
		/// </summary>
		string NotificationEmail { get; set; }

		/// <summary>
		/// List of organization for a user
		/// </summary>
		IList<Organization> organization { get; set; }

		/// <summary>
		/// PartyRole (e.g. User Job Title) attributes
		/// </summary>
		PartyRole PartyRole { get; set; }

		/// <summary>
		/// Initial password of the user
		/// </summary>
		string Password { get; set; }

		/// <summary>
		/// Persona associated with the Profile
		/// </summary>
		IList<Persona> Persona { get; set; }

		/// <summary>
		/// ProductBatch attributes
		/// </summary>
		IList<ProductBatch> productBatch { get; set; }

		/// <summary>
		/// Summary count of properties, products and roles associated to user
		/// </summary>
		SummaryCounts SummaryCount { get; set; }

		/// <summary>
		/// Contact mechanisim telecommunication number attributes
		/// </summary>
		IList<TelecommunicationNumber> TelecommunicationNumber { get; set; }

        /// <summary>
        /// UserLogin attributes
        /// </summary>
        IUserLogin userLogin { get; set; }

		/// <summary>
		/// User Type (Regular User, SuperUser, Regular User with no email)
		/// </summary>
		int UserTypeId { get; set; }

		/// <summary>
		/// Get or set verification activity token
		/// </summary>
		string VerificationActivityToken { get; set; }
        /// <summary>
        /// Used to get  organization level settings
        /// </summary>
        List<OrganizationSetting> OrganizationSettings { get; set; }

		/// <summary>
		/// User Custom fields attributes
		/// </summary>
		IList<CustomFieldValue> CustomFields { get; set; }

		/// <summary>
		/// Custom Field string
		/// </summary>
		string CustomField { get; set; }

		/// <summary>
		/// CreateUserSourceType
		/// </summary>
		CreateUserSourceType? CreateUserSourceType { get; set; }

		/// <summary>
		/// Total number of records count (without any paging if the response is limited by paging)
		/// </summary>
		int TotalRecords { get; set; }

		/// <summary>
		/// EmployeeId
		/// </summary>
		int UserEmployeeId { get; set; }

        /// <summary>
        /// SuperVisorUserId
        /// </summary>
        long SuperVisorUserId { get; set; }

        /// <summary>
        /// SuperVisor information object
        /// </summary>
        UserInfoLite SuperVisorUser { get; set; }

        /// <summary>
        /// User Enterprise Role Template Id
        /// </summary>
        int RoleTemplateId { get; set; }

		/// <summary>
		/// User Enterprise Role Name
		/// </summary>		
		string EntepriseRoleName { get; set; }
		bool PersonaHasProductError { get; set; }


		/// <summary>
		/// ExternalUserRelationship
		/// </summary>
		ExternalUserRelationship ExternalUserRelationship { get; set; }

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

		bool IsDelegateAdmin { get; set; }

		bool IsRealPartner { get; set; }

		/// <summary>
		/// DelegateRoleTemplate
		/// </summary>
		DelegateRoleTemplate DelegateRoleTemplate { get; set;}

    }
}