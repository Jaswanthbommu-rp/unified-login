using Newtonsoft.Json;
using System.Collections.Generic;

namespace UnifiedLogin.SharedObjects.BlackBook
{
    /// <summary>
    /// The object to store BlackBook property information
    /// </summary>
    public class PropertyInstance
    {
        /// <summary>
        /// The id of the site
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Property instance id
        /// </summary>
        public int PropertyInstanceId { get; set; }

        /// <summary>
        /// used in provisioning
        /// </summary>
        public string CustomerEnvironment { get; set; }

        /// <summary>
        /// used in provisioning
        /// </summary>
        [JsonProperty("customerPropertyId", NullValueHandling=NullValueHandling.Ignore)]
        public int? CustomerPropertyId { get; set; }

        /// <summary>
        /// Property instance source id
        /// </summary>
        public string PropertyInstanceSourceId { get; set; }
        /// <summary>
        /// Company instance source id
        /// </summary>
        public string CompanyInstanceSourceId { get; set; }
        /// <summary>
        /// The company relationship type
        /// </summary>
        public string CompanyRelationship { get; set; }
        /// <summary>
        /// Owner instance source id
        /// </summary>
        public string OwnerId { get; set; }
        /// <summary>
        /// Property name
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// Source
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Address
        /// </summary>
        public InstanceAddress Address { get; set; }

        public string State { get; set; }

        //public string YearBuilt { get; set; }

        //public string RenovationStartDate { get; set; }
        //public string RenovationEndDate { get; set; }
        //public int SquareFeet { get; set; }
        /// <summary>
        /// Units
        /// </summary>
        public int Units { get; set; }
        //public string Stories { get; set; }
        /// <summary>
        /// Who created the original record
        /// </summary>
        public string CreatedBy { get; set; }
        /// <summary>
        /// Who last modified the record
        /// </summary>
        public string ModifiedBy { get; set; }
        //public string CreatedAt { get; set; }
        //public string ModifiedAt { get; set; }
        //public string DeletedAt { get; set; }
        //public string PropertyType { get; set; }
        //public string PropertySubType { get; set; }
        //public string GoogleLatitude { get; set; }
        //public string GoogleLongitude { get; set; }
        //public string ConstructionStatus { get; set; }
        //public bool Geocoded { get; set; }
        /// <summary>
        /// Is the Property active
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Used to store various phone numbers for the property
        /// </summary>
        public InstancePhone[] PropertyInstancePhone { get; set; }
        /// <summary>
        /// Used to get Property to Company relationships
        /// </summary>
        public List<CompanyPropertyInstanceMap> CompanyPropertyInstanceMap { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PropertyInstancePartner> PropertyInstancePartners { get; set; }

    }

    public class PropertyInstancePartner
    {
        public string TargetPropertyInstanceSourceId { get; set; }

        public string TargetSource { get; set; }
    }
}
