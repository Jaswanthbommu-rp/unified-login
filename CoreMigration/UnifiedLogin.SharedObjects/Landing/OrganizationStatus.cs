using Newtonsoft.Json;
using System;
using UnifiedLogin.SharedObjects.Enum;

namespace UnifiedLogin.SharedObjects.Landing
{
    public class OrganizationStatus
    {
        private DateTime _fromDate;
        private bool _fromDateNull = true;
        private DateTime _thruDate;
        private bool _thruDateNull = true;
        private DateTime _statusThruDate;
        private bool _statusThruDateNull = true;

        public OrganizationStatus()
        {

        }

        /// <summary>
        /// The unique id for the Organization in GreenBook
        /// </summary>
        [JsonProperty(PropertyName = "PartyId")]
        public long PartyId { get; set; }

        /// <summary>
        /// The unique guid for the Organization in GreenBook
        /// </summary>
        [JsonProperty(PropertyName = "RealPageId")]
        public Guid RealPageId { get; set; }

        /// <summary>
        /// The name of the Organization
        /// </summary>
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// A flag to indicate which company is the users primary authentication
        /// </summary>
        [JsonProperty(PropertyName = "PrimaryOrganization")]
        public bool PrimaryOrganization { get; set; }

        /// <summary>
        /// The status id of the Organization for the user
        /// </summary>
        [JsonProperty(PropertyName = "StatusTypeId")]
        public int StatusTypeId { get; set; }

        public bool? IsPending { get; set; } = false;

        public bool? IsExpired { get; set; } = false;

        public bool? IsActive { get; set; } = false;

        public bool? IsLocked { get; set; } = false;

        public bool? IsForceReSetPassword { get; set; } = false;

        public bool? IsTainted { get; set; } = false;// TBD after MVP

        public UserUiStatusType Status { get; set; } = UserUiStatusType.UnDefined;

        public DateTime FromDate
        {
            get
            {
                return DateTime.SpecifyKind(_fromDate, DateTimeKind.Utc);
            }

            set
            {
                _fromDate = value;
            }
        }

        public DateTime? ThruDate
        {
            get
            {
                if (!_thruDateNull)
                {
                    return DateTime.SpecifyKind(_thruDate, DateTimeKind.Utc);
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
                    _thruDate = value.Value;
                    _thruDateNull = false;
                }
                else
                {
                    _thruDateNull = true;
                }
            }
        }

        /// <summary>
        /// User Status As Of Date time
        /// </summary>
        [JsonProperty(PropertyName = "StatusThruDate")]
        public DateTime? StatusThruDate
        {
            get
            {
                if (!_statusThruDateNull)
                {
                    return DateTime.SpecifyKind(_statusThruDate, DateTimeKind.Utc);
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
                    _statusThruDate = value.Value;
                    _statusThruDateNull = false;
                }
                else
                {
                    _statusThruDateNull = true;
                }
            }
        }

        public void SetOrganizationStatus(bool hasLoggedIn)
        {
            UserUiStatusType status;
            if (System.Enum.TryParse(StatusTypeId.ToString(), true, out status))
            {
                switch (status)
                {
                    case UserUiStatusType.ForceResetPassword:
                        if (StatusThruDate != null && StatusThruDate.Value >= DateTime.UtcNow)
                        {
                            IsForceReSetPassword = true;
                            IsActive = true;
                            IsPending = true;
                            Status = UserUiStatusType.Active;
                            //user never loggedin then set to pending 
                            if (!hasLoggedIn)
                            {
                                Status = UserUiStatusType.Pending;
                            }
                        }
                        else if (StatusThruDate != null && StatusThruDate.Value < DateTime.UtcNow)
                        {
                            IsExpired = true;
                            IsActive = true;
                            Status = UserUiStatusType.Expired;
                        }
                        break;
                    case UserUiStatusType.Pending:
                        if (StatusThruDate != null && StatusThruDate.Value >= DateTime.UtcNow)
                        {
                            IsPending = true;
                            IsActive = true;
                            Status = UserUiStatusType.Pending;
                        }
                        else if (StatusThruDate != null && StatusThruDate.Value < DateTime.UtcNow)
                        {
                            IsExpired = true;
                            IsActive = true;
                            Status = UserUiStatusType.Expired;
                        }
                        break;
                    case UserUiStatusType.Active:
                        if (StatusThruDate == null && FromDate <= DateTime.UtcNow && (ThruDate == null || ThruDate.Value >= DateTime.UtcNow))
                        {
                            IsActive = true;
                            Status = UserUiStatusType.Active;
                        }
                        else if (StatusThruDate == null && FromDate <= DateTime.UtcNow && ThruDate.Value < DateTime.UtcNow)
                        {
                            IsActive = false;
                            Status = UserUiStatusType.Disabled;
                        }
                        break;
                    case UserUiStatusType.Locked:
                        IsLocked = true;
                        IsActive = true;
                        Status = UserUiStatusType.Locked;
                        break;
                    case UserUiStatusType.Disabled:
                        IsActive = false;
                        Status = UserUiStatusType.Disabled;
                        break;
                    case UserUiStatusType.Expired:
                        IsExpired = true;
                        IsActive = true;
                        Status = UserUiStatusType.Expired;
                        break;
                }
            }
        }
    }
}
