using System;

using Newtonsoft.Json;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.Product.EmployeeAccess
{
    
    /// <summary>
    /// Used to determine if the product user is associated to the object
    /// </summary>
    public class UserDetail : IProductUser
    {
        /// <summary>
        /// The id of the product user
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// The login name of the product user
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string UserLogin { get; set; }

        /// <summary>
        /// The users first name
        /// </summary>        
        public string FirstName { get; set; }

        /// <summary>
        /// The users last name
        /// </summary>        
        public string LastName { get; set; }

        /// <summary>
        /// The name of the product user
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The user type of the product user
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// If the user is assigned to the object
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool IsAssigned { get; set; }

        /// <summary>
        /// The id of the company
        /// </summary>
        public long CompanyId { get; set; }

        /// <summary>
        /// The company name of the user
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// The status of the company
        /// </summary>
        public int CompanyStatus { get; set; }

        /// <summary>
        /// third party IDP
        /// </summary>
        public string Name3rdPartyIDP { get; set; }

        /// <summary>
        /// The email id of the user
        /// </summary>
        public string EmailId { get; set; }

        /// <summary>
        /// Realpage Employee User RealPage Id
        /// </summary>
        public string UserRealPageId { get; set; }

        /// <summary>
        /// Company RealPage Id
        /// </summary>
        public string CompanyRealPageId { get; set; }

        /// <summary>
        /// Books Master Id
        /// </summary>
        public long BooksMasterId { get; set; }
        /// <summary>
        /// Persona Id
        /// </summary>
        public long PersonaId { get; set; }
        /// <summary>
        /// User RealPage Id
        /// </summary>
        public Guid PersonaRealPageId { get; set; }
        /// <summary>
        /// User Status
        /// </summary>
        public string UserStatus { get; set; }
    }
}
