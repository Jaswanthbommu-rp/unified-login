using System;
using System.Linq;
using System.Security.Claims;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Models
{
    /// <summary>
    /// Use to hold user claim related information
    /// </summary>
    public class DefaultUserClaim
    {
        /// <summary>
        /// Use the passed claim to set the values
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        public DefaultUserClaim(ClaimsPrincipal claimsPrincipal)
        {

            UserId = Convert.ToInt32((from nvp in claimsPrincipal.Claims where(nvp.Type.Equals("sub", StringComparison.OrdinalIgnoreCase) || nvp.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", StringComparison.OrdinalIgnoreCase)) select nvp.Value).FirstOrDefault()); 
			OrganizationPartyId = Convert.ToInt32((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("orgPartyId", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());
			LoginName = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("LOGINNAME", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();
			OrganizationMasterId = Convert.ToInt64((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("ORGMASTERID", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());
			CustomerMasterId = Convert.ToInt64((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("orgCompanyMasterId", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault());
			OrganizationName = (from nvp in claimsPrincipal.Claims where nvp.Type.Equals("ORGNAME", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault();

            Guid realGuid;
            if (Guid.TryParse((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("realPageId", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault(), out realGuid))
			{
				UserRealPageGuid = realGuid;
			}

			Guid correlationId;
            if (Guid.TryParse((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("CORRELATIONID", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault(), out correlationId))
            {    CorrelationId = correlationId;}
	        else
	        {
		        CorrelationId = Guid.NewGuid();
	        }

	        Guid organizationRealPageId;
            if (Guid.TryParse((from nvp in claimsPrincipal.Claims where nvp.Type.Equals("ORGID", StringComparison.OrdinalIgnoreCase) select nvp.Value).FirstOrDefault(), out organizationRealPageId))
			{
				OrganizationRealPageGuid = organizationRealPageId;
			}
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
        /// The books id of the users organization
        /// </summary>
        public long OrganizationMasterId { get; set; }

        /// <summary>
        /// The Bluebook id of the users organization
        /// </summary>
        public long CustomerMasterId { get; set; }
    }
}