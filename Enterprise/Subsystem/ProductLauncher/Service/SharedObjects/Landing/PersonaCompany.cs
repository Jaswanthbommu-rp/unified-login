using System;
using System.Collections.Generic;

namespace RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Landing
{
    /// <summary>
    /// The company persona details
    /// </summary>
    public class PersonaCompany
    {
        /// <summary>
        /// the company name
        /// </summary>

        public string CompanyName { get; set; }

        /// <summary>
        /// the company name
        /// </summary>
        public Guid CompanyRealPageId { get; set; }

        /// <summary>
        /// A list of personas for the company
        /// </summary>
        public List<PersonaCompanyDetails> Personas { get; set; }
    }
}
