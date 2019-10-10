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
        /// Default constructor
        /// </summary>
        public DefaultUserClaim()
        {
        }

        /// <summary>
        /// Use the passed claim to set the values
        /// </summary>
        /// <param name="claimsPrincipal"></param>
        public DefaultUserClaim(ClaimsPrincipal claimsPrincipal)
        {

            UserId =
                Convert.ToInt32(
                    (from nvp in claimsPrincipal.Claims where nvp.Type == "sub" select nvp.Value).FirstOrDefault());
            OrganizationPartyId =
                Convert.ToInt32(
                    (from nvp in claimsPrincipal.Claims where nvp.Type == "orgPartyId" select nvp.Value)
                        .FirstOrDefault());
            LoginName =
                (from nvp in claimsPrincipal.Claims where nvp.Type.ToUpper() == "LOGINNAME" select nvp.Value)
                    .FirstOrDefault();
            OrganizationMasterId =
                Convert.ToInt64(
                    (from nvp in claimsPrincipal.Claims where nvp.Type.ToUpper() == "ORGMASTERID" select nvp.Value)
                        .FirstOrDefault());
            OrganizationName =
                (from nvp in claimsPrincipal.Claims where nvp.Type.ToUpper() == "ORGNAME" select nvp.Value)
                    .FirstOrDefault();

            Guid realGuid;
            if (
                Guid.TryParse(
                    (from nvp in claimsPrincipal.Claims where nvp.Type == "realPageId" select nvp.Value)
                        .FirstOrDefault(), out realGuid))
                UserRealPageGuid = realGuid;

            Guid correlationId;
            if (
                Guid.TryParse(
                    (from nvp in claimsPrincipal.Claims where nvp.Type.ToUpper() == "CORRELATIONID" select nvp.Value)
                        .FirstOrDefault(), out correlationId))
                CorrelationId = correlationId;

            Guid organizationRealPageId;
            if (
                Guid.TryParse(
                    (from nvp in claimsPrincipal.Claims where nvp.Type.ToUpper() == "ORGID" select nvp.Value)
                        .FirstOrDefault(), out organizationRealPageId))
                OrganizationRealPageGuid = organizationRealPageId;
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
        public string OrganizationName { get; set; }

        /// <summary>
        /// The books id of the users organization
        /// </summary>
        public long OrganizationMasterId { get; set; }
    }
}