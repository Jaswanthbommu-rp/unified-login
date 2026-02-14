using System.Security.Claims;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Use to hold user claim related information
    /// </summary>
    public class DefaultUserClaim
    {
        public static readonly Guid EmployeeCompanyRealPageId = new Guid("0D018E46-C20E-477D-ADED-4E5A35FB8F99");
        public static readonly Guid ExternalCompanyRealPageId = new Guid("EEFACE50-9F75-4DCE-B133-A97EE0E0D723");
        public static readonly Guid ContractCompanyRealPageId = new Guid("10F5A427-4636-4F47-840E-6212BD842BC0");

        /// <summary>
        /// Default constructor
        /// </summary>
        public DefaultUserClaim() { }

        /// <summary>
        /// Use the passed claim to set the values
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        public DefaultUserClaim(ClaimsPrincipal claimsPrincipal)
        {
            UserId = Convert.ToInt32((from nvp in claimsPrincipal.Claims where (nvp.Type.Equals("sub", StringComparison.OrdinalIgnoreCase) || nvp.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase)) select nvp.Value).FirstOrDefault());
            OrganizationPartyId = Convert.ToInt32((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("orgPartyId", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());
            LoginName = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("LOGINNAME", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();
            OrganizationMasterId = Convert.ToInt64((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("ORGMASTERID", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());
            CustomerMasterId = Convert.ToInt64((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("orgCompanyMasterId", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());
            OrganizationName = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("ORGNAME", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();
            OrganizationType = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("orgType", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();
            Roles = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("ROLE", StringComparison.OrdinalIgnoreCase) || nvp.Type.Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();
            FirstName = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("FIRSTNAME", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();
            LastName = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("LASTNAME", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();
            ClientCode = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("CLIENT_ID", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();
            Rights = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("RIGHT", StringComparison.OrdinalIgnoreCase) select nvp.Value).ToList();
            PersonaId = Convert.ToInt32((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("PERSONAID", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());
            IsRPEmployee = Convert.ToBoolean((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("isRPEmployee", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());


            Guid realGuid;
            if (Guid.TryParse((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("realPageId", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault(), out realGuid))
            {
                UserRealPageGuid = realGuid;
            }

            Guid correlationId;
            if (Guid.TryParse((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("CORRELATIONID", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault(), out correlationId))
            { CorrelationId = correlationId; }
            else
            {
                CorrelationId = Guid.NewGuid();
            }

            Guid organizationRealPageId;
            if (Guid.TryParse((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("ORGID", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault(), out organizationRealPageId))
            {
                OrganizationRealPageGuid = organizationRealPageId;
            }
            RealPageEmployee = organizationRealPageId == EmployeeCompanyRealPageId;

            Guid impersonatedBy;
            if (Guid.TryParse((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("IMPERSONATEDBY", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault(), out impersonatedBy))
            {
                ImpersonatedBy = impersonatedBy;
            }

            ImpersonatedByName = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("ImpersonatedByName", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();

        }

        /// <summary>
        /// The users id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The id used to track the users actions
        /// </summary>
        public Guid CorrelationId { get; set; }

        /// <summary>
        /// The guid for the user
        /// </summary>
        public Guid UserRealPageGuid { get; set; }

        /// <summary>
        /// The login name for the user
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// The guid for the users organization
        /// </summary>
        public Guid OrganizationRealPageGuid { get; set; }

        /// <summary>
        /// The int id of the users organization
        /// </summary>
        public long OrganizationPartyId { get; set; }

        /// <summary>
        /// The name of the users organization
        /// </summary>
        public string OrganizationName { get; set; } = "";

        /// <summary>
        /// The name of the users organization type
        /// </summary>
        public string OrganizationType { get; set; } = "";

        /// <summary>
        /// The books id of the users organization
        /// </summary>
        public long OrganizationMasterId { get; set; }

        /// <summary>
        /// The Bluebook id of the users organization
        /// </summary>
        public long CustomerMasterId { get; set; }

        /// <summary>
        /// User roles for the given organization
        /// </summary>
        public string Roles { get; set; }

        /// <summary>
        /// User rights for the given role
        /// </summary>
        public List<string> Rights { get; set; }
        /// <summary>
        /// User First Name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// User Last Name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// ClientCode
        /// </summary>
        public string ClientCode { get; set; }

        /// <summary>
        /// Persona Id
        /// </summary>
        public long PersonaId { get; set; }

        /// <summary>
        /// Used to indicate if the user is a RealPage employee
        /// </summary>
        public bool RealPageEmployee { get; set; }

        /// <summary>
        /// The id used to track the user that is impersonating a user
        /// </summary>
        public Guid ImpersonatedBy { get; set; }

        /// <summary>
	    /// The id used to track the user that is impersonating a user
	    /// </summary>
	    public string ImpersonatedByName { get; set; }

        /// <summary>
	    /// Flag used to determine if RP employee is logged in
	    /// </summary>
        public bool IsRPEmployee { get; set; }
        public int RoleId { get; set; }
    }
}
