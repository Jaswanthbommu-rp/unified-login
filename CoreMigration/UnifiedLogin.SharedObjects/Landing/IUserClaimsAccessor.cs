using System;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.Landing
{
    /// <summary>
    /// Interface for accessing authenticated user claims in a decoupled, testable manner.
    /// This abstraction replaces direct usage of DefaultUserClaim and enables proper dependency injection.
    /// </summary>
    public interface IUserClaimsAccessor
    {
        /// <summary>
        /// The user's id
        /// </summary>
        int UserId { get; }

        /// <summary>
        /// The id used to track the user's actions
        /// </summary>
        Guid CorrelationId { get; }

        /// <summary>
        /// The guid for the user
        /// </summary>
        Guid UserRealPageGuid { get; }

        /// <summary>
        /// The login name for the user
        /// </summary>
        string LoginName { get; }

        /// <summary>
        /// The guid for the user's organization
        /// </summary>
        Guid OrganizationRealPageGuid { get; }

        /// <summary>
        /// The int id of the user's organization
        /// </summary>
        long OrganizationPartyId { get; }

        /// <summary>
        /// The name of the user's organization
        /// </summary>
        string OrganizationName { get; }

        /// <summary>
        /// The name of the user's organization type
        /// </summary>
        string OrganizationType { get; }

        /// <summary>
        /// The books id of the user's organization
        /// </summary>
        long OrganizationMasterId { get; }

        /// <summary>
        /// The Bluebook id of the user's organization
        /// </summary>
        long CustomerMasterId { get; }

        /// <summary>
        /// User roles for the given organization
        /// </summary>
        string Roles { get; }

        /// <summary>
        /// User rights for the given role
        /// </summary>
        List<string> Rights { get; }

        /// <summary>
        /// User First Name
        /// </summary>
        string FirstName { get; }

        /// <summary>
        /// User Last Name
        /// </summary>
        string LastName { get; }

        /// <summary>
        /// ClientCode
        /// </summary>
        string ClientCode { get; }

        /// <summary>
        /// Persona Id
        /// </summary>
        long PersonaId { get; }

        /// <summary>
        /// Used to indicate if the user is a RealPage employee
        /// </summary>
        bool RealPageEmployee { get; }

        /// <summary>
        /// The id used to track the user that is impersonating a user
        /// </summary>
        Guid ImpersonatedBy { get; }

        /// <summary>
        /// The name of the user that is impersonating a user
        /// </summary>
        string ImpersonatedByName { get; }

        /// <summary>
        /// Flag used to determine if RP employee is logged in
        /// </summary>
        bool IsRPEmployee { get; }

        /// <summary>
        /// Gets the complete DefaultUserClaim object for backward compatibility.
        /// This should be used temporarily during migration and eventually removed.
        /// </summary>
        DefaultUserClaim GetUserClaim();
    }
}
