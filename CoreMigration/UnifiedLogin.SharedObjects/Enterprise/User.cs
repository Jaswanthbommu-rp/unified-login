using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace UnifiedLogin.SharedObjects.Enterprise
{
    /// <summary>
    /// The user details
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class User
    {
        /// <summary>
        /// The users full name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// The users title
        /// </summary>

        public string Title { get; set; }

        /// <summary>
        /// the users company
        /// </summary>

        public string CompanyName { get; set; }

        /// <summary>
        /// the users realpage id
        /// </summary>

        public Guid RealPageId { get; set; }

        /// <summary>
        /// The users current persona id
        /// </summary>
        public long PersonaId { get; set; }

        /// <summary>
        /// Does the user have multiple companies
        /// </summary>
        public bool HasMultiCompany { get; set; }
        /// <summary>
        /// Does the user have multiple personas
        /// </summary>
        public bool HasMultiPersona { get; set; } = false;

        /// <summary>
        /// User status
        /// </summary>
        public string Status { get; set; }

    }
}
