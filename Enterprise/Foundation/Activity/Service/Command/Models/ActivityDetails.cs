using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Command.Models
{
    // [Serializable]
    [DataContract]
    public class DerivedActivityDetails
    {
        [DataMember]
        public long ActivityId { get; set; }
        [DataMember]
        public string LogCategoryName { get; set; }
        [DataMember]
        public string LogActivityTypeName { get; set; }
        [DataMember]
        public string CorrelationId { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public long FromUserId { get; set; }
        [DataMember]
        public long? ToUserId { get; set; }
        [DataMember]
        public int OrganizationId { get; set; }
        [DataMember]
        public string ProductName { get; set; }
        [DataMember]
        public string ProductModuleName { get; set; }
        [DataMember]
        public string ProductModuleStepName { get; set; }
        [DataMember]
        public string ServerName { get; set; }
        [DataMember]
        public DateTime Timestamp { get; set; }
        [DataMember]
        public List<AdditionalParameters> AdditionalInformation { get; set; }
        [DataMember]
        public bool IsRealPageEmployee { get; set; }
    }
}