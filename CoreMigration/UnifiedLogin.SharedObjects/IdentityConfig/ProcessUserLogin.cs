using Newtonsoft.Json;
using System;
using UnifiedLogin.SharedObjects.Landing;

namespace UnifiedLogin.SharedObjects.IdentityConfig
{
    public class ProcessUserLogin
    {
        private DateTime _fromDate;
        private bool _fromDateNull = true;

        /// <summary>
        /// RealPageId
        /// </summary>
        [JsonProperty(PropertyName = "UserRealPageId")]
        public Guid UserRealPageId { get; set; }

        /// <summary>
        /// Organization RealPageId
        /// </summary>
        [JsonProperty(PropertyName = "OrganizationRealPageId")]
        public Guid OrganizationRealPageId { get; set; }

        /// <summary>
        /// When the account can be used
        /// </summary>
        [JsonProperty(PropertyName = "FromDate")]
        public DateTime? FromDate
        {
            get
            {
                if (!_fromDateNull)
                {
                    return DateTime.SpecifyKind(_fromDate, DateTimeKind.Utc);
                }
                else
                {
                    return null;
                }
            }

            set
            {
                if (value.HasValue)
                {
                    _fromDate = value.Value;
                    _fromDateNull = false;
                }
                else
                {
                    _fromDateNull = true;
                }
            }
        }
    }
}
