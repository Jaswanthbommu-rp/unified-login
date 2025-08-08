using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Shared.Models
{
    // [Serializable]
    [DataContract]
    public class ActivityDetailMessage
    {
        private DateTime _applicationTimestamp;

		[DataMember]
        public long ActivityId { get; set; }

        [DataMember]
        public string LogCategoryName { get; set; }

        [DataMember]
        public string LogActivityTypeName { get; set; }

        [DataMember]
        public string LogCategoryType { get; set; }


        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string FromUserLoginName { get; set; }

        [DataMember]
        public long FromUserLoginId { get; set; }

        [DataMember]
        public string FromUserFirstName { get; set; }

        [DataMember]
        public string FromUserLastName { get; set; }

        [DataMember]
        public Guid FromUserRealpageId { get; set; }

        [DataMember]
        public Guid? ToUserRealpageId { get; set; } 

        [DataMember]
        public long OrganizationPartyId { get; set; }
        
        [DataMember]
        public DateTime ApplicationTimestamp
        {
            get
            {
                return DateTime.SpecifyKind(_applicationTimestamp, DateTimeKind.Utc);
            }

            set
            {
                _applicationTimestamp = DateTime.SpecifyKind(value, DateTimeKind.Utc);
            }
        }

        [DataMember]
        public List<AdditionalParameters> AdditionalInformation { get; set; }

		[DataMember]
		public string ApplicationTimestampOffset { get; set; }

        [DataMember]
        public string ContextId { get; set; }

        [DataMember]
        public int LogActivityTypeId { get; set; }

        [DataMember]
        public string ContextReferenceId { get; set; }
        
        [DataMember]
        public bool IsRealPageEmployee { get; set; }

        [DataMember]
        public string ApplicationCorrelationId { get; set; }

    }
}